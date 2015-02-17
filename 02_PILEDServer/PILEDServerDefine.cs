using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumitech.Helpers;
using System.Threading;
using System.Collections.Concurrent;

namespace PILEDServer
{
    public enum PILEDMode
    {
        PILED_SET_NONE = 0,
        PILED_SET_BRIGHTNESS = 1,
        PILED_SET_CCT = 2,
        PILED_SET_XY = 3,
        PILED_SET_RGB = 4,
        PILED_SET_LOCKED = 99,
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
        public PILEDMode mode;
        public int groupid;
        public int cct;
        public int brightness;
        public double[] xy = new double[2];
        public int[] rgb = new int[3];
        public string sender;
        public string receiver;
        public LLMsgType msgtype;

        public PILEDData() 
        {
            Reset();
        }

        public void Reset()
        {
            mode = PILEDMode.PILED_SET_CCT;
            groupid = 0; //Broadcast
            cct = 3000;
            brightness = 255;
            xy[0] = 0.0; xy[1] = 0.0;
            rgb[0] = 0; rgb[1] = 0; rgb[2] = 0;
            sender = String.Empty;
            receiver = String.Empty;
            msgtype = LLMsgType.LL_SET_LIGHTS;
        }
        
        public override string ToString()
        {
            return String.Format("g:{0} --> m:{1} b:{2} cct:{3} xy:{4:0.000}/{5:0.000} rgb:{6} / {7} / {8} s->r:{9}->{10} mt:{11}", groupid, mode, brightness, cct, xy[0], xy[1], rgb[0], rgb[1], rgb[2], sender, receiver, msgtype);
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

        public LightLifeData()  { }
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
