using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumitech.Helpers;
using PILEDServer;
using Lumitech.Interfaces;
using LightLife;
using MvvmFoundation.Wpf;
using System.Diagnostics;
using System.Threading;


namespace LightLifeAdminConsole.MVVM
{
    class PiledVM : ObservableObject, IObservable<PILEDData>, IObservable<LightLifeData>, IDisposable
    {
        private static PiledVM _instance;
     
        private Settings ini;
        private List<IObserver<PILEDData>> observersPILED;
        private List<IObserver<LightLifeData>> observersLightLife;

        public IDictionary<int, string> rooms { get { return LLSQL.llrooms; } }
        private int _selectedRoom;
        public int SelectedRoom
        {
            get { return _selectedRoom; }
            set
            {
                _selectedRoom = value;                
                if (_selectedRoom  > -1)                
                    SelectedGroup = -1;
                RaisePropertyChanged("SelectedRoom");
            }
        } 

        public IDictionary<int, string> groups { get { return LLSQL.llgroups; } }
        private int _selectedGroup;
        public int SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                _selectedGroup = value;
                if (_selectedGroup > -1)
                    SelectedRoom = -1;
                RaisePropertyChanged("SelectedGroup");
            }
        }

        private string _ErrorText;
        public string ErrorText { get { return _ErrorText; } private set { _ErrorText = value; RaisePropertyChanged("ErrorText"); } }

        //public PILEDData piled;
        public LightLifeData lldata;

        public byte Brightness {
            get { return (byte)lldata.piled.brightness; }
            set
            {
                if (lldata.piled.brightness != value)
                {
                    lldata.piled.mode = PILEDMode.SET_BRIGHTNESS;
                    lldata.piled.brightness = value;
                    Notify();
                    RaisePropertyChanged("Brightness");
                }
            }
        }

        public int CCT
        {
            get { return lldata.piled.cct; }
            set
            {
                if (lldata.piled.cct != value)
                {
                    lldata.piled.mode = PILEDMode.SET_CCT;
                    lldata.piled.cct = value;
                    Notify();
                    RaisePropertyChanged("CCT");
                    RaisePropertyChanged("X");
                    RaisePropertyChanged("Y");
                }
            }
        }

        public byte R
        {
            get { return lldata.piled.r; }
            set
            {
                if (lldata.piled.r != value)
                {
                    lldata.piled.mode = PILEDMode.SET_RGB;
                    lldata.piled.r = value;
                    Notify();
                    RaisePropertyChanged("R");
                }
            }
        }

        public byte G
        {
            get { return lldata.piled.g; }
            set
            {
                if (lldata.piled.g != value)
                {
                    lldata.piled.mode = PILEDMode.SET_RGB;
                    lldata.piled.g = value;
                    Notify();
                    RaisePropertyChanged("G");
                }
            }
        }

        public byte B
        {
            get { return lldata.piled.b; }
            set
            {
                if (lldata.piled.b != value)
                {
                    lldata.piled.mode = PILEDMode.SET_RGB;
                    lldata.piled.b = value;
                    Notify();
                    RaisePropertyChanged("B");
                }
            }
        }

        public float X
        {
            get { return lldata.piled.x; }
            set
            {
                if (lldata.piled.x != value)
                {
                    lldata.piled.mode = PILEDMode.SET_XY;
                    lldata.piled.x = value;
                    //Senden nur bei Änderung von Y
                    //Notify();
                    RaisePropertyChanged("X");
                }
            }
        }

        public float Y
        {
            get { return lldata.piled.y; }
            set
            {
                if (lldata.piled.y != value)
                {
                    lldata.piled.mode = PILEDMode.SET_XY;
                    lldata.piled.y = value;
                    Notify();
                    RaisePropertyChanged("Y");
                }
            }
        }

        public static PiledVM GetInstance()
        {
            if (_instance == null)
                _instance = new PiledVM();

            return _instance;
        }

        private PiledVM()
        {
            ini = Settings.GetInstance();
            
            observersPILED = new List<IObserver<PILEDData>>();
            observersLightLife = new List<IObserver<LightLifeData>>();

            InitData();

            AddObservers();
        }

        private void InitData()
        {
            lldata = new LightLifeData();
            lldata.piled.sender = "AdminConsole";
            lldata.piled.receiver = "Lights";

            MainVM m = MainVM.GetInstance();
            lldata.userid = m.login.UserId;
            lldata.vlid = m.login.UserId;

            _selectedGroup = 0; //Broadcast
            _selectedRoom = -1;
        }

        public void Notify()
        {
            DateTime n = DateTime.Now;
            Debug.Print(n.ToString() + ":" + (n.ToString()));

            //First send data to devices
            foreach (var observer in observersPILED)
                observer.OnNext(lldata.piled);           

            //sencond, log it to database
            foreach (var observer in observersLightLife)
                observer.OnNext(lldata);

            //UI Thread verzögert 200ms
            //Thread.Sleep(200);
        }

        private void AddObservers()
        {
            string[] strInterfaces = ini.Read<string>("Pages", "Interfaces", "").Split(',');

            foreach (string s in strInterfaces)
            {
                bool bEnable = ini.Read<bool>(s, "Enable", false);

                if (bEnable)
                {
                    if (s.ToLower() == "neolink")
                    {
                        NeoLink nl = new NeoLink();
                        nl.Subscribe(this);
                    }


                    if (s.ToLower() == "lightlifelogger")
                    {
                        LightLifeLogger ll = new LightLifeLogger();
                        ll.Subscribe(this);
                    }
                }

            }
        }

        public void Done()
        {
            foreach (var observer in observersPILED)
                observer.OnCompleted();
            observersPILED.Clear();

            foreach (var observer in observersLightLife)
                observer.OnCompleted();
            observersLightLife.Clear();
        }

        public void Dispose()
        {
            Done();
        }

        public IDisposable Subscribe(IObserver<PILEDData> observer)
        {
            // Check whether observer is already registered. If not, add it 
            if (!observersPILED.Contains(observer))
            {
                observersPILED.Add(observer);
            }
            return new UnsubscriberPILEDData<PILEDData>(observersPILED, observer);
        }

        public IDisposable Subscribe(IObserver<LightLifeData> observer)
        {
            // Check whether observer is already registered. If not, add it 
            if (!observersLightLife.Contains(observer))
            {
                observersLightLife.Add(observer);
            }
            return new UnsubscriberLightLifeData<LightLifeData>(observersLightLife, observer);
        }

    }
}
