using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumitech.Helpers;
using MvvmFoundation.Wpf;
using LightLife;

namespace LightLifeAdminConsole.MVVM
{
    class BoxVM: ObservableObject
    {
        private static BoxVM _instance;
        private Settings ini;
        public IDictionary<int, Box> boxes;

        public int cntBoxes { get { return boxes.Count; } }
        public int SelectedBox { get; set; }

        public IDictionary<int, string> probanden { get { return LLSQL.probanden; } }

        private int _selectedProband;
        public int SelectedProband
        {
            get {
                if (_selectedProband > -1)
                    return boxes[SelectedBox].ProbandID;
                else
                    return _selectedProband;
            }
            set
            {
                if (value > -1)
                {
                    boxes[SelectedBox].ProbandID = value;                    
                }

                _selectedProband = value;
                RaisePropertyChanged("SelectedProband");
            }
        }
           
        public static BoxVM GetInstance()
        {
            if (_instance == null)
                _instance = new BoxVM();

            return _instance;
        }

        private BoxVM()
        {
            ini = Settings.GetInstance();

            boxes = new Dictionary<int, Box>();
            getBoxes(ref boxes);
            _selectedProband = -1;
        }

        private void getBoxes(ref IDictionary<int, Box> b)
        {
            int n=ini.Read<int>("Pages", "NrOfBoxes", 3);
            for (int i=0; i < n; i++)
            {
                b.Add(i, new Box(i));
            }
        }
    }
}
