using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using LightLife.Data;

namespace LightLifeAdminConsole
{
    //public enum enumRemoteCommand { DISCOVER = 1, ENABLE_BUTTONS = 2, SET_PILED = 3, GET_PILED = 4, SET_SEQUENCE=5 };
    //public enum enumRemoteGetCommand : int { SET_LOCKED = 100, SET_DEFAULT = 101 } 

    public delegate void ReceiveDataDelegate(RemoteCommandData rmCmd);

    public struct RemoteCommandData
    {
        public int cmdId;
        public string cmdParams;
        public IDictionary<string, string> flds;

        public RemoteCommandData(string paramsString)
        {
            Debug.Print(paramsString);

            cmdParams = paramsString;
            flds = RemoteCommandBase.str2Dict(cmdParams);

            if (!flds.ContainsKey("CmdId"))
                cmdId = (int)LLMsgType.LL_NONE;
            else
                cmdId = Int32.Parse(flds["CmdId"]);

            Debug.Print("cmdId:" + ((LLMsgType)cmdId).ToString());

        }
    }

    class RemoteCommandBase
    {
        public const int RECVBUF_LEN = 256;
        private const int TIMEOUT_INTERVAL = 3000;

        protected IPAddress _ip;
        protected int _sendport;
        protected UdpClient sendClient;
        protected int _recvport;
        protected UdpClient recvClient;

        private IPEndPoint receivedfromEP = new IPEndPoint(IPAddress.Any, 0);
        private byte[] recvBuf = new byte[RECVBUF_LEN];

        public SocketException sockEx;
        public ReceiveDataDelegate ReceiveData;
        public bool IsAsync { get; private set; }
        private IAsyncResult iar;
        private bool isReceiving = false;

        public RemoteCommandBase(IPAddress ip, int sendport, int recvport, bool async)
        {
            _ip = ip;
            _sendport = sendport;
            _recvport = recvport; // kann auch das gleich wie _sendport sein
            sockEx = null;
            IsAsync = async;

            //Send and Receive on same Port: Remote Commands to ControlBox --> Answers back
            var recvEP = new IPEndPoint(IPAddress.Any, _recvport);
            recvClient = new UdpClient();
            recvClient.ExclusiveAddressUse = false;
            recvClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            recvClient.Client.ReceiveTimeout = TIMEOUT_INTERVAL;
            recvClient.Client.Bind(recvEP);
            //byte[] buffer = receiveClient.Receive(ref ep1);

            sendClient = new UdpClient();
            sendClient.ExclusiveAddressUse = false;
            sendClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sendClient.Client.ReceiveTimeout = TIMEOUT_INTERVAL;
            var remoteEP = new IPEndPoint(_ip, _sendport);
            sendClient.Connect(remoteEP);

            //TEST TEST
            //StartReceiveAsync();
        }

        public void Close()
        {
            recvClient.Close();
            sendClient.Close();            
        }

        protected int send(LLMsgType cmd, string data)
        {
            try
            {
                sockEx = null;
                var remoteEP = new IPEndPoint(_ip, _sendport);

                Byte[] sendBytes = Encoding.ASCII.GetBytes(data);

                //Shoot and forget
                return sendClient.Send(sendBytes, sendBytes.Length);
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
        protected IDictionary<string, string> sendAndReceive(LLMsgType cmd, string data)
        {
            try
            {
                sockEx = null;

                string s = "CmdId=" + ((Int32)cmd).ToString() + data;

                Byte[] sendBytes = Encoding.ASCII.GetBytes(s);
                sendClient.Send(sendBytes, sendBytes.Length);
                recvBuf = recvClient.Receive(ref receivedfromEP);

                string returnData = Encoding.ASCII.GetString(recvBuf);
                Debug.Print(returnData); //Debug.Print("\r\n");

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
        protected void sendAndReceiveAsync(LLMsgType cmd, string data, bool wait)
        {
            try
            {
                sockEx = null;
                string s = "CmdId=" + ((Int32)cmd).ToString() + data;

                if (!isReceiving)
                    StartReceiveAsync();

                Byte[] sendBytes = Encoding.ASCII.GetBytes(s);
                sendClient.Send(sendBytes, sendBytes.Length);

                if (wait)
                {                    
                    iar.AsyncWaitHandle.WaitOne(TIMEOUT_INTERVAL, true);

                    if (!iar.IsCompleted)
                    {
                        isReceiving = false;
                        StartReceiveAsync();
                    }

                }                                   
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
                if (isReceiving) return;

                for (int i = 0; i < RECVBUF_LEN; i++) recvBuf[i] = 0x00;
                sockEx = null;

                EndPoint ep = receivedfromEP;
                iar = recvClient.Client.BeginReceiveFrom(recvBuf, 0, RECVBUF_LEN, SocketFlags.None, ref ep, new AsyncCallback(recvAsync), recvClient.Client);

                isReceiving = true;
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

                if (recvClient.Client != null)
                {
                    int msgLen = recvSock.EndReceiveFrom(result, ref clientEP);

                    string returnData = Encoding.ASCII.GetString(recvBuf);

                    if (ReceiveData != null)
                    {
                        RemoteCommandData rmCmd = new RemoteCommandData(returnData);
                        ReceiveData(rmCmd);
                    }
                }
            }
            catch (SocketException se)
            {
                sockEx = se;
            }
            catch (ObjectDisposedException)
            {
                //Silently ignore!!
            }
            finally
            {                      
                //Restart Receiving
                isReceiving = false;
                if (recvClient.Client != null)
                    StartReceiveAsync();
            }
        }

        public static IDictionary<string, string> str2Dict(string s)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string[] returnData = s.Split(';');
            foreach (var s1 in returnData)
            {
                string[] s2 = s1.Split('=');
                if (s2.Length == 2)
                {
                    dict[s2[0]] = s2[1];
                }
            }

            return dict;
        }

        protected bool SendAndReceiveBool(LLMsgType cmd, string data, bool wait)
        {
            if (!IsAsync)
            //if (true)
            {
                IDictionary<string, string> d = sendAndReceive(cmd, data);
                if (d.Count > 0) return true;
            }
            else
            {
                sendAndReceiveAsync(cmd, data, wait);

                if (iar.IsCompleted)               
                    return true;               
            }

            return false;
        }

        protected IDictionary<string, string> SendAndReceiveDict(LLMsgType cmd, string data, bool wait)
        {
            if (!IsAsync)
            {
                IDictionary<string, string> d = sendAndReceive(cmd, data);
                return d;
            }
            else
            {
                sendAndReceiveAsync(cmd, data, wait);
            }

            return null;
        }


    }

    class LLRemoteCommand : RemoteCommandBase
    {
        const string PILED_SEND_TEMPLATE = ";mode={0};brightness={1};cct={2};r={3};g={4};b={5};x={6};y={7};fadetime={8};duv={9}";

        public LLRemoteCommand(IPAddress ip, int sendp, int recvp, bool async)
            : base(ip, sendp, recvp, async)
        {
            //ReceiveData += ReceiveUDP;
        }

        public bool Ping(int groupid, bool isPracticeBox)
        {
            string Params = ";groupid=" + groupid.ToString() + ";ispracticebox=" + ((isPracticeBox) ? "1" : "0");
            return SendAndReceiveBool(LLMsgType.LL_DISCOVER, Params, true);
        }

        public bool EnableButtons(string Params)
        {
            return SendAndReceiveBool(LLMsgType.LL_ENABLE_BUTTONS, Params, false);
        }

        public bool SetPILED(PILEDMode mode, int brightness, int cct, int[] rgb, float[] xy, int fadetime, float duv)
        {
            string Params = String.Format(PILED_SEND_TEMPLATE, (int)mode, brightness, cct, rgb[0], rgb[1], rgb[2], xy[0], xy[1], fadetime, duv);
            return SendAndReceiveBool(LLMsgType.LL_SET_PILED, Params, false);
        }

        public IDictionary<string, string> GetPILED(string Params)
        {
            return SendAndReceiveDict(LLMsgType.LL_GET_PILED, Params, false);
        }

        public bool SetSequence(string Params)
        {
            return SendAndReceiveBool(LLMsgType.LL_SET_SEQUENCEDATA, Params, false);
        }

        public bool StartDeltaTest(string Params)
        {
            return SendAndReceiveBool(LLMsgType.LL_START_DELTATEST, Params, false);
        }
    }
}
