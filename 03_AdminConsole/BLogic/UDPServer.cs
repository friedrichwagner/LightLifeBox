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
using System.Collections;

namespace LightLifeAdminConsole
{

    public struct RemoteCommandData
    {
        public int cmdID;
        public int BoxNr;
        public IDictionary<string, string> param;

        public RemoteCommandData(string s, string ip)
        {
            param = RemoteCommandBase.str2Dict(s);
            cmdID = Int32.Parse(param["CmdId"]);
            BoxNr = Int32.Parse(param["BoxNr"]);            
        }
    }

    public sealed class UDPServer : IObservable<RemoteCommandData>, IDisposable
    {
        private bool done;
        private int listenPort;
        private Logger log;
        private Settings ini;
        private  UdpClient listener;
        private bool bisStarted;
        public bool isStarted
        {
            get { return bisStarted; }
        }

        private List<IObserver<RemoteCommandData>> observers;

        public UDPServer()
        {
            log = Logger.GetInstance();
            ini = Settings.GetInstance();

            done = false;
            listenPort = ini.Read<int>("UDPServer", "ListenPort", (int)LightLifePorts.ADMIN_CONSOLE_LISTEN_PORT);
            observers = new List<IObserver<RemoteCommandData>>();
        }


        public void Dispose()
        {
            Stop();
        }

        public IDisposable Subscribe(IObserver<RemoteCommandData> observer)
        {
            // Check whether observer is already registered. If not, add it 
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber<RemoteCommandData>(observers, observer);
        }


        public void Start()
        {
            if (!bisStarted)
                 StartListener();
        }

        public void Stop()
        {
            foreach (var observer in observers)
                observer.OnCompleted();
            observers.Clear();

            done = true;
            listener.Close();   
        }

        private void StartListener()
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
                                    log.Info(receivedResults.RemoteEndPoint.Address + ":" + s);

                                    string ip = receivedResults.RemoteEndPoint.Address.ToString(); //always save address as well

                                    RemoteCommandData info = new RemoteCommandData(s, ip);

                                    foreach (var observer in observers)
                                    {
                                        observer.OnNext(info);
                                    }
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
    }

    internal class Unsubscriber<RemoteCommandData> : IDisposable
    {
        private List<IObserver<RemoteCommandData>> _observers;
        private IObserver<RemoteCommandData> _observer;

        internal Unsubscriber(List<IObserver<RemoteCommandData>> observers, IObserver<RemoteCommandData> observer)
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
