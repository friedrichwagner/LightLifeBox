using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightLife
{

    public enum BoxStatus { NONE, STARTED, STOPPED, PAUSED};
    public enum TestSequence { NONE, BRIGHTNESS, CCT, JUDD, ALL };

    class Box
    {
        public int BoxNr { get; private set; }
        public int ProbandID { get; set; }
        public BoxStatus State { get; set; }

        public int SequenceID { get; private set; }
        
        public IList<TestSequence> TestSequenceOder { get; private set; }
        public int StepID { get; private set; }

        public Box(int boxnr)
        {
            BoxNr = boxnr;
            ProbandID = -1;

            TestSequenceOder = new List<TestSequence>(4);
            State = BoxStatus.NONE;
            SequenceID = -1;
        }

        public void StartSequence()
        {
            if ((State != BoxStatus.STOPPED) && (State != BoxStatus.PAUSED) && (State != BoxStatus.NONE)) 
                    throw new ArgumentOutOfRangeException("Testsquenz nicht gestoppt!");

            if (ProbandID <0 ) throw new ArgumentOutOfRangeException("Kein Proband!");

            getSequenceOrder();
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
    }
}
