using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Lumitech.Helpers;
using Lumitech.Interfaces;

namespace PILEDServer
{
    public sealed class UDPServer : IObservable<PILEDData>, IObservable<LightLifeData>, IDisposable
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

        private List<IObserver<PILEDData>> observersPILED;
        private List<IObserver<LightLifeData>> observersLightLife;

        public frmGroups fmGroups { get; set; }

        public UDPServer()
        {
            log = Logger.GetInstance();
            ini = Settings.GetInstance();

            done = false;
            listenPort = ini.Read<int>("UDPServer", "ListenPort",12345);
            observersPILED = new List<IObserver<PILEDData>>();
            observersLightLife = new List<IObserver<LightLifeData>>();

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
            foreach (var observer in observersPILED)
                observer.OnCompleted();
            observersPILED.Clear();

            foreach (var observer in observersLightLife)
                observer.OnCompleted();
            observersLightLife.Clear();

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
                 log.Info("UDPServer started Port="+ listenPort.ToString());

                 while (!done)
                 {
                     byte[] bytes = listener.Receive(ref groupEP);

                     if (bytes.Length > 0)
                     {
                         try
                         {
                             string s = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                             log.Info(groupEP.Address +":"+ s);

                             LightLifeData info = new LightLifeData();

                             if (s.Contains("roomid"))
                             {
                                 //Receive Messages from Admin Console
                                 info = s.FromJson<LightLifeData>();
                             }
                             else
                             {
                                 //Receive Messages from ControlBox
                                 info.piled = s.FromJson<PILEDData>();

                                 if (fmGroups!= null)
                                 {
                                     fmGroups.UpdateDataTable(info.piled);
                                 }
                             }

                             info.ip = groupEP.Address.ToString(); //always save address as well

                             //First send data to devices
                             foreach (var observer in observersPILED)
                             {
                                 observer.OnNext(info.piled);

                             }

                             //sencond, log it to database
                             foreach (var observer in observersLightLife)
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
    }
}
