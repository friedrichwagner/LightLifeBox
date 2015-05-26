using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightLife.Data;
using LightLifeAdminConsole.Data;
using Lumitech.Helpers;
using System.Threading;

namespace LightLifeAdminConsole
{
    public enum TestMode : int { DTEST_NONE = 0, DTEST_BRIGHTNESS_UP = 1, DTEST_BRIGHTNESS_DOWN = 2, DTEST_CCT_UP = 3, DTEST_CCT_DOWN = 4, DTEST_JUDD_UP = 5, DTEST_JUDD_DOWN = 6 };

    public enum DeltaTestCommand { START, STOP, SAVE};
    public delegate void DeltaTestEventDelegate(DeltaTestCommand cmd, TestMode mode);

    class DeltaTest
    {
        //private LLRemoteCommand _rCmd;
        private Box2 _box;

        public int Proband { get; set; }
        public int Brightness { get; set; }
        public int CCT { get; set; }
        public TestMode TestMode   { get; set; }
        public DeltaTestEventDelegate DeltaTestEvent;

        private bool _isRunning;
        public bool isRunning { get { return _isRunning; } }
        public string Result { get; private set; }
 
        public DeltaTest(Box2 b)
        {
            Proband = 0;
            Brightness = LightLifeData.DEFAULT_BRIGHTNESS;
            CCT = LightLifeData.DEFAULT_CCT;
            TestMode = TestMode.DTEST_NONE;
            _box = b;
            Result = String.Empty;
        }

        public bool CanStart()
        {
            if ((Proband > 0) && (Brightness > 0) && (Brightness <= 255) && (CCT >= LightLifeData.MIN_CCT) && (CCT <= LightLifeData.MAX_CCT) && (TestMode > TestMode.DTEST_NONE)) return true;
            return false;
        }

        public void Start()
        {
            string Params= ";brightness="+ Brightness + ";cct=" + CCT + ";userid="+Proband + ";mode="+ (int)TestMode;
            _box.rCmd.StartDeltaTest(Params);
            _isRunning = true;
            Result = String.Empty;

            if (DeltaTestEvent != null)
                DeltaTestEvent(DeltaTestCommand.START, TestMode);
        }

        public void Stop()
        {
            _isRunning = false;
            string Params = "";
            _box.rCmd.StopDeltaTest(Params);
            Thread.Sleep(500);
            EnableBoxButtons("11111", "00000");
           
            if (DeltaTestEvent != null)
                DeltaTestEvent(DeltaTestCommand.STOP, TestMode);
        }

        private void EnableBoxButtons(string enabledButtons, string blinkleds)
        {
            string Params = ";buttons=" + enabledButtons + ";blinkleds=" + blinkleds;

            _box.rCmd.EnableButtons(Params);
        }

        public void Save(string Params)
        {
            _isRunning = false;
            /*string[] sarr = Params.Split(';');
            Dictionary<string, string> dtmp = sarr.Select(item => item.Split('=')).ToDictionary(s => s[0], s => s[1]);*/

            IDictionary<string, string> dtmp = RemoteCommandBase.str2Dict(Params);
            MyDictionary d = new MyDictionary((Dictionary<string, string>)dtmp);
            AdminBase deltatest = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLDeltaTest"]);
            
            deltatest.insert(d);

            getResult(d);

            if (DeltaTestEvent != null)
                DeltaTestEvent(DeltaTestCommand.SAVE, TestMode);
        }

        private string getResult(MyDictionary d)
        {
            switch(TestMode)
            {
                case TestMode.DTEST_BRIGHTNESS_DOWN:
                case TestMode.DTEST_BRIGHTNESS_UP:
                    Result = d["brightness"].ToString();
                    break;

                case TestMode.DTEST_CCT_DOWN:
                case TestMode.DTEST_CCT_UP:
                    Result = d["cct"].ToString();
                    break;

                case TestMode.DTEST_JUDD_DOWN:
                case TestMode.DTEST_JUDD_UP:
                    Result = d["actduv"].ToString();
                    break;
            }

            return Result;
        }

    }
}
