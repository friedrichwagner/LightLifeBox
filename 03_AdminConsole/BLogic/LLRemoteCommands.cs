using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace LightLifeAdminConsole
{
    public enum enumRemoteCommand { DISCOVER = 1, ENABLE_BUTTONS = 2, SET_PILED = 3, GET_PILED = 4, SET_SEQUENCE=5 };
    public enum enumRemoteGetCommand { SET_LOCKED , SET_DEFAULT };
    public delegate void ReceiveDataDelegate(IDictionary<string, string> d);

    class RemoteCommandBase
    {
        public const int RECVBUF_LEN = 256;
        private const int TIMEOUT_INTERVAL = 3000;
        protected IPAddress _ip;
        protected int _sendport;
        protected int _listenport;
        protected UdpClient sendClient;
        protected  Socket receiveSock;
        private IPEndPoint receivedfromEP = new IPEndPoint(IPAddress.Any, 0);
        private byte[] recvBuf = new byte[RECVBUF_LEN];

        public SocketException sockEx;
        public ReceiveDataDelegate ReceiveData;
        public bool bAsync;

        public RemoteCommandBase(IPAddress ip, int sendport, int listenport)
        {
            _ip = ip;
            _sendport = sendport;
            _listenport = listenport;
            sockEx = null;
            bAsync = false;

            sendClient = new UdpClient();
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sendClient.ExclusiveAddressUse = false; // only if you want to send/receive on same machine
            sendClient.Client.ReceiveTimeout = TIMEOUT_INTERVAL;

            var remoteEP = new IPEndPoint(_ip, _sendport);
            sendClient.Connect(remoteEP);


            var listenEP = new IPEndPoint(IPAddress.Any, _listenport);
            receiveSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receiveSock.ExclusiveAddressUse = false;
            receiveSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            receiveSock.ReceiveTimeout = TIMEOUT_INTERVAL;
            receiveSock.Bind(listenEP);

            //StartReceiveAsync();
        }

        public void Close()
        {
            //if (sendClient.Client.Connected)
                sendClient.Close();

            //if (sendClient.Client.Connected)
                receiveSock.Close();
        }

        protected int send(enumRemoteCommand cmd, string data)
        {
            try
            {
                sockEx = null;
                var remoteEP = new IPEndPoint(_ip, _sendport);

                Byte[] sendBytes = Encoding.ASCII.GetBytes(data);

                //Shoot and forget
                //return sendClient.Send(sendBytes, sendBytes.Length, remoteEP);
                return sendClient.Send(sendBytes, sendBytes.Length);

                /*client.Connect(remoteEP);
                client.BeginSend(sendBytes, sendBytes.Length, null, null);
                return sendBytes.Length;*/
            }
            catch (SocketException se)
            {
                sockEx = se;
            }
            catch
            {
                throw;
            }

            return -1;
        }

        //Synchronous
        protected IDictionary<string, string> sendAndReceive(enumRemoteCommand cmd, string data)
        {
            try
            {
                sockEx = null;
                string s = ((Int32)cmd).ToString() + data;

                Byte[] sendBytes = Encoding.ASCII.GetBytes(s);
                //sendClient.Send(sendBytes, sendBytes.Length, remoteEP);
                sendClient.Send(sendBytes, sendBytes.Length);

                // Blocks until a message returns on this socket from a remote host.
                //Byte[] receiveBytes = sendClient.Receive(ref remoteEP);
                int len = receiveSock.Receive(recvBuf, RECVBUF_LEN, SocketFlags.None);

                string returnData = Encoding.ASCII.GetString(recvBuf);
                Debug.Print(returnData);

                return str2Dict(returnData);
            }
            catch (SocketException se)
            {
                sockEx = se;
                Debug.Print(se.Message);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw;
            }
            return str2Dict("");
        }


        //Send and Asynchronous Receive
        protected void sendAndReceiveAsync(enumRemoteCommand cmd, string data)
        {
            try
            {
                sockEx = null;                
                string s = ((Int32)cmd).ToString() + data;

                EndPoint ep = receivedfromEP;
                IAsyncResult iar = receiveSock.BeginReceiveFrom(recvBuf, 0, RECVBUF_LEN, SocketFlags.None, ref ep, new AsyncCallback(recvAsync), receiveSock);

                Byte[] sendBytes = Encoding.ASCII.GetBytes(s);
                sendClient.Send(sendBytes, sendBytes.Length);
                //iar.AsyncWaitHandle.WaitOne();
                
            }
            catch (SocketException se)
            {
                sockEx = se;
            }
            catch
            {
                throw;
            }
        }

        //Asynchronous Receive --> e.g. Set_Locked from ControlBox
        protected void StartReceiveAsync()
        {
            try
            {
                sockEx = null;
                EndPoint ep = receivedfromEP;
                IAsyncResult iar = receiveSock.BeginReceiveFrom(recvBuf, 0, RECVBUF_LEN, SocketFlags.None, ref ep, new AsyncCallback(recvAsync), receiveSock);
            }
            catch (SocketException se)
            {
                sockEx = se;
            }
            catch
            {
                throw;
            }
        }

        protected void recvAsync(IAsyncResult result)
        {
            try
            {
                Socket recvSock = (Socket)result.AsyncState;
                EndPoint clientEP = receivedfromEP;
                int msgLen = recvSock.EndReceiveFrom(result, ref clientEP);

                string returnData = Encoding.ASCII.GetString(recvBuf);

                if (ReceiveData != null)
                {
                    ReceiveData(str2Dict(returnData));
                }
            }
            catch (SocketException se)
            {
                sockEx = se;
            }
            finally
            {
                //Restart Receiving
                StartReceiveAsync();
            }
        }

        public IDictionary<string, string> str2Dict(string s)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string[] returnData = s.Split(';');
            foreach (var s1 in returnData)
            {
                string[] s2 = s1.Split('=');
                if (s2.Length==2)
                    dict[s2[0]] = s2[1];
            }

            return dict;
        }

        protected bool SendAndReceiveBool(enumRemoteCommand cmd, string data)
        {
            if (!bAsync)
            {
                IDictionary<string, string> d = sendAndReceive(cmd, data);
                if (d.Count > 0) return true;
            }
            else
            {
                sendAndReceiveAsync(cmd, data);
            }

            return false;
        }

        protected IDictionary<string, string> SendAndReceiveDict(enumRemoteCommand cmd, string data)
        {
            if (!bAsync)
            {
                IDictionary<string, string> d = sendAndReceive(cmd, data);
                return d;
            }
            else
            {
                sendAndReceiveAsync(cmd, data);
            }

            return null;
        }
    }

    class LLRemoteCommand : RemoteCommandBase
    {
        const string PILED_TEMPLATE = ";mode={0};brightness={1};cct={2};r={3};g={4};b={5};x={6};y={7}";
        public LLRemoteCommand(IPAddress ip, int sendp, int recvp)
            : base(ip, sendp, recvp)
        {
            //ReceiveData += ReceiveUDP;
        }

        public bool Ping(int groupid)
        {
            return SendAndReceiveBool(enumRemoteCommand.DISCOVER, ";groupid=" + groupid.ToString());
        }

        public bool EnableButtons(string Params)
        {
            return SendAndReceiveBool(enumRemoteCommand.ENABLE_BUTTONS, Params);
        }

        public bool SetPILED(int mode, int brightness, int cct, int[] rgb, float[] xy)
        {
            string Params = String.Format(PILED_TEMPLATE, mode, brightness, cct, rgb[0], rgb[1], rgb[2], xy[0], xy[1]);
            return SendAndReceiveBool(enumRemoteCommand.SET_PILED, Params);
        }

        public IDictionary<string, string> GetPILED(string Params)
        {
            return SendAndReceiveDict(enumRemoteCommand.GET_PILED, Params);
        }

        public bool SetSequence(string Params)
        {
            return SendAndReceiveBool(enumRemoteCommand.SET_SEQUENCE, Params);
        }
    }
}
