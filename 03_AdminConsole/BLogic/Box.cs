using LightLife;
using LightLifeAdminConsole.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightLifeAdminConsole
{

    public enum BoxStatus { NONE, STARTED, STOPPED, PAUSED};
    public enum TestSequence { NONE, BRIGHTNESS, CCT, JUDD, ALL };

    class Box
    {
        public int BoxNr { get; private set; }
        public int ProbandID { get; set; }
        public BoxStatus State { get; set; }

        public int SequenceID { get; private set; }
        public string Remark { get; set; }
        
        public IList<TestSequence> TestSequenceOder { get; private set; }
        public int StepID { get; private set; }

        private AdminBase head;
        private AdminBase pos;

        public Box(int boxnr)
        {
            BoxNr = boxnr;
            ProbandID = -1;

            TestSequenceOder = new List<TestSequence>(4);
            State = BoxStatus.NONE;
            SequenceID = -1;

            head = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequenceHead"]);
            pos = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLTestSequencePos"]);
        }

        public void StartSequence()
        {
            if ((State != BoxStatus.STOPPED) && (State != BoxStatus.PAUSED) && (State != BoxStatus.NONE)) 
                    throw new ArgumentOutOfRangeException("Testsquenz nicht gestoppt!");

            if (ProbandID <0 ) throw new ArgumentOutOfRangeException("Kein Proband!");
                     
            InsertSequence();
            StepID = 0;
            State = BoxStatus.STARTED;
        }

        public void StopSequence()
        {
            if (State != BoxStatus.STARTED) throw new ArgumentOutOfRangeException("Testsquenz nicht gestartet!");
            State = BoxStatus.STOPPED;
        }

        public void PauseSequence()
        {
            if (State != BoxStatus.STARTED) throw new ArgumentOutOfRangeException("Testsquenz nicht gestartet!");
            State = BoxStatus.PAUSED;
        }

        public void PrevStep()
        {
            if (StepID>0)
                StepID--;
        }

        public void NextStep()
        {
            if (StepID < 4)
                StepID++;

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
    }
}
