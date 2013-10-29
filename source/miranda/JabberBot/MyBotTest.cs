using System;
using System.Net;
using System.Net.Sockets;

namespace miranda.JabberBot
{
    public class MyBotTest
    {
        private Socket _socket;

        public void ConnectTest(string addr, int port)
        {
            /*
            this.m_Compressed = false;
            this.m_ReadBuffer = null;
            this.m_ReadBuffer = new byte[0x400];
            //*/
            try
            {
                IPAddress address = Dns.GetHostEntry(addr).AddressList[1];
                var remoteEP = new IPEndPoint(address, port);
                //this.m_ConnectTimedOut = false;
                //var callback = new TimerCallback(this.connectTimeoutTimerDelegate);
                //this.connectTimeoutTimer = new Timer(callback, null, this.ConnectTimeout, this.ConnectTimeout);
                if (Socket.OSSupportsIPv6 && (remoteEP.AddressFamily == AddressFamily.InterNetworkV6))
                {
                    this._socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                }
                else
                {
                    this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                _socket.Connect(remoteEP);
                //this._socket.BeginConnect(remoteEP, new AsyncCallback(this.EndConnect), null);
            }
            catch (Exception exception)
            {
               
            }
        }

        private void EndConnect(IAsyncResult ar)
        {
            return;
        }
    }
}