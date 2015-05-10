using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightLife.Data;
using LightLifeAdminConsole.Data;
using Lumitech.Helpers;

namespace LightLifeAdminConsole
{
    enum TestMode : int { DTEST_NONE = 0, DTEST_BRIGHTNESS_UP = 1, DTEST_BRIGHTNESS_DOWN = 2, DTEST_CCT_UP = 3, DTEST_CCT_DOWN = 4, DTEST_JUDD_UP = 5, DTEST_JUDD_DOWN = 6 };

    class DeltaTest
    {
        private LLRemoteCommand _rCmd;
        private Box2 _box;

        public int Proband { get; set; }
        public int Brightness { get; set; }
        public int CCT { get; set; }
        public TestMode TestMode   { get; set; }
 
        public DeltaTest(Box2 b)
        {
            Proband = 0;
            Brightness = LightLifeData.DEFAULT_BRIGHTNESS;
            CCT = LightLifeData.DEFAULT_CCT;
            TestMode = TestMode.DTEST_NONE;
            _box = b;
        }

        public bool CanStart()
        {
            if ((Proband > 0) && (Brightness > 0) && (Brightness <= 255) && (CCT >= LightLifeData.MIN_CCT) && (CCT <= LightLifeData.MAX_CCT) && (TestMode > TestMode.DTEST_NONE)) return true;
            return false;
        }

        public void Start()
        {
            string Params= ";brightness="+ Brightness + ";cct=" + CCT + ";user="+Proband + ";mode="+ (int)TestMode;
            _box.rCmd.StartDeltaTest(Params);
        }

        public void Save(string Params)
        {
            string[] sarr = Params.Split(';');
            Dictionary<string, string> dtmp = sarr.Select(item => item.Split('=')).ToDictionary(s => s[0], s => s[1]);
            MyDictionary d = new MyDictionary(dtmp);
            AdminBase deltatest = new AdminBase(LLSQL.sqlCon, LLSQL.tables["LLDeltaTest"]);
            deltatest.insert(d);


        }

    }
}
