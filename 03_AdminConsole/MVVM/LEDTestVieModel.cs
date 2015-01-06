using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Data;
using System.IO;
using MvvmFoundation.Wpf;
using FirstFloor.ModernUI.Windows.Controls;
using System.Threading;
using System.ComponentModel;
using Lumitech;
using Lumitech.Helpers;

namespace PILEDTestSuite.MVVM
{
    public enum PILED_LEDNUM { RED = 0, PHOSPHOR = 1, BLUE = 2 };

    class LEDTestViewModel : ObservableObject
    {

        #region fields

        public static LEDTestViewModel _instance;
        private BackgroundWorker bw1 = new BackgroundWorker();

        private LTQSData ltqs;
        private MMSmall mm2;

        public byte BrightnessLevel;
        byte ModuleNr;
        Int16[] TemperatureRange;
        public bool isStarted = false;

        private int _iLEDOnTime;
        public int LEDOnTime { get { return _iLEDOnTime; } }

        private bool _bUsePowerSource;
        
        private PowerSource _PowerSource;
        private Int16[] CurrentRange;
        private string _ExportTemplate;

        #endregion

        #region properties

        public DataItemQSHead head { get; set; }
        public DataItemQSPos pos { get; set; }

        public string TestBoxComport { get { return mm2.Comport; } set { mm2.Comport = value; } }

        public DataTable Articles { get { return ltqs.getArticleList(); } }
        public DataTable Locations { get { return ltqs.getLocationList(); } }

        public bool isConnected { get { return mm2.isConnected; } }

        public bool OnlineData { get { return ltqs.OnlineData; } }
        public bool UsePowerSource { get { return _bUsePowerSource; } }

        private byte[] _brightness;
        public byte[] Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness[0] = value[0]; _brightness[1] = value[1]; _brightness[2] = value[2];

                if (_brightness[0] > 0) _bRedIsOn = true; else _bRedIsOn = false;
                if (_brightness[1] > 0) _bPhosphorIsOn = true; else _bPhosphorIsOn = false;
                if (_brightness[2] > 0) _bBlueIsOn = true; else _bBlueIsOn = false;

                SetBrightness();
            }
        }

        private bool _bPhosphorIsOn;
        public bool PhosphorIsOn
        {
            get { return _bPhosphorIsOn; }
            set
            {
                _brightness[1] = (value ? BrightnessLevel : (byte)0);
                SetBrightness();
                _bPhosphorIsOn = value;
            }
        }

        private bool _bBlueIsOn;
        public bool BlueIsOn
        {
            get { return _bBlueIsOn; }
            set
            {
                _brightness[2] = (value ? BrightnessLevel : (byte)0);
                SetBrightness();
                _bBlueIsOn = value;
            }
        }

        private bool _bRedIsOn;
        public bool RedIsOn
        {
            get { return _bRedIsOn; }
            set
            {
                _brightness[0] = (value ? BrightnessLevel : (byte)0);
                SetBrightness();
                _bRedIsOn = value;
            }
        }

        public bool LEDIsOn(PILED_LEDNUM lednr)
        {
            bool ret = false;
            switch (lednr)
            {
                case PILED_LEDNUM.RED:
                    ret = _bRedIsOn; break;
                case PILED_LEDNUM.PHOSPHOR:
                    ret = _bPhosphorIsOn; break;
                case PILED_LEDNUM.BLUE:
                    ret = _bBlueIsOn; break;
            }

            return ret;
        }

        public bool SetLED(PILED_LEDNUM lednr, bool value)
        {
            bool ret = false;

            switch (lednr)
            {
                case PILED_LEDNUM.RED:
                    ret = RedIsOn = value; break;
                case PILED_LEDNUM.PHOSPHOR:
                    ret = PhosphorIsOn = value; break;
                case PILED_LEDNUM.BLUE:
                    ret = BlueIsOn = value; break;
            }

            return ret;
        }

        public bool ToggleLED(PILED_LEDNUM lednr)
        {
            bool ret = false;

            switch (lednr)
            {
                case PILED_LEDNUM.RED:
                    ret = RedIsOn = !_bRedIsOn; break;
                case PILED_LEDNUM.PHOSPHOR:
                    ret = PhosphorIsOn = !_bPhosphorIsOn; break;
                case PILED_LEDNUM.BLUE:
                    ret = BlueIsOn = !_bBlueIsOn; break;
            }

            return ret;
        }


        //private bool _bTSensorIsOK;
        public bool TSensorIsOK { 
            get { return (bool)pos.TSensorOK; } 
        }

        public bool checkTemperature()
        {
            pos.TSensorOK = false;

            if (mm2 != null)
            {
                pos.Temperature = (int)mm2.getTemperature(ModuleNr);
                if (pos.Temperature >= TemperatureRange[0] && pos.Temperature <= TemperatureRange[1])
                    pos.TSensorOK = true;
                else
                    pos.TSensorOK = false;
            }

            return (bool)pos.TSensorOK;
        }

        //private bool _bEeepromIsOK;
        public bool EepromIsOK { get { return (bool)pos.EepromOK; } }

        #endregion

        #region ICommands

        private ICommand _ToggleLEDCommand;
        public ICommand ToggleLEDCommand
        {
            get
            {
                if (_ToggleLEDCommand == null)
                {
                    //_ToggleLEDCommand = new RelayCommand<PILED_LEDNUM>(param => this.ToggleLED((PILED_LEDNUM)param), param => mm2.isConnected);
                    _ToggleLEDCommand = new RelayCommand<PILED_LEDNUM>(param => this.ToggleLED((PILED_LEDNUM)param), param => mm2.isConnected);
                }
                return _ToggleLEDCommand;
            }
        }

        private ICommand _CheckEepromAndTSensorCommand;
        public ICommand CheckEepromAndTSensorCommand
        {
            get
            {
                if (_CheckEepromAndTSensorCommand == null)
                {
                    _CheckEepromAndTSensorCommand = new RelayCommand(() => this.CheckEepromAndTSensor(), () => mm2.isConnected);
                }
                return _CheckEepromAndTSensorCommand;
            }
        }

        private ICommand _ReadTemperatureCommand;
        public ICommand ReadTemperatureCommand
        {
            get
            {
                if (_ReadTemperatureCommand == null)
                {
                    _ReadTemperatureCommand = new RelayCommand(() => this.checkTemperature(), () => mm2.isConnected);
                }
                return _ReadTemperatureCommand;
            }
        }

        private ICommand _CheckEepromCommand;
        public ICommand CheckEepromCommand
        {
            get
            {
                if (_CheckEepromCommand == null)
                {
                    _CheckEepromCommand = new RelayCommand(() => this.checkEeprom(), () => mm2.isConnected);
                }
                return _CheckEepromCommand;
            }
        }

        private ICommand _StartCommand;
        public ICommand StartCommand
        {
            get
            {
                if (_StartCommand == null)
                {
                    _StartCommand = new RelayCommand(() => this.StartTest());
                }
                return _StartCommand;
            }
        }

        #endregion

        #region methods

        public static LEDTestViewModel GetInstance(bool withDBConnection, bool connectLTEFImmediately)
        {
            if (_instance == null)
                _instance = new LEDTestViewModel(withDBConnection, connectLTEFImmediately);

            return _instance;
        }

        private LEDTestViewModel(bool withDBConnection, bool connectLTEFImmediately)
        {

            ltqs = new LTQSData(withDBConnection);
            mm2 = new MMSmall(connectLTEFImmediately);
            head = new DataItemQSHead(); head.setEmpty();
            pos = new DataItemQSPos(); pos.Reset();

            head.LocationID = Int32.Parse(Locations.Rows[0]["LocationID"].ToString());
            head.ArticleNr = Articles.Rows[0]["Nr"].ToString();

            bw1.DoWork += (obj, li) => bw_DoWork(this, null);
            bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_TestCompleted);
            //bw1.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);

            bw1.WorkerReportsProgress = true;
            bw1.WorkerSupportsCancellation = false;

            InitLEDTest();
            InitPowerSource();
        }

        private void InitLEDTest()
        {
            Settings ini = Settings.GetInstance();
            BrightnessLevel = ini.Read<byte>("LEDTest", "BrightnessLevel", 255);
            ModuleNr = ini.Read<byte>("LEDTest", "ModuleNr", 0);
            _iLEDOnTime = ini.Read<int>("LEDTest", "LEDOnTimeMS", 3000);
            _ExportTemplate = ini.Read<string>("LEDTest", "ExportTemplate", "");
            string[] trange = ini.Read<string>("LEDTest", "TemperatureRange", "").Split(';');

            TemperatureRange = new Int16[2] { 15, 35 };
            if (trange.Length == 2)
            {
                TemperatureRange[0] = Int16.Parse(trange[0]);
                TemperatureRange[1] = Int16.Parse(trange[1]);
            }

            _brightness = new byte[3] { 0, 0, 0 };
            _bPhosphorIsOn = false;
            _bBlueIsOn = false;
            _bRedIsOn = false;
        }

        private void InitPowerSource()
        {
            Settings ini = Settings.GetInstance();
            _bUsePowerSource = ini.Read<string>("LEDTest", "OperatingMode", "").Equals("automatic");

            String pSource = ini.Read<string>("LEDTest", "PowerSource", "");
            if (pSource.Equals("Keithley"))
                _PowerSource = PowerSource.GetInstance(pSource);

            if (pSource.Equals("Toellner"))
                _PowerSource = Toellner.GetInstance(pSource);

            if (_PowerSource != null)
            {
                _PowerSource.SourceVoltage = ini.Read<float>(pSource, "Voltage", 48.0f);
                _PowerSource.SourceCurrent = ini.Read<float>(pSource, "Current", 1000f);
                _PowerSource.Mode = (OUTPUT_MODE)ini.Read<int>(pSource, "OutputMode", (int)OUTPUT_MODE.CONSTANT_VOLTAGE);
            }

            string[] crange = ini.ReadString("LEDTest", "CurrentRange", "").Split(';');

            CurrentRange = new Int16[2] { 20, 500 };
            if (crange.Length == 2)
            {
                CurrentRange[0] = Int16.Parse(crange[0]);
                CurrentRange[1] = Int16.Parse(crange[1]);
            }

            if (_bUsePowerSource && pSource.Length == 0)
            {
                _bUsePowerSource = false;
                if (_PowerSource == null) throw new ArgumentOutOfRangeException("No Powersource selected! Check settings file.");
            }
        }

        public bool PowerSourceIsOn { get { if (_PowerSource == null) return false; else  return _PowerSource.isOn; } }
        public void PowerSourceOn()
        {
            if (!_PowerSource.wasInitialized) _PowerSource.Init();
            _PowerSource.On();
        }

        public void PowerSourceOff(bool disconnect = false)
        {
            _PowerSource.Off();
            if (disconnect) _PowerSource.Done();
        }

        public bool CheckCurrentInRange(ref Single Current)
        {
            bool ret = false;

            Current = -1;

            if (!_bUsePowerSource) return true;

            if (_PowerSource.isOn)
            {
                _PowerSource.Measure();

                if (_PowerSource.Current >= CurrentRange[0] && _PowerSource.Current <= CurrentRange[1]) ret = true;
                Current = _PowerSource.Current;
            }

            return ret;
        }

        private void SetBrightness()
        {
            if (mm2 != null)
                mm2.setBrightness(_brightness);
        }

        private bool CheckHeaderData()
        {
            if (head.ArticleNr == null) throw new ArgumentException("Please select an article!");
            if (head.LocationID == 0) throw new ArgumentException("Please select a location!");
            if (head.FAUF.Length == 0) throw new ArgumentException("Please enter a Batchnr!");
            if (head.ProductionDate == null) throw new ArgumentException("Please enter a production date!");
            if (head.UserID.Length == 0) throw new ArgumentException("Please enter a Username!");

            return true;
        }

        public int SaveHead()
        {
            CheckHeaderData();
            return ltqs.SaveHead(head, pos);
        }

        private bool CheckPosData()
        {
            if (float.IsNaN(pos.mARed)) throw new ArgumentException("Please enter current consumption!");
            if (pos.RedOK == null) throw new ArgumentException("Please check OK/NOK!");

            if (float.IsNaN(pos.mAPhosphor)) throw new ArgumentException("Please enter current consumption!");
            if (pos.PhosphorOK == null) throw new ArgumentException("Please check OK/NOK!");


            if (float.IsNaN(pos.mABlue)) throw new ArgumentException("Please enter current consumption!");
            if (pos.BlueOK == null) throw new ArgumentException("Please check OK/NOK!");

            return true;
        }

        public bool StartTest()
        {
            try
            {
                if (head.FAUF.Length == 0) throw new ArgumentException("Fertigungsauftrag eingeben!");
                //if (ledtest.mm2 != null)
                {
                    //if (ledtest.mm2.isConnected) throw new ArgumentException("Not connected!");
                    if (UsePowerSource) PowerSourceOn();

                    if (!isConnected)  Connect();

                    if (UsePowerSource) StartBackgroundThread();
                    else
                    {
                        pos.Reset();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
               if (UsePowerSource) PowerSourceOff();
               ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
               return false;
            }
        }

        public bool Connect()
        {
            if (!mm2.isConnected)
            {
                mm2.Connect();
            }

            return mm2.isConnected;
        }

        public bool Disconnect()
        {

            mm2.Disconnect();
            return mm2.isConnected;
        }


        public void SavePos()
        {
            CheckPosData();
            ltqs.SavePos(head, pos);

            pos.Reset(false);

            if (UsePowerSource) PowerSourceOff();

            if (!UsePowerSource)
            {
                if (mm2.isConnected)
                {
                    //ledtest.mm2.setBrightness(new byte[] { 0, 0, 0 });
                    Brightness = new byte[] { 0, 0, 0 };
                    mm2.Disconnect();
                }
            }
        }

        public DataView getQCOrderPos()
        {
            return ltqs.ds.Tables["QCOrderPos"].DefaultView;
        }

        public void ExportToExcel()
        {
            Dictionary<string, string> headerData = new Dictionary<string, string>();

            headerData.Add("FAUF", head.FAUF);
            headerData.Add("ArticleNr", head.ArticleNr);
            headerData.Add("ProductionDate", head.ProductionDate.ToString());
            headerData.Add("QtyOK", head.cntOK.ToString());
            headerData.Add("QtyTotal", head.cntTotal.ToString());

            DataRow[] row = ltqs.ds.Tables["Location"].Select("LocationID='" + head.LocationID + "'");
            if (row.Length > 0)
                headerData.Add("Location", row[0][1].ToString());
            else
                headerData.Add("Location", "");

            row = ltqs.ds.Tables["Article"].Select("Nr='" + head.ArticleNr + "'");
            if (row.Length > 0)
                headerData.Add("ArticleDesc", row[0][1].ToString());
            else
                headerData.Add("ArticleDesc", "");

            //row = ltqs.ds.Tables["QCOrderPos"].Select("HeadID='" + head.HeadID + "'");
            DataTable ExportTable = ltqs.ds.Tables["QCOrderPos"].Select("HeadID='" + head.HeadID + "'").CopyToDataTable().DefaultView
                .ToTable(false, "PosID", "mARed", "RedOK", "mAPhosphor", "PhosphorOK", "mABlue", "BlueOK", "Temperature", "TSensorOK", "EepromOK", "AllOK", "TimeStamp", "Remark");

            //Nochmal neu holen
            Settings ini = Settings.GetInstance(true);
            _ExportTemplate = ini.Read<string>("LEDTest", "ExportTemplate", "");

            if (!Path.IsPathRooted(_ExportTemplate))
                _ExportTemplate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _ExportTemplate);

            ExportToExcel xl = new ExportToExcel();
            xl.GenerateReport<DataTable>(_ExportTemplate, headerData, ExportTable);
        }

        public bool checkEeprom()
        {
            pos.EepromOK = false;

            if (mm2 != null)
            {
                pos.EepromOK = mm2.checkEEPROM(ModuleNr);
            }

            return (bool)pos.EepromOK;
        }

        public void CheckEepromAndTSensor()
        {
            checkEeprom();
            checkTemperature();
        }


        private bool StartBackgroundThread()
        {
            if (bw1.IsBusy == true) return false;

            Mouse.SetCursor(Cursors.Wait);
            //ResetRBs();

            if (UsePowerSource)
            {
                PowerSourceOn();
                Disconnect();
                Thread.Sleep(1000);
                Connect();
            }

            //EnableButtons(false);

            if (bw1.IsBusy != true)
                bw1.RunWorkerAsync();

            return true;
        }

        #endregion

        #region AUTOMATIC_TESTING

        private void bw_DoWork(object sender, List<object> obj)
        {
            try
            {
                //Step0: All Off
                Brightness = new byte[] { 0, 0, 0 };

                for (int i = 0; i <= (int)PILED_LEDNUM.BLUE; i++)
                {
                    SetLED((PILED_LEDNUM)i, true);
                    Thread.Sleep(LEDOnTime);
                    //bw1.ReportProgress(i);
                    CheckPowerSource((PILED_LEDNUM)i);
                    Brightness = new byte[] { 0, 0, 0 };
                }

                bool b = checkTemperature();
                bw1.ReportProgress(5);

                b = checkEeprom();
                bw1.ReportProgress(10);
            }
            catch
            {
                throw;
            }
        }

        void bw_TestCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null) throw (e.Error);

                cntOK();
                SaveToFile();
            }
            catch (Exception ex)
            {
                Mouse.SetCursor(Cursors.Arrow);
                FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
            }
            finally
            {
                Disconnect();
                if (UsePowerSource)
                    PowerSourceOff();
            }
        }

        private int cntOK()
        {
            if (pos.PhosphorOK == true && pos.BlueOK == true && pos.RedOK == true && pos.TSensorOK == true && pos.EepromOK == true) head.cntOK++;
            head.cntTotal++;
                

            return 1;

        }

        private void CheckPowerSource(PILED_LEDNUM lednr)
        {
            bool ret = false;
            Single current = 0;
            if (UsePowerSource)
            {
                if (CheckCurrentInRange(ref current))
                    ret = true;

                switch (lednr)
                {
                    case PILED_LEDNUM.RED:
                        this.pos.RedOK= ret; break;
                    case PILED_LEDNUM.PHOSPHOR:
                        this.pos.PhosphorOK=ret; break;
                    case PILED_LEDNUM.BLUE:
                        this.pos.BlueOK=ret; break;
                }
            }
        }

        public void SaveToFile()
        {
            string fname = DateTime.Today.ToString().Replace(':', '_').Replace('.', '_') + ".txt";

            using (StreamWriter sw = new StreamWriter(fname, true))
            {
                string line = String.Format("{0:G}\tFAUF:{1}\tR:{2}\tG:{3}\tB:{4}\tT:{5}\tE:{6}\tTotal:{7}OK:{8}", DateTime.Now, head.FAUF, pos.RedOK, pos.PhosphorOK, pos.BlueOK, pos.TSensorOK, pos.EepromOK, head.cntTotal, head.cntOK);
                sw.WriteLine(line);
                sw.Close();
            }

            cntOK();
            
            //In Gui ausführen, damit man sieht, was passiert ist
            //pos.Reset();
        }

        #endregion
    }
}
