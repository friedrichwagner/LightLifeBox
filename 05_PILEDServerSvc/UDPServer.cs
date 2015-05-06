using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Lumitech.Helpers;
using Lumitech.Interfaces;
using LightLife.Data;
using System.Threading.Tasks;
using LightLifeGlobalDefines;

namespace PILEDServer
{
    public sealed class UDPServer : IObservable<PILEDData>, IObservable<LightLifeData>, IDisposable
    {
        private bool done;
        private int listenPort;
        //private Thread ServerThread;
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

        public UDPServer()
        {
            ini = Settings.GetInstance();

            int loglevel = ini.Read<int>("Logging", "loglevel", 6);
            log = Logger.GetInstance(loglevel);            

            done = false;
            listenPort = ini.Read<int>("UDPServer", "ListenPort", (int)LightLifePorts.PILED_SERVER_LISTEN_UDP);
            observersPILED = new List<IObserver<PILEDData>>();
            observersLightLife = new List<IObserver<LightLifeData>>();

            AddObservers();
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
            //Alternativ
            log.Info("Starting Listener...");
            StartListener2();
        }

        public void Stop()
        {
            log.Info("Stopping Listener...");

            foreach (var observer in observersPILED)
                observer.OnCompleted();
            observersPILED.Clear();

            foreach (var observer in observersLightLife)
                observer.OnCompleted();
            observersLightLife.Clear();

            //Alternativ
            done = true;
            listener.Close();   
        }

        private  void StartListener2()
        {
            Task.Run(async () =>
            {
                using (listener = new UdpClient(listenPort))
                {
                    try
                    {
                        log.Info("UDPServer started Port=" + listenPort.ToString());
                        while (!done)
                        {
                            bisStarted = true;
                            var receivedResults = await listener.ReceiveAsync();
                            string s = Encoding.ASCII.GetString(receivedResults.Buffer);
                            

                            if (s.Length > 0)
                            {
                                try
                                {
                                    log.Cout(receivedResults.RemoteEndPoint.Address + ":" + s);

                                    LightLifeData info = new LightLifeData(s);

                                    info.ip = receivedResults.RemoteEndPoint.Address.ToString(); //always save address as well

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
                        done = true;
                        bisStarted = false;
                    }
                }
            });
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
                        log.Info("Added Observer 'NeoLink'");
                    }


                    if (s.ToLower() == "lightlifelogger")
                    {
                        LightLifeLogger ll = new LightLifeLogger();
                        ll.Subscribe(this);
                        log.Info("Added Observer 'LightLifeLogger'");
                    }
                }

            }
        }
    }
}
