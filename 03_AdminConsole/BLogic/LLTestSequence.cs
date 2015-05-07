﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightLifeAdminConsole.Data;
using System.Data;
using System.Data.SqlClient;


namespace LightLifeAdminConsole
{
    public enum TestSequenceState { NONE=0, IN_PROGRESS=10, TESTING=20, FADING_OUT=30, PAUSED=40, STOPPED=90, FINISHED=99}; //identisch Tabelle LLTestSequenceState
    public enum TestSequenceStep { STOPPED, BRIGHTNESS, CCT, JUDD, ALL, ALL_BIG }; // identisch Tabelle LLStep

    class LLTestSequence
    {
        private const int DEFAULT_CCT = 4000; //K
        private const int DEFAULT_BRIGHTNESS = 125; //255:2

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
            else
                filter = " where BoxID=:1 and SequenceID = (select max(SequenceID) from LLTestSequenceHead where boxid=:1) ";

            DataTable dt1 = head.execQuery(LLSQL.tables["LLTestSequenceHead"].selectSQL, new string[] { boxnr.ToString() }, filter);
            

            InitSequence(dt1);
        }

        private void InitSequence(DataTable dt)
        {
            if (dt.Rows.Count >= 1)
            {
                SequenceID = dt.Rows[0].Field<int>("SequenceID");
                ProbandID = dt.Rows[0].Field<int>("UserID");
                StepID = (TestSequenceStep)dt.Rows[0].Field<int>("ActualPosID");
                Remark = dt.Rows[0].Field<string>("Remark");
                State = (TestSequenceState)dt.Rows[0].Field<int>("TestStateID");

                /*if (IsActive)
                {
                    int brightness = 0;
                    if (State == TestSequenceState.STARTED) brightness = 100;
                    UpdateVarious(LLMsgType.LL_RELOAD_TESTSEQUENCE, brightness);
                }*/
            }
            else
            {
                StepID = TestSequenceStep.STOPPED;
                Remark = String.Empty;
            }
        }

        #region Sequence - Head

        public int SaveNewSequence()
        {
            CreateSequence();
            State = TestSequenceState.NONE;

            return SequenceID;
        }

        public int StartSequence()
        {
            if (!CanStart)
                throw new ArgumentException("Testsquenz nicht gestoppt!");

            State = TestSequenceState.IN_PROGRESS;
            UpdateHeadState();

            return SequenceID;
        }

        public bool CanStart
        {
            get
            {
                if ((State == TestSequenceState.STOPPED) || (State == TestSequenceState.PAUSED) || (State == TestSequenceState.NONE) || (State == TestSequenceState.FINISHED)) return true;
                return false;
            }
        }

        public void Stop()
        {
            if (!CanStop)            
                throw new ArgumentException("Testsquenz nicht gestartet!");
            State = TestSequenceState.STOPPED;
            //UpdateVarious(LLMsgType.LL_STOP_TESTSEQUENCE, 0);
            UpdateHeadState();
        }

        public bool CanStop
        {
            get
            {
                if ((State == TestSequenceState.IN_PROGRESS) || (State == TestSequenceState.TESTING) || (State == TestSequenceState.FADING_OUT) || (State == TestSequenceState.PAUSED)) return true;
                return false;
            }
        }

        public void Pause()
        {
            if (!CanPause)
                throw new ArgumentException("Testsquenz nicht gestartet!");

            State = TestSequenceState.PAUSED;
            //UpdateVarious(LLMsgType.LL_PAUSE_TESTSEQUENCE, 0);
            UpdateHeadState();
        }

        public bool CanPause
        {
            get
            {
                if ((State == TestSequenceState.IN_PROGRESS) || (State == TestSequenceState.TESTING) || (State == TestSequenceState.FADING_OUT)) return true;
                return false;
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

                CreateSequencePos(tran);

                LLSQL.cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                LLSQL.cmd.Transaction.Rollback();
                throw ex;
            }
        }

        private void CreateSequencePos(SqlTransaction tran)
        {
            def.Transaction = tran;
            DataTable dt1 = def.select("where SequenceDef='" + _SequenceDef + "'");
            if (dt1.Rows.Count == 1)
            {
                int ActivationID2 = 0;

                if (ActivationID == 0) ActivationID2 = 1;
                else ActivationID2 = 0;

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
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });

                //Zyklus 2 - Aktivierend oder Entspannend (gegenteil von oben)  
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID1").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID2").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID3").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                //Hier im grossen Raum, nochmal ALLE
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID.ToString(), dt1.Rows[0].Field<int>("StepID4").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
                pos.insert(new string[] { SequenceID.ToString(), "2", ActivationID2.ToString(), dt1.Rows[0].Field<int>("StepID5").ToString(), "0", DEFAULT_BRIGHTNESS.ToString(), DEFAULT_CCT.ToString(), "0.0", "0.0", "0.0", string.Empty });
            }
        }



        private void InsertPos()
        {
            

            DataTable dt = pos.select(" where SequenceID=:1 and ActivationID=:2 and StepID=:3 order by PosID desc");
            if (dt.Rows.Count > 0) PosID = dt.Rows[0].Field<int>("PosID");
            else throw new ArgumentException("No TestSequence Position found!");
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

        #endregion Sequence

        #region Sequence - Pos(Step)


        public void StartStep()
        {
            InsertStep();

            State = TestSequenceState.IN_PROGRESS;

            UpdateHeadState();
        }


        public void UpdateStep()
        {

        }

        public bool checkStepExists()
        {
            DataTable dt = pos.execQuery(" where SequenceID=:1 and ActivationID=:2 and StepID=:3", new string[] { SequenceID.ToString(), ActivationID.ToString(), StepID.ToString() }, "");

            if (dt.Rows.Count > 0) return true;
            return false;
        }

        #endregion

    }
}
