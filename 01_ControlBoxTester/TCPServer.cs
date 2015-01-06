using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ControlBoxTester
{
    public delegate void NewClientConnectedEventHandler(string msg, int cnt, bool added);
    public delegate void NewMessageEventHandler(string msg);

    class SignalButton
    {
        public string Name;
        public int ID;
        public int Value;
        public int Value2;
        public bool done = false;
        public EventWaitHandle _waitHandle = new AutoResetEvent(false);

        public SignalButton(int pID, string pName)
        {
            ID = pID;
            Name = pName;
        }
    }

    class TCPServer
    {
        static readonly object _locker = new object();

        public event NewClientConnectedEventHandler NewClientConnected;
        public event NewMessageEventHandler NewMessage;

        private TcpListener tcpListener;
        private Thread listenThread;
        private int iConnectedClients;
        public int connectedClients 
        {
            get { return iConnectedClients;}
        }

        private int PortNr;

        public List<SignalButton> Buttons;

        public TCPServer( int pPortNr)
        {
            int iConnectedClients = 0;
            PortNr = pPortNr;

            //this.tcpListener = new TcpListener(IPAddress.Loopback, PortNr); // Change to IPAddress.Any for internet wide Communication
            this.tcpListener = new TcpListener(IPAddress.Any, PortNr); 
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

            Buttons = new List<SignalButton>();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            try
            {
                while (true) // Never ends until the Server is closed.
                {
                    //blocks until a client has connected to the server
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    //create a thread to handle communication 
                    //with connected client
                    iConnectedClients++; // Increment the number of clients that have communicated with us.

                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);
                }
            }
            catch (SocketException e)
            {
                //Console.WriteLine("SocketException: {0}", e);
                //NewMessage(e.Message);
            }
            finally
            {
                // Stop listening for new clients.
                tcpListener.Stop();
            }

        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            //byte[] buffer = new byte[3];
            int bytesRead=0;
            int cnt = 0;
            

            //1. Read the "name" which his send from the button after connecting
            //message has successfully been received
            bytesRead = clientStream.Read(message, 0, 4096);

            ASCIIEncoding encoder = new ASCIIEncoding();

            // Convert the Bytes received to a string and display it on the Server Screen
            SignalButton b;
            string btnName = encoder.GetString(message, 0, bytesRead);
            lock (_locker)
            {
                b = new SignalButton(Buttons.Count(), btnName);

                Buttons.Add(b);
            }

            NewClientConnected(btnName, iConnectedClients, true);

            while (!b.done)
            {
                try
                {
                    //blocks until a client sends a message

                    b._waitHandle.WaitOne();
                    if (b.done)
                    {
                        throw new ArgumentException("Done");
                    }

                    //byte[] buffer = encoder.GetBytes(b.Value.ToString()+";"+b.Value2.ToString());
                    byte[] buffer = encoder.GetBytes(b.Value.ToString());
                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush();

                    cnt++;

                    //Thread.Sleep(1000);
                }
                catch
                {
                    //a socket error has occured
                    iConnectedClients--;
                    NewClientConnected(btnName, iConnectedClients, false);
                    lock (_locker)
                    {
                        Buttons.Remove(b);
                    }
                    break;
                }
            }

            tcpClient.Close();
        }

        private void WriteMessage(string msg)
        {
            /*if (this.rtbServer.InvokeRequired)
            {
                WriteMessageDelegate d = new WriteMessageDelegate(WriteMessage);
                this.rtbServer.Invoke(d, new object[] { msg });
            }
            else
            {
                this.rtbServer.AppendText(msg + Environment.NewLine);
            }*/
        }

        private void Echo(string msg, ASCIIEncoding encoder, NetworkStream clientStream)
        {
            // Now Echo the message back
            byte[] buffer = encoder.GetBytes(msg);

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }

        public void Close()
        {
            foreach (SignalButton b in Buttons)
            {
                b.done = true;
                b._waitHandle.Set();
            }
            tcpListener.Stop();
        }

        public void SetButton(string name, int val)
        {
            foreach (SignalButton b in Buttons)
            {
                if (b.Name == name)
                {
                    b.Value = val;
                    b._waitHandle.Set();
                }
            }
        }

        public void SetPoti(string name, int val)
        {
            foreach (SignalButton b in Buttons)
            {
                if (b.Name == name)
                {
                    b.Value = val;
                    b._waitHandle.Set();
                }
            }
        }
    }
}
