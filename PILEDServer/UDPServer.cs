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
	    LL_SET_XY = 2,
	    LL_SET_CCT = 3,
	    LL_SET_RGB = 4,
    };

    public class PILEDData
    {
        public LLMode mode;
        public int groupid;
        public int cct;
        public int brigthness;
        public double[] xy= new double[2];
        public int[] rgb= new int[3];

        public PILEDData(string json)
        {
            PILEDData data = json.FromJson<PILEDData>();
            groupid = data.groupid;
            cct = data.cct;
            brigthness = data.brigthness;
            mode = data.mode;
            xy = data.xy;
            rgb = data.rgb;
        }
    }

    public sealed class UDPServer
    {
        private bool done;
        private int listenPort;
        private Thread ServerThread;
        private Logger log;
        private  UdpClient listener;

        public UDPServer()
        {
            log = Logger.GetInstance();
            Settings ini = Settings.GetInstance();
            done = false;
            listenPort = ini.Read<int>("UDPServer", "ListenPort",12345);

            ServerThread = new Thread(new ThreadStart(StartListener));            
        }

        ~UDPServer()
        {
            Stop();
        }

        public void Start()
        {
            if (!ServerThread.IsAlive) ServerThread.Start();
        }

        public void Stop()
        {
            if (ServerThread.IsAlive)
            {
                done = true;
                listener.Close();
                ServerThread.Join();
            }
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
                         string s = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                         PILEDData p = s.FromJson<PILEDData>();
                         log.Info(s);
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

    }
}