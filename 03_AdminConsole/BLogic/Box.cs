using LightLifeAdminConsole.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LightLifeGlobalDefines;

namespace LightLifeAdminConsole
{
    public enum BoxStatus { NONE, STARTED, STOPPED, PAUSED, FINISHED};
    public enum TestSequence { NONE, BRIGHTNESS, CCT, JUDD, ALL };
    public enum BoxPotis : byte { BRIGHTNESS, CCT, JUDD };

    class Box
    {
        private const byte NUM_POTIS = 3;
        private const byte NUM_BUTTONS = 2;

        public int BoxNr { get; private set; }
        public int ProbandID { get; set; }
        public BoxStatus State { get; set; }
        public IPAddress BoxIP { get; private set; }
        public bool IsActive { get; private set; }

        public int SequenceID { get; private set; }
        public string Remark { get; set; }
        
        public IList<TestSequence> TestSequenceOder { get; private set; }
        public int StepID { get; private set; }

        private AdminBase head;
        private AdminBase pos;
        private AdminBase boxdata;
        private LLRemoteCommand rCmd;

        private byte[] PotisActive = new byte[NUM_POTIS];
        private byte[] ButtonsActive = new byte[NUM_BUTTONS];

        public Box(int boxnr)
        {
            InitBox(boxnr);
        }

        public Box(DataTable dt)
        {
            InitBox(dt.Rows[0].Field<int>("BoxID"));

            SequenceID = dt.Rows[0].Field<int>("SequenceID");           
            ProbandID = dt.Rows[0].Field<int>("UserID");
            State = GetStatefromString(dt.Rows[0].Field<string>("Status"));
            StepID = dt.Rows[0].Field<int>("ActualStep");
            Remark = dt.Rows[0].Field<string>("Remark");
 
            GetTestSequenceOrder();
        }

        private void InitBox(int boxnr)
        {
            BoxNr = boxnr;
            ProbandID = -1;
            State = BoxStatus.NONE;
            SequenceID = -1;
            Remark = String.Empty;
            StepID = 0;
            IsActive = false;
            BoxIP = IPAddress.Loopback;
            TestSequenceOder = new List<TestSequence>(4);

            head = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequenceHead"]);
            pos = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequencePos"]);
            boxdata = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLBox"]);
            DataTable dt = boxdata.select("where boxID=" + BoxNr.ToString());


            if (dt.Rows.Count > 0)
            {
                IsActive = (dt.Rows[0].Field<int>("active") == 1) ? true : false;
                BoxIP = IPAddress.Parse(dt.Rows[0].Field<string>("BoxIP"));

                rCmd = new LLRemoteCommand(BoxIP, (int)LightLifePorts.CONTROL_BOX_LISTEN_UDP, (int)LightLifePorts.ADMIN_CONSOL_LISTEN_UDP);
                rCmd.bAsync = false;
                IsActive = rCmd.Ping();
            }

            setPotisActive(TestSequence.NONE);
        }

        public static Box ReloadSequence(int id)
        {
            AdminBase head = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequenceHead"]);
            DataTable dt = head.select(" where SequenceID=" + id.ToString());

            if (dt.Rows.Count < 1) throw new ArgumentException("Sequence ID does not exist!");

            //BoxStatus State = GetStatefromString(dt.Rows[0].Field<string>("Status"));
            //if (State == BoxStatus.FINISHED) throw new ArgumentException("TestSequence already finished!");

            Box newBox = new Box(dt);
            return newBox;
        }

        public void StartSequence()
        {
            if ((State != BoxStatus.STOPPED) && (State != BoxStatus.PAUSED) && (State != BoxStatus.NONE))
                throw new ArgumentException("Testsquenz nicht gestoppt!");

            if (ProbandID < 0) throw new ArgumentException("Kein Proband!");
                     
            InsertSequence();
            StepID = 1;
            State = BoxStatus.STARTED;
            UpdateHeadState();
            EnableBoxButtons();
        }

        public void StopSequence()
        {
            if (State != BoxStatus.STARTED) throw new ArgumentException("Testsquenz nicht gestartet!");
            State = BoxStatus.STOPPED;
            UpdateHeadState();
            EnableBoxButtons();
        }

        public void PauseSequence()
        {
            if (State != BoxStatus.STARTED) throw new ArgumentException("Testsquenz nicht gestartet!");
            State = BoxStatus.PAUSED;
            UpdateHeadState();
            EnableBoxButtons();
        }

        public void PrevStep()
        {
            if (StepID>0)
                StepID--;
            UpdateHeadState();
            EnableBoxButtons();
        }

        public void NextStep()
        {
            if (StepID < 4)
                StepID++;
            UpdateHeadState();
            EnableBoxButtons();

        }

        public void getSequenceOrder()
        {
            TestSequenceOder.Clear();

            Random rand = new Random();
            int order = rand.Next(5);
            switch (order)
            {
                case 0: TestSequenceOder.Add(TestSequence.BRIGHTNESS); TestSequenceOder.Add(TestSequence.CCT); TestSequenceOder.Add(TestSequence.JUDD); TestSequenceOder.Add(TestSequence.ALL); break;
                case 1: TestSequenceOder.Add(TestSequence.JUDD); TestSequenceOder.Add(TestSequence.BRIGHTNESS); TestSequenceOder.Add(TestSequence.CCT); TestSequenceOder.Add(TestSequence.ALL); break;
                case 2: TestSequenceOder.Add(TestSequence.CCT); TestSequenceOder.Add(TestSequence.JUDD); TestSequenceOder.Add(TestSequence.BRIGHTNESS); TestSequenceOder.Add(TestSequence.ALL); break;
                case 3: TestSequenceOder.Add(TestSequence.JUDD); TestSequenceOder.Add(TestSequence.CCT); TestSequenceOder.Add(TestSequence.BRIGHTNESS); TestSequenceOder.Add(TestSequence.ALL); break;
                case 4: TestSequenceOder.Add(TestSequence.BRIGHTNESS); TestSequenceOder.Add(TestSequence.JUDD); TestSequenceOder.Add(TestSequence.CCT); TestSequenceOder.Add(TestSequence.ALL); break;
                case 5: TestSequenceOder.Add(TestSequence.CCT); TestSequenceOder.Add(TestSequence.BRIGHTNESS); TestSequenceOder.Add(TestSequence.JUDD); TestSequenceOder.Add(TestSequence.ALL); break;
                
                default: TestSequenceOder.Add(TestSequence.BRIGHTNESS); TestSequenceOder.Add(TestSequence.CCT); TestSequenceOder.Add(TestSequence.JUDD); TestSequenceOder.Add(TestSequence.ALL); break;
                    
            }
        }

        private void InsertSequence()
        {
            try
            {
                SqlTransaction tran = LLSQL.sqlCon.BeginTransaction();
                LLSQL.cmd.Transaction = tran;

                SequenceID = getNextHeadID();

                head.Transaction = tran;
                head.insert(new string[] { SequenceID.ToString(), BoxNr.ToString(), ProbandID.ToString(), "0", String.Empty });

                getSequenceOrder();
                pos.Transaction = tran;
                for (int i = 0; i < TestSequenceOder.Count; i++)
                {
                    pos.insert(new string[] { SequenceID.ToString(), (i + 1).ToString(), TestSequenceOder[i].ToString(), "0", "0", "0.0", "0.0", String.Empty });
                }

                LLSQL.cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                LLSQL.cmd.Transaction.Rollback();
                throw ex;
            }

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
            string stmt = "update LLTestSequenceHead set remark='"+txt+"' where sequenceID="+SequenceID.ToString();
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();
        }

        private void UpdateHeadState()
        {
            string stmt = "update LLTestSequenceHead set Status='" + State.ToString() + "', ActualStep=" + StepID.ToString() +" where sequenceID=" + SequenceID.ToString();
            LLSQL.cmd.prep(stmt);
            LLSQL.cmd.Exec();
        }

        private static BoxStatus GetStatefromString(string s)
        {
            if (s.Equals("STARTED")) return BoxStatus.STARTED;
            if (s.Equals("STOPPED")) return BoxStatus.STOPPED;
            if (s.Equals("PAUSED")) return BoxStatus.PAUSED;
            if (s.Equals("FINISHED")) return BoxStatus.FINISHED;
            
            return BoxStatus.NONE;
        }

        private void GetTestSequenceOrder()
        {
            DataTable dt = pos.select(" where SequenceID=" + SequenceID.ToString() + " order by StepID");

            foreach( DataRow row in dt.Rows)
            {
                if (row.Field<string>("pimode").Equals("CCT")) TestSequenceOder.Add(TestSequence.CCT);
                if (row.Field<string>("pimode").Equals("BRIGHTNESS")) TestSequenceOder.Add(TestSequence.BRIGHTNESS);
                if (row.Field<string>("pimode").Equals("JUDD")) TestSequenceOder.Add(TestSequence.JUDD);
                if (row.Field<string>("pimode").Equals("ALL")) TestSequenceOder.Add(TestSequence.ALL);
            }
        }

        private void setPotisActive(TestSequence s)
        {
            int i = 0;
            PotisActive[(byte)BoxPotis.BRIGHTNESS] = 0; PotisActive[(byte)BoxPotis.CCT] = 0; PotisActive[(byte)BoxPotis.JUDD] = 0;
            for (i = 0; i < ButtonsActive.Length; i++) ButtonsActive[i] = 1;

                switch (s)
                {
                    case TestSequence.NONE:
                        //all inactive
                        for (i = 0; i < ButtonsActive.Length; i++) ButtonsActive[i] = 0;
                        break;
                    case TestSequence.BRIGHTNESS:
                        PotisActive[(byte)BoxPotis.BRIGHTNESS] = 1;
                        break;
                    case TestSequence.CCT:
                        PotisActive[(byte)BoxPotis.CCT] = 1;
                        break;
                    case TestSequence.JUDD:
                        PotisActive[(byte)BoxPotis.JUDD] = 1;
                        break;
                    case TestSequence.ALL:
                        PotisActive[(byte)BoxPotis.BRIGHTNESS] = 1; PotisActive[(byte)BoxPotis.CCT] = 1; PotisActive[(byte)BoxPotis.JUDD] = 1;
                        break;
                    default:
                        break;

                }
        }

        private void EnableBoxButtons()
        {
            if (State != BoxStatus.STARTED)  setPotisActive(TestSequence.NONE);                
            else setPotisActive(TestSequenceOder[StepID]);

            string Params = ";potis=" + PotisActive[(byte)BoxPotis.BRIGHTNESS] + PotisActive[(byte)BoxPotis.CCT] + PotisActive[(byte)BoxPotis.JUDD] +
                            ";buttons=" + ButtonsActive[0] + ButtonsActive[1];
           rCmd.EnableButtons(Params);           
        }

        private void ReceiveUDP(IDictionary<string, string> d)
        {

        }
    }
}

