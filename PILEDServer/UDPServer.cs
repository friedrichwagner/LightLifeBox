using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Lumitech.Helpers;

namespace PILEDServer
{
    //Singleton: There can only be one
    //http://www.codeproject.com/Articles/42354/The-Art-of-Logging
    //http://www.dofactory.com/Patterns/PatternSingleton.aspx#csharp

    
    public enum LLMode
    {
	    LL_SET_BRIGHTNESS = 1,
        LL_SET_CCT = 2,
	    LL_SET_XY = 3,	    
	    LL_SET_RGB = 4,
        LL_SET_LOCKED = 99,
    };

    public class PILEDData
    {
        public LLMode mode;
        public int groupid;
        public int cct;
        public int brightness;
        public double[] xy= new double[2];
        public int[] rgb= new int[3];

        public PILEDData() {}

        /*public PILEDData(string json)
        {
            PILEDData data = json.FromJson<PILEDData>();
            groupid = data.groupid;
            cct = data.cct;
            brigthness = data.brigthness;
            mode = data.mode;
            xy = data.xy;
            rgb = data.rgb;
        }*/
    }

    internal class Unsubscriber<PILEDData> : IDisposable
    {
        private List<IObserver<PILEDData>> _observers;
        private IObserver<PILEDData> _observer;

        internal Unsubscriber(List<IObserver<PILEDData>> observers, IObserver<PILEDData> observer)
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


    public sealed class UDPServer : IObservable<PILEDData>, IDisposable
    {
        private bool done;
        private int listenPort;
        private Thread ServerThread;
        private Logger log;
        private Settings ini;
        private  UdpClient listener;
        private bool bisStarted;
        public bool isStarted
        {
            get { return bisStarted; }
        }

        private List<IObserver<PILEDData>> observers;

        public UDPServer()
        {
            log = Logger.GetInstance();
            ini = Settings.GetInstance();

            done = false;
            listenPort = ini.Read<int>("UDPServer", "ListenPort",12345);
            observers = new List<IObserver<PILEDData>>();

            AddObservers();

            ServerThread = new Thread(new ThreadStart(StartListener));

            //PILEDData p = new PILEDData();
            //string s=p.ToJson();
        }

        public void Dispose()
        {
            Stop();
        }

        public IDisposable Subscribe(IObserver<PILEDData> observer)
        {
            // Check whether observer is already registered. If not, add it 
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber<PILEDData>(observers, observer);
        }


        public void Start()
        {
            if (!ServerThread.IsAlive)
            {
                ServerThread = new Thread(new ThreadStart(StartListener));
                ServerThread.Start();
            }

            bisStarted = ServerThread.IsAlive;
        }

        public void Stop()
        {
            foreach (var observer in observers)
                observer.OnCompleted();
            observers.Clear();

            if (ServerThread.IsAlive)
            {
                done = true;
                listener.Close();
                ServerThread.Join();
            }

            bisStarted = ServerThread.IsAlive;
        }


        private void StartListener()
        {
             
             IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
             listener = new UdpClient(listenPort);

             try
             {
                 log.Info("UDPServer started");

                 while (!done)
                 {
                     byte[] bytes = listener.Receive(ref groupEP);

                     if (bytes.Length > 0)
                     {
                         try
                         {
                             string s = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                             log.Info(s);
                             PILEDData info = s.FromJson<PILEDData>();                             
                             foreach (var observer in observers)
                                 observer.OnNext(info);
                         }
                         catch (Exception ex)
                         {
                             log.Error(ex.Message);
                         }

                     }
                 }

             }
             catch (Exception e)
             {
                 log.Error(e.Message);
             }
             finally
             {
                 log.Info("UDPServer stopped");
                 listener.Close();
             }
        }


        private void AddObservers()
        {
            string[] strInterfaces = ini.Read<string>("Definitions", "Interfaces", "").Split(',');

            foreach(string s in strInterfaces)
            {
                bool bEnable = ini.Read<bool>(s, "Enable", false);

                if (bEnable)
                {
                    IObserver<PILEDData> obs = null;

                    if (s == "LTDMX")
                    {
                        obs = new LTDMXBase(3);
                    }
                    else if (s=="LightLifeLogger")
                    {
                        obs = new LightLifeLogger();
                    }

                    if (obs != null)
                        Subscribe(obs);
                }

            }
        }
    }
}