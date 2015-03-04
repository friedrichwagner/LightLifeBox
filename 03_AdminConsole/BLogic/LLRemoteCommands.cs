using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LightLifeAdminConsole
{
    public enum enumRemoteCommand { DISCOVER = 1, ENABLE_BUTTONS = 2, SET_PILED = 3, GET_PILED = 4 };
    public delegate void ReceiveDataDelegate(IDictionary<string, string> d);

    class RemoteCommandBase
    {
        private const int TIMEOUT_INTERVAL = 5000;
        protected IPAddress _ip;
        private const int _port = 9998;
        protected UdpClient client;
        private IPEndPoint receivedfromEP = new IPEndPoint(IPAddress.Any, 0);
        public SocketException sockEx;
        public ReceiveDataDelegate ReceiveData;
        public bool bAsync;

        public RemoteCommandBase(IPAddress ip)
        {
            _ip = ip;
            client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false; // only if you want to send/receive on same machine
            client.Client.ReceiveTimeout = TIMEOUT_INTERVAL;
            client.EnableBroadcast = true;
            sockEx = null;
            bAsync = false;
        }

        protected int send(enumRemoteCommand cmd, string data)
        {
            try
            {
                sockEx = null;
                var remoteEP = new IPEndPoint(_ip, _port);
                string s = ((Int32)cmd).ToString() + data;

                Byte[] sendBytes = Encoding.ASCII.GetBytes(s);

                //Shoot and forget
                return client.Send(sendBytes, sendBytes.Length, remoteEP);
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
                var remoteEP = new IPEndPoint(_ip, _port);
                string s = ((Int32)cmd).ToString() + data;

                Byte[] sendBytes = Encoding.ASCII.GetBytes(s);

                client.Send(sendBytes, sendBytes.Length, remoteEP);

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = client.Receive(ref remoteEP);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                return str2Dict(returnData);
            }
            catch (SocketException se)
            {
                sockEx = se;
            }
            catch
            {
                throw;
            }
            return str2Dict("");
        }


        //Asynchronous Receive
        protected void sendAndReceiveAsync(enumRemoteCommand cmd, string data)
        {
            try
            {
                sockEx = null;
                var remoteEP = new IPEndPoint(_ip, _port);
                string s = ((Int32)cmd).ToString() + data;

                Byte[] sendBytes = Encoding.ASCII.GetBytes(s);

                client.Send(sendBytes, sendBytes.Length, remoteEP);

                IAsyncResult iar = client.BeginReceive(recvAsync, client);
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
            IPEndPoint receivedfromEP = new IPEndPoint(IPAddress.Any, 0);

            UdpClient client = (UdpClient)result.AsyncState;
            byte[] receiveBytes = client.EndReceive(result, ref receivedfromEP);
            string returnData = Encoding.ASCII.GetString(receiveBytes);

            if (ReceiveData!= null)
            {
                ReceiveData(str2Dict(returnData));
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
        public LLRemoteCommand(IPAddress ip)
            : base(ip)
        {
            //ReceiveData += ReceiveUDP;
        }

        public bool Ping()
        {
            return SendAndReceiveBool(enumRemoteCommand.DISCOVER, "");
        }

        public bool EnableButtons(string Params)
        {
            return SendAndReceiveBool(enumRemoteCommand.ENABLE_BUTTONS, Params);
        }

        public bool SetPILED(bool async, string Params)
        {
            return SendAndReceiveBool(enumRemoteCommand.SET_PILED, Params);
        }

        public IDictionary<string, string> GetPILED(string Params)
        {
            return SendAndReceiveDict(enumRemoteCommand.GET_PILED, Params);
        }
    }
}
