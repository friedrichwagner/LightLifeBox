using System;
using System.Windows;
using System.Collections.Generic;
using Lumitech.Helpers;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LightLife.Data
{
    public enum PILEDMode
    {
        SET_NONE = 0,
        SET_BRIGHTNESS = 1,
        SET_CCT = 2,
        SET_XY = 3,
        SET_RGB = 4, 
        SET_SEQUENCE = 5,
        SET_SCENE = 6,

        IDENTIFY=50,

        SET_LOCKED = 99,
    };

    public enum LLMsgType
    {
        LL_SET_LIGHTS = 10,
        LL_CALL_SCENE = 20,
        LL_START_TESTSEQUENCE = 30,
        LL_STOP_TESTSEQUENCE = 31,
        LL_PAUSE_TESTSEQUENCE = 32,
        LL_NEXT_TESTSEQUENCE_STEP = 33,
        LL_PREV_TESTSEQUENCE_STEP = 34,
        LL_SET_BUTTONS = 35
    };

    public class PILEDData
    {
        //CIE194
        public static Point wl460nm = new Point(0.1389222, 0.0589201);
        public static Point wl615nm = new Point(0.6730550, 0.3269450);
        public static Point wl560nm = new Point(0.3905251, 0.5623339);

        public const int MIN_CCT = 2500;
        public const int MAX_CCT = 7000;
        public const Observer obs = Observer.Observer10deg;

        public PILEDMode mode;
        
        private byte _groupid;
        public byte groupid 
        {
            get { return _groupid; }
            set { _groupid = value; }
        }

        private int _cct;
        public int cct
        {
            get { return _cct; }
            set
                {
                    if (value >= MIN_CCT && value <= MAX_CCT)
                    {
                        _cct = value;
                        CIECoords cie = (Photometric.CCT2xy(_cct, obs));
                        _xy[0] = (float)cie.x; _xy[1] = (float)cie.y;
                    }
                    else
                        throw new ArgumentOutOfRangeException("CCT only between 2500! and 7000K!");
                }
        }

        private byte _brightness;    
        public byte brightness
        {
            get { return _brightness; }
            set { _brightness=value; }
        }


        private Single[] _xy = new Single[2];
        public Single[] xy
        {
            get { return _xy; }
            set {
                if (_xy[0] >= 0 && _xy[0] <= 1 && _xy[1] >= 0 && _xy[1] <= 1)
                    _xy = value;
                else
                    throw new ArgumentOutOfRangeException("xy only between 0.0 and 1.0!");
            }
        }
        public Single x
        {
            get { return _xy[0]; }
            set { checkXY(ref _xy[0], value);  }
        }

        public Single y
        {
            get { return _xy[1]; }
            set { checkXY(ref _xy[1], value); }
        }

        private byte[] _rgb = new byte[3];
        public byte[] rgb
        {
            get { return _rgb; }
            set { _rgb = value; }
        }
        public byte r
        {
            get { return _rgb[0]; }
            set { checkRGB(ref _rgb[0], value); }
        }
        public byte g
        {
            get { return _rgb[1]; }
            set { checkRGB(ref _rgb[1], value); }
        }
        public byte b
        {
            get { return _rgb[2]; }
            set { checkRGB(ref _rgb[2], value); }
        }

        private byte _sceneid;
        public byte sceneid
        {
            get { return _sceneid; }
            set { _sceneid = value; }
        }

        private byte _sequenceid;
        public byte sequenceid
        {
            get { return _sequenceid; }
            set { _sequenceid = value; }
        }

        public string sender;
        public string receiver;
        public LLMsgType msgtype;

        public PILEDData()
        {
            Reset();           
        }

        public PILEDData(PILEDMode m, byte br, int cct, float x, float y, byte r, byte g, byte b, string psender, string preceiver)
        {
            mode = m;
            _groupid = 0; //Broadcast
            _cct = cct;
            _brightness = br;
            _xy[0] = x; _xy[1] = y;
            _rgb[0] = r; _rgb[1] = g; _rgb[2] = b;

            sender = psender;
            receiver = preceiver;
            msgtype = LLMsgType.LL_SET_LIGHTS;

            _sequenceid = 0;
            _sceneid = 0;
        }

        public void Reset()
        {
            mode = PILEDMode.SET_BRIGHTNESS;
            _groupid = 0; //Broadcast
            _cct = 3000;
            _brightness = 255;
            _xy[0] = 0.0f; _xy[1] = 0.0f;
            _rgb[0] = 0; _rgb[1] = 0; _rgb[2] = 0;
            sender = String.Empty;
            receiver = String.Empty;
            msgtype = LLMsgType.LL_SET_LIGHTS;

            _sequenceid = 0;
            _sceneid = 0;
        }

        public override string ToString()
        {
            return String.Format("g:{0} --> m:{1} b:{2} cct:{3} xy:{4:0.000}/{5:0.000} rgb:{6} / {7} / {8} s->r:{9}->{10} mt:{11}", groupid, mode, _brightness, _cct, _xy[0], _xy[1], _rgb[0], _rgb[1], _rgb[2], sender, receiver, msgtype);
        }

        private void checkXY(ref Single s, float value)
        {
            if (s >= 0 && s <= 1)
                s = value;
            else
                throw new ArgumentOutOfRangeException("xy only between 0.0 and 1.0!");
        }

        private void checkRGB(ref byte b, byte value)
        {
            if (b >= 0 && b <= 255)
                b = value;
            else
                throw new ArgumentOutOfRangeException("rgb only between 0 and 255!");
        }
    }

    public class LightLifeData
    {
        public int roomid;
        public int userid;
        public int vlid;
        public int sceneid;
        public int sequenceid;
        public int stepid;
        public string remark;
        public string ip;
        public byte[] buttons = new byte[4];

        public PILEDData piled;

        public LightLifeData()  
        {
            Reset();
        }

        public void Reset()
        {
            piled = new PILEDData();

            piled.sender = "AdminConsole";
            piled.receiver = "Lights";
   
            roomid = 0;
            userid = 0;
            vlid = 0;
            sceneid = 0;
            sequenceid = 0;
            stepid = 0;
            remark = String.Empty;

            bool found = false;
            List<IPAddress> ipList = new List<IPAddress>();
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        //take first one
                        ip = ua.Address.ToString();
                        found = true;
                        break;
                    }

                if (found) break;
            }
        }
    }

    internal class UnsubscriberPILEDData<PILEDData> : IDisposable
    {
        private List<IObserver<PILEDData>> _observers;
        private IObserver<PILEDData> _observer;

        internal UnsubscriberPILEDData(List<IObserver<PILEDData>> observers, IObserver<PILEDData> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }

    internal class UnsubscriberLightLifeData<LightLifeData> : IDisposable
    {
        private List<IObserver<LightLifeData>> _observers;
        private IObserver<LightLifeData> _observer;

        internal UnsubscriberLightLifeData(List<IObserver<LightLifeData>> observers, IObserver<LightLifeData> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
