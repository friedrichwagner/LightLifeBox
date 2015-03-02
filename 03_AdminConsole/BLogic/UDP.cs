using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LightLifeAdminConsole
{
    class LLUDPClient
    {
        private IPAddress _ip;
        private const int _port = 9998;
        private UdpClient client;

        public LLUDPClient(IPAddress ip) 
        {
            _ip = ip;
            client = new UdpClient();
        }

        public void send(enumRemoteCommand cmd, string data)
        {
            var remoteEP = new IPEndPoint(_ip, _port);
            string s = ((Int32)cmd).ToString() + data;

            Byte[] sendBytes = Encoding.ASCII.GetBytes(s);

            //Shoot and forget
            client.SendAsync(sendBytes, sendBytes.Length, remoteEP);
        }

        public IDictionary<string, string> sendAndReceive(enumRemoteCommand cmd, string data)
        {
            var remoteEP = new IPEndPoint(_ip, _port);
            string s = ((Int32)cmd).ToString() + data;

            Byte[] sendBytes = Encoding.ASCII.GetBytes(s);

            client.Send(sendBytes, sendBytes.Length, remoteEP);

            // Blocks until a message returns on this socket from a remote host.
            Byte[] receiveBytes = client.Receive(ref remoteEP);
            string returnData = Encoding.ASCII.GetString(receiveBytes);

            return str2Dict(returnData);
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
    }
}
