using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightLifeAdminConsole.Data;
using System.Data;
using System.Data.SqlClient;
using LightLifeAdminConsole.MVVM;
using Lumitech.Helpers;


namespace LightLifeAdminConsole
{
    public enum TestSequenceState { NONE=0, IN_PROGRESS=10, TESTING=20, FADING_OUT=30, PAUSED=40, STOPPED=90, FINISHED=99}; //identisch Tabelle LLTestSequenceState
    public enum TestSequenceStep { STOPPED, BRIGHTNESS, CCT, JUDD, ALL, ALL_BIG }; // identisch Tabelle LLStep
    public enum TestSequenceActivation { NONE=0, ACTIVATING=1, RELAXING=2 }; // identisch Tabelle LLStep

    class LLTestSequence
    {
        private const int DEFAULT_CCT = 4000; //K
        private const int DEFAULT_BRIGHTNESS = 50; //255:2

        public int SequenceID { get; private set; }

        private string _SequenceDef;
        public string SequenceDef 
        { 
            get { return _SequenceDef;} 
            set {
                string name;
                if (!LLSQL.lltestsequencedef.TryGetValue(value, out name))
                {
                    throw new ArgumentOutOfRangeException("Sequence definition not found:" + value);
                }
                else
                    _SequenceDef = value;
            }
        }

        public int PosID { get; private set; } 
        public int CycleID { get; private set; }
        public int ActivationID { get; set; }

        private int _MinPosID;
        private int _MaxPosID;

        private TestSequenceStep _stepID;
        public TestSequenceStep StepID {
            get { return _stepID; }
            set{
                    DataRow[] dr = LLSQL.llstep.Select("Stepid=" + (int)value);                
                    if (dr.Length==1)
                    {
                        _stepID = value;
                        EnabledButtons = dr[0].Field<string>("EnabledButtons");
                    }
                    else
                        EnabledButtons = "00000";
            } 
        }

        public string EnabledButtons { get; private set; }
        public int ProbandID { get; set; }
        public string Remark { get; set; }
        public TestSequenceState State { get; private set; }  

        private AdminBase head;
        private AdminBase pos;
        private AdminBase def;

        private int _boxnr;

        public LLTestSequence(int boxnr, int seqid)
        {
            _boxnr = boxnr;
            head = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequenceHead"]);
            pos = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequencePos"]);
            def = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequenceDefinition"]);           

            string filter=String.Empty;
            if (seqid > 0)
                filter = " where BoxID=:1 and SequenceID=" + seqid.ToString();
            else if (seqid < 0) //letzte holen
                filter = " where BoxID=:1 and SequenceID = (select max(SequenceID) from LLTestSequenceHead where boxid=:1 and TestStateID < " + ((int)TestSequenceState.FINISHED).ToString() + ") ";
            else //neue Sequeunce
            {
                filter = " where BoxID=:1 and 1=0";
            }

            DataTable dt1 = head.execQuery(LLSQL.tables["LLTestSequenceHead"].selectSQL, new string[] { boxnr.ToString()}, filter);
            
            InitSequence(dt1);
        }

        private void InitSequence(DataTable dt)
        {
            if (dt.Rows.Count >= 1)
            {
                SequenceID = dt.Rows[0].Field<int>("SequenceID");
                ProbandID = dt.Rows[0].Field<int>("UserID");
                PosID = dt.Rows[0].Field<int>("ActualPosID");
                //StepID = (TestSequenceStep)dt.Rows[0].Field<int>("ActualPosID");
                Remark = dt.Rows[0].Field<string>("Remark");
                State = (TestSequenceState)dt.Rows[0].Field<int>("TestStateID");
                SequenceDef = dt.Rows[0].Field<string>("SequenceDef");
                ActivationID = (int)TestSequenceActivation.NONE;

                getMinMaxPosId();
                getPos();
            }
            else
            {
                StepID = TestSequenceStep.STOPPED;
                Remark = String.Empty;
                SequenceDef = String.Empty;
            }
        }

        #region Sequence - Head

        public int SaveNew()
        {
            CreateSequence();
            State = TestSequenceState.NONE;

            return SequenceID;
        }

        public int Start()
        {
            State = TestSequenceState.IN_PROGRESS;
            UpdateHeadState();

            return SequenceID;
        }

        public void Stop()
        {
            State = TestSequenceState.STOPPED;
            //UpdateVarious(LLMsgType.LL_STOP_TESTSEQUENCE, 0);
            UpdateHeadState();
        }

        public void Pause()
        {
            State = TestSequenceState.PAUSED;
            UpdateHeadState();
        }

        public void Prev()
        {
            State = TestSequenceState.IN_PROGRESS;
            if (PosID > _MinPosID)
            {
                PosID--;
                getPos();
                UpdateHeadState();
            }
        }

        public void Next()
        {
            State = TestSequenceState.IN_PROGRESS;
            if (PosID < _MinPosID)
            {
                PosID++;
                getPos();
                UpdateHeadState();
            }
        }

        private void CreateSequence()
        {
            try
            {
                SqlTransaction tran = LLSQL.sqlCon.BeginTransaction();
                LLSQL.cmd.Transaction = tran;

                SequenceID = getNextHeadID();
                head.Transaction = tran;
                head.insert(new string[] { SequenceID.ToString(),_SequenceDef, _boxnr.ToString(), ProbandID.ToString(), "0", ((int)TestSequenceState.NONE).ToString(), "0", String.Empty });

                int firstpos = CreateSequencePos(tran);

                head.update("", new string[] { ((int)TestSequenceState.NONE).ToString(), firstpos.ToString(), SequenceID.ToString() });

                LLSQL.cmd.Transaction.Commit();

                getMinMaxPosId();
            }
            catch (Exception ex)
            {
                LLSQL.cmd.Transaction.Rollback();
                throw ex;
            }
        }

        private int  CreateSequencePos(SqlTransaction tran)
        {
            def.Transaction = tran;
            DataTable dt1 = def.select(" where SequenceDef='" + _SequenceDef + "'");
            if (dt1.Rows.Count == 1)
            {
                int ActivationID2 = (int)TestSequenceActivation.ACTIVATING;
                if (ActivationID == (int)TestSequenceActivation.ACTIVATING) ActivationID2 = (int)TestSequenceActivation.RELAXING;

                pos.Transaction = tran;
                //Zyklus 1 - Aktivierend oder Entspannend
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(),  dt1.Rows[0].Field<int>("StepID1").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID2").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID3").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });

                //Zyklus 1 - Aktivierend oder Entspannend (gegenteil von oben)  
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID1").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID2").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID3").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });

                //Zyklus 2 - Aktivierend oder Entspannend
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID1").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID2").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID3").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                //Hier im grossen Raum, nochmal ALLE
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), ((int)TestSequenceStep.ALL_BIG).ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });

                //Zyklus 2 - Aktivierend oder Entspannend (gegenteil von oben)  
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID1").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID2").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID3").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                //Hier im grossen Raum, nochmal ALLE
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), ((int)TestSequenceStep.ALL_BIG).ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
            }

            return getPos(true);
        }


        private int getNextHeadID()
        {
            string stmt = "select IsNull(Max(SequenceID)+1,1) from LLTestSequenceHead";
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();

            LLSQL.cmd.dr.Read(); //Es muss einen geben
            int ret = LLSQL.cmd.dr.GetInt32(0);
            LLSQL.cmd.dr.Close();
            return ret;
        }

        public void UpdateRemark(string txt)
        {
            string stmt = "update LLTestSequenceHead set remark='" + txt + "' where sequenceID=" + SequenceID.ToString();
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();
        }

        private void UpdateHeadState()
        {
            string stmt = "update LLTestSequenceHead set TestStateID='" + State + "', ActualPosID=" + PosID.ToString() + " where sequenceID=" + SequenceID.ToString();
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();
        }

        public bool btnEnabled(BoxUIButtons btn)
        {
            switch (btn)
            {
                case BoxUIButtons.START:
                    if ((State == TestSequenceState.STOPPED) || (State == TestSequenceState.PAUSED) || (State == TestSequenceState.NONE)) return true;
                    break;

                case BoxUIButtons.STOP:
                    if ((State == TestSequenceState.IN_PROGRESS) || (State == TestSequenceState.TESTING) || (State == TestSequenceState.FADING_OUT) || (State == TestSequenceState.PAUSED)) return true;
                    break;

                case BoxUIButtons.PREV:
                    if ((State == TestSequenceState.IN_PROGRESS) || (State == TestSequenceState.TESTING) || (State == TestSequenceState.FADING_OUT) || (State == TestSequenceState.PAUSED) && (PosID > 1)) return true;
                    break;

                case BoxUIButtons.NEXT:
                    if ((State == TestSequenceState.IN_PROGRESS) || (State == TestSequenceState.TESTING) || (State == TestSequenceState.FADING_OUT) || (State == TestSequenceState.PAUSED) && (PosID < 22)) return true;
                    break;

                case BoxUIButtons.PAUSE:
                    if ((State == TestSequenceState.IN_PROGRESS) || (State == TestSequenceState.TESTING) || (State == TestSequenceState.FADING_OUT)) return true;
                    break;

                case BoxUIButtons.UPDATE:
                    //if (boxes[SelectedBox].State == BoxStatus.STARTED) return true; 
                    //else 
                    //kann immer upgedated werden
                    return true;

                case BoxUIButtons.SAVENEW:
                    //if ((State == TestSequenceState.STOPPED) || (State == TestSequenceState.NONE) || (State == TestSequenceState.FINISHED)) return true;
                    if ((SequenceID <= 0) && (ProbandID > 0) && (ActivationID > 0) && (SequenceDef.Length > 0)) return true;
                    break;
            }

            return false;
        }

        #endregion Sequence

        #region Sequence - Pos(Step)

        private void getMinMaxPosId()
        {
            string stmt = "select IsNull(min(POSID),0) , IsNull(max(PosID),0) Maxi from LLTestSequencePos where SequenceID=" + SequenceID.ToString();
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();

            if (LLSQL.cmd.dr.Read())
            {
                _MinPosID = LLSQL.cmd.dr.GetInt32(0);
                _MaxPosID = LLSQL.cmd.dr.GetInt32(1);
            }
            LLSQL.cmd.dr.Close();
        }

        private int getPos(bool firstpos=false)
        {
            int ret = 0;

            if (firstpos)
            {
                DataTable dt = pos.select(" where SequenceID=" + SequenceID.ToString()+" order by PosID");
                if (dt.Rows.Count > 0)
                {
                    ret = PosID = dt.Rows[0].Field<int>("PosID");
                }
            }
            else
            {
                DataTable dt = pos.select(" where PosID=" + PosID.ToString());
                if (dt.Rows.Count > 0)
                {
                    CycleID = dt.Rows[0].Field<int>("CycleID");
                    ActivationID = dt.Rows[0].Field<int>("ActivationID");
                    PosID = dt.Rows[0].Field<int>("PosID");
                    StepID = (TestSequenceStep)dt.Rows[0].Field<int>("StepID");

                    ret = PosID;
                }
                else throw new ArgumentException("No TestSequence Position found!");
            }

            return ret;
        }

        public void UpdatePos(string Params)
        {
            string[] sarr = Params.Split(';');
            Dictionary<string, string> dtmp = sarr.Select(item => item.Split('=')).ToDictionary(s => s[0], s => s[1]);
            MyDictionary d = new MyDictionary(dtmp);

            pos.update("where PosID="+PosID.ToString(), d);
        }
        #endregion

    }
}
