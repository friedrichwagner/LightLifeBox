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
using LightLife.Data;


namespace LightLifeAdminConsole
{
    public enum TestSequenceState { NONE=0, IN_PROGRESS=10, TESTING=20, FADING_OUT=30, PAUSED=80, STOPPED=90, FINISHED=99}; //identisch Tabelle LLTestSequenceState
    public enum TestSequenceStep { STOPPED=0, BRIGHTNESS, CCT, JUDD, ALL, ALL_BIG, DELTATEST }; // identisch Tabelle LLStep
    public enum TestSequenceActivation { NONE=0, ACTIVATING=1, RELAXING=2 }; // identisch Tabelle LLStep
    public enum CommandSender { GUI=0, CONTROLBOX=1 }; // identisch Tabelle LLStep

    public enum TestSequenceCommand { SAVENEW, REFRESH, POSUPDATE, STATECHANGED, START, PREV, GOTO, NEXT, PAUSE, STOP, FINISH };

    public delegate void TestSequenceEventDelegate(int SequenceID, int PosID, TestSequenceCommand seqCmd);

    class LLTestSequence
    {
        //private const int DEFAULT_CCT = 4000; //K
        //private const int DEFAULT_BRIGHTNESS = 50; //255:2
        public TestSequenceEventDelegate TestSequenceEvent;

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
        public int MinPosID { get { return _MinPosID;} }

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

        private AdminBase _head;
        private AdminBase _pos;
        private AdminBase _posView;
        private AdminBase _def;

        public DataTable posView { get; private set; }

        private int _boxnr;

        public LLTestSequence(int boxnr, int seqid)
        {
            _boxnr = boxnr;
            _head = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequenceHead"]);
            _pos = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequencePos"]);
            _def = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequenceDefinition"]);
            _posView = new AdminBase(LLSQL.sqlCon, LLSQL.tables["VLLTestSequence"]);

            DataTable dt1 = getSequence(boxnr, seqid);
            
            InitSequence(dt1);
        }

        public LLTestSequence(int boxnr, int seqid, int ProbandID)
        {
            SequenceID = getUserSequence(ProbandID);

            if (SequenceID > 0)
            {
                SetBoxNrtoSequence(SequenceID, boxnr);

                _boxnr = boxnr;
                _head = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequenceHead"]);
                _pos = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequencePos"]);
                _def = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequenceDefinition"]);
                _posView = new AdminBase(LLSQL.sqlCon, LLSQL.tables["VLLTestSequence"]);

                DataTable dt1 = getSequence(boxnr, SequenceID);

                InitSequence(dt1);
            }
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
                posView = _posView.select(" where SequenceID=" + SequenceID.ToString() + " order by PosID");
            }
        }


        //FW 10.6.2015 - obsolet
        /*public void SendSequenceStep()
        {
            string Params = ";userid=" + ProbandID + ";vlid=" + Box2.VLID + ";cycleid=" + CycleID + ";sequenceid=" + SequenceID
                            + ";posid=" + PosID + ";stepid=" + ((int)StepID).ToString() + ";activationstate=" + ActivationID + ";msgtype=" + ((int)LLMsgType.LL_SET_SEQUENCEDATA).ToString();
            Box2.boxes[_boxnr].rCmd.SetSequence(Params);
        }*/

        #region Sequence - Head

        public int SaveNew()
        {
            CreateSequence();
            State = TestSequenceState.NONE;

            RaiseEvent(TestSequenceCommand.SAVENEW);

            return SequenceID;
        }

        public void SaveALL()
        {
            for (int i=1; i<=24; i++)
            {
                _boxnr = 0;
                switch (i)
                {
                    case 1: ActivationID=1; _SequenceDef = "BFJa"; break;
                    case 2: ActivationID = 2; _SequenceDef = "BFJa"; break;

                    case 3: ActivationID = 1; _SequenceDef = "BJFa"; break;
                    case 4: ActivationID = 2; _SequenceDef = "BJFa"; break;

                    case 5: ActivationID = 1; _SequenceDef = "FBJa"; break;
                    case 6: ActivationID = 2; _SequenceDef = "FBJa"; break;

                    case 7: ActivationID = 1; _SequenceDef = "FJBa"; break;
                    case 8: ActivationID = 2; _SequenceDef = "FJBa"; break;

                        //9,10, falsch im Excel ?
                    case 9: ActivationID = 1; _SequenceDef = "JBFa"; break;
                    case 10: ActivationID = 2; _SequenceDef = "JBFa"; break;

                    case 11: ActivationID = 1; _SequenceDef = "JFBa"; break;
                    case 12: ActivationID = 2; _SequenceDef = "JFBa"; break;

                    //---------------------------------------------------------
                    case 13: ActivationID = 1; _SequenceDef = "BFJa"; break;
                    case 14: ActivationID = 2; _SequenceDef = "BFJa"; break;

                    case 15: ActivationID = 1; _SequenceDef = "BJFa"; break;
                    case 16: ActivationID = 2; _SequenceDef = "BJFa"; break;

                    case 17: ActivationID = 1; _SequenceDef = "FBJa"; break;
                    case 18: ActivationID = 2; _SequenceDef = "FBJa"; break;

                    case 19: ActivationID = 1; _SequenceDef = "FJBa"; break;
                    case 20: ActivationID = 2; _SequenceDef = "FJBa"; break;

                    case 21: ActivationID = 1; _SequenceDef = "JBFa"; break;
                    case 22: ActivationID = 2; _SequenceDef = "JBFa"; break;

                    case 23: ActivationID = 1; _SequenceDef = "JFBa"; break;
                    case 24: ActivationID = 2; _SequenceDef = "JFBa"; break;


                }

                for (int k = 1; k <= 8;k++ )
                {
                    ProbandID = 100 * k + i;
                    CreateSequence();
                }
                    
            }
        }

        private void RaiseEvent(TestSequenceCommand cmd)
        {
            if (TestSequenceEvent != null)
            {
                TestSequenceEvent(SequenceID, PosID, cmd);
            }
        }

        public void Refresh()
        {
            DataTable dt1 = getSequence(_boxnr, SequenceID);
            InitSequence(dt1);

            RaiseEvent(TestSequenceCommand.REFRESH);
        }

        public int Start()
        {
            State = TestSequenceState.TESTING;

            //SendSequenceStep(); System.Threading.Thread.Sleep(RemoteCommandBase.WAIT_TIME);
            Box2.boxes[_boxnr].CBoxSetPILed(PILEDMode.SET_CCT, LightLifeData.DEFAULT_BRIGHTNESS, LightLifeData.DEFAULT_CCT, LightLifeData.DEFAULT_NEOLINK_FADETIME); System.Threading.Thread.Sleep(RemoteCommandBase.WAIT_TIME);
            Box2.boxes[_boxnr].CBoxEnableBoxButtons(EnabledButtons, Box2.ALL_BUTTONS_DISABLED);

            UpdateHeadState();

            RaiseEvent(TestSequenceCommand.START);

            return SequenceID;
        }

        public void Stop()
        {
            State = TestSequenceState.STOPPED;
            Box2.boxes[_boxnr].CBoxSetPILed(PILEDMode.SET_CCT, LightLifeData.DEFAULT_BRIGHTNESS, LightLifeData.DEFAULT_CCT, LightLifeData.DEFAULT_NEOLINK_FADETIME); System.Threading.Thread.Sleep(RemoteCommandBase.WAIT_TIME);
            Box2.boxes[_boxnr].CBoxEnableBoxButtons(Box2.ALL_BUTTONS_DISABLED, Box2.ALL_BUTTONS_DISABLED);

            UpdateHeadState();

            RaiseEvent(TestSequenceCommand.STOP);
        }

        public void Finish()
        {
            State = TestSequenceState.FINISHED;

            UpdateHeadState();

            RaiseEvent(TestSequenceCommand.FINISH);
        }

        public void Pause()
        {
            State = TestSequenceState.PAUSED;
            Box2.boxes[_boxnr].CBoxEnableBoxButtons(Box2.ALL_BUTTONS_DISABLED, Box2.ALL_BUTTONS_DISABLED);
            RaiseEvent(TestSequenceCommand.PAUSE);
            UpdateHeadState();
        }

        public bool Prev(CommandSender sender)
        {
            if (PosID > _MinPosID)
            {
                PosID--;
                getPos();

                //SendSequenceStep(); System.Threading.Thread.Sleep(RemoteCommandBase.WAIT_TIME);

                if (sender == CommandSender.GUI)
                {
                    State = TestSequenceState.TESTING;
                    Box2.boxes[_boxnr].CBoxEnableBoxButtons(EnabledButtons, Box2.ALL_BUTTONS_DISABLED);
                }
                else
                    State = TestSequenceState.FADING_OUT;

                RaiseEvent(TestSequenceCommand.PREV);
                UpdateHeadState();
                return true;
            }

            return false;
        }

        public bool Next(CommandSender sender)
        {                        
            if (PosID < _MaxPosID)
            {
                PosID++;
                getPos();

                //SendSequenceStep(); System.Threading.Thread.Sleep(RemoteCommandBase.WAIT_TIME);
               

                //Wenn Cmd von GUI kommt, dann gleich mit Test starten
                //wenn Cmd von ControlBox --> Fading 30sec, dann starten
                if (sender == CommandSender.GUI)
                {
                    State = TestSequenceState.TESTING;
                    Box2.boxes[_boxnr].CBoxEnableBoxButtons(EnabledButtons, Box2.ALL_BUTTONS_DISABLED);
                }
                else
                    State = TestSequenceState.FADING_OUT;
                
               
                RaiseEvent(TestSequenceCommand.NEXT);               
                UpdateHeadState();

                return true;
            }

            return false;
        }

        public bool Goto(CommandSender sender, int posid)
        {
            if ( (posid >= _MinPosID) && (posid <= _MaxPosID ) )
            {
                PosID = posid;
                getPos();

                //FW 23.5. Hier nur Feld ActualPos in DB setzen. User muss dann wieder START drücken
                /*SendSequenceStep(); System.Threading.Thread.Sleep(RemoteCommandBase.WAIT_TIME);

                if (sender == CommandSender.GUI)
                {
                    State = TestSequenceState.TESTING;
                    Box2.boxes[_boxnr].CBoxEnableBoxButtons(EnabledButtons, Box2.ALL_BUTTONS_DISABLED);
                }
                else
                    State = TestSequenceState.FADING_OUT;*/

                RaiseEvent(TestSequenceCommand.GOTO);
                UpdateHeadState();
                return true;
            }

            return false;
        }


        private void CreateSequence()
        {
            try
            {
                SqlTransaction tran = LLSQL.sqlCon.BeginTransaction();
                LLSQL.cmd.Transaction = tran;

                SequenceID = getNextHeadID();
                _head.Transaction = tran;
                _head.insert(new string[] { SequenceID.ToString(),_SequenceDef, _boxnr.ToString(), ProbandID.ToString(), "0", ((int)TestSequenceState.NONE).ToString(), "0", String.Empty });

                int firstpos = CreateSequencePos(tran);

                _head.update("", new string[] { ((int)TestSequenceState.NONE).ToString(), firstpos.ToString(), SequenceID.ToString() });

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
            _def.Transaction = tran;
            DataTable dt1 = _def.select(" where SequenceDef='" + _SequenceDef + "'");
            if (dt1.Rows.Count == 1)
            {
                int ActivationID2 = (int)TestSequenceActivation.ACTIVATING;
                if (ActivationID == (int)TestSequenceActivation.ACTIVATING) ActivationID2 = (int)TestSequenceActivation.RELAXING;

                _pos.Transaction = tran;
                //Zyklus 1 - Aktivierend oder Entspannend
                _pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID1").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID2").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID3").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                //pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", Box2.DEFAULT_BRIGHTNESS.ToString(), Box2.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });

                //Zyklus 1 - Aktivierend oder Entspannend (gegenteil von oben)  
                _pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID1").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID2").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID3").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                //pos.insert(new string[] { SequenceID.ToString(), "1", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", Box2.DEFAULT_BRIGHTNESS.ToString(), Box2.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });

                //Zyklus 2 - Aktivierend oder Entspannend
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID1").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID2").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID3").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                //Hier im grossen Raum, nochmal ALLE
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), ((int)TestSequenceStep.ALL_BIG).ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                //pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", Box2.DEFAULT_BRIGHTNESS.ToString(), Box2.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });

                //Zyklus 2 - Aktivierend oder Entspannend (gegenteil von oben)  
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID1").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID2").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID3").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                //Hier im grossen Raum, nochmal ALLE
                _pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), ((int)TestSequenceStep.ALL_BIG).ToString(), "0", LightLifeData.DEFAULT_BRIGHTNESS.ToString(), LightLifeData.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                //pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", Box2.DEFAULT_BRIGHTNESS.ToString(), Box2.DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
            }

            _posView.Transaction = tran;
            return getPos(true);
        }

        private DataTable getSequence(int boxnr, int seqid)
        {
            string filter = String.Empty;
            if (seqid > 0)
                filter = " where BoxID=:1 and SequenceID=" + seqid.ToString();
            else if (seqid < 0) //letzte holen
            {
                //filter = " where BoxID=:1 and SequenceID = (select max(SequenceID) from LLTestSequenceHead where boxid=:1 and TestStateID < " + ((int)TestSequenceState.FINISHED).ToString() + ") ";
                filter = " where BoxID=:1 and SequenceID = (select max(SequenceID) from LLTestSequenceHead where boxid=:1) ";
            }
            else //neue Sequeunce
            {
                filter = " where BoxID=:1 and 1=0";
            }

            DataTable dt1 = _head.execQuery(LLSQL.tables["LLTestSequenceHead"].selectSQL, new string[] { boxnr.ToString() }, filter);
            return dt1;
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

        public void UpdateRemarkPos(int posid, string txt)
        {
            string stmt = "update LLTestSequencePos set remark='" + txt + "' where PosID=" + posid.ToString();
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();
        }

        public void SetTestSequenceState(TestSequenceState s)
        {
            State = s;
            UpdateHeadState();

            RaiseEvent(TestSequenceCommand.STATECHANGED);
        }

        private void UpdateHeadState()
        {
            string stmt = "update LLTestSequenceHead set TestStateID='" + ((int)State).ToString() + "', ActualPosID=" + PosID.ToString() + " where sequenceID=" + SequenceID.ToString();
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();
        }

        private int getUserSequence(int ProbandID)
        {
            int ret=-1;
            string stmt = "select SequenceID from LLTestSequenceHead where UserID=" + ProbandID.ToString() + " order by SequenceID desc";
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();

            if (LLSQL.cmd.dr.Read())
                ret = LLSQL.cmd.dr.GetInt32(0);

            LLSQL.cmd.dr.Close();

            return ret;
        }

        private void SetBoxNrtoSequence(int SeqId, int boxnr)
        {
            string stmt = "update LLTestSequenceHead set BoxID=" + boxnr.ToString() + " where SequenceID=" + SeqId.ToString();
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();
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
            DataTable dt;

            if (firstpos)
            {
                dt = _pos.select(" where SequenceID=" + SequenceID.ToString()+" order by PosID");
            }
            else
            {
                dt = _pos.select(" where PosID=" + PosID.ToString());
                //else throw new ArgumentException("No TestSequence Position found!");
            }

            if (dt.Rows.Count > 0)
            {
                CycleID = dt.Rows[0].Field<int>("CycleID");
                ActivationID = dt.Rows[0].Field<int>("ActivationID");
                PosID = dt.Rows[0].Field<int>("PosID");
                StepID = (TestSequenceStep)dt.Rows[0].Field<int>("StepID");

                ret = PosID;

                posView = _posView.select(" where SequenceID=" + SequenceID.ToString() + " order by PosID"); 
            }            

            return ret;
        }

        public void UpdatePos(string Params)
        {
            string[] sarr = Params.Split(';');
            Dictionary<string, string> dtmp = sarr.Select(item => item.Split('=')).ToDictionary(s => s[0], s => s[1]);
            MyDictionary d = new MyDictionary(dtmp);

            _pos.update("where PosID="+PosID.ToString(), d);
            posView = _posView.select(" where SequenceID=" + SequenceID.ToString() + " order by PosID");

            RaiseEvent(TestSequenceCommand.POSUPDATE);
        }

        #endregion

    }
}
