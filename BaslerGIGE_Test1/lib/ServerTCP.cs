using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AsyncTCP;
using System.Net;
using System.Windows.Forms;

namespace BaslerGIGE_Test1.lib
{
    public class ServerTCP
    {
        public AsyncTcpServer ServerTcp = null;
        
        public bool TCPConnected = false;
        private SocketClientManager ClientTcp = null;


        public string msReceiveData { get; set; }
        /// <summary>
        /// 服务器断开连接时向客户端发送的消息数据
        /// </summary>
        private string ServerClose { get; set; }
        /// <summary>
        /// 客户端心跳包消息文本
        /// </summary>
        private string ClientHeartbeatPpacketMsg { get; set; }
        /// <summary>
        /// 客户端用于心跳包消息的验证文本
        /// </summary>
        private string ClientHeartbeatPpacketVerificationMsg { get; set; }



        public ServerTCP()
        { 
        
        }
        /// <summary>
        /// 打开TCP
        /// </summary>
        /// <param name="port"></param>
        public void OpenServer(int port)
        {
           // if (!ComputerInfo.GetNetworkIsAvailable())
           // {
                string strIP = GetAddressIP();
                this.ServerClose = "$#@The server is closed. Please reconnect the client@#$";
                this.ClientHeartbeatPpacketMsg = "$#@The client tries to send the heartbeat packet to the server to verify that the connection is really lost@#$";
                this.ClientHeartbeatPpacketVerificationMsg = "$#@The client tries to send the heartbeat packet to the server, and the service verification is successful without losing the connection@#$";
                //IPAddress _ip = IPAddress.Parse("127.0.0.1");
                //ServerTcp = new AsyncTcpServer(IPAddress.Parse(strIP),port);
                ServerTcp = new AsyncTcpServer(port);
                ServerTcp.Encoding = Encoding.UTF8;

                ServerTcp.ClientConnected += new EventHandler<TcpClientConnectedEventArgs>(Tcp_CilentConnect);
                ServerTcp.ClientDisconnected += new EventHandler<TcpClientDisconnectedEventArgs>(Tcp_CilentDisconnect);
                ServerTcp.DatagramReceived += new EventHandler<TcpDatagramReceivedEventArgs<byte[]>>(Tcp_PlainByteReceived);
                ServerTcp.Start(1000);

          //  }
          //  else
          //  {
           //     MessageBox.Show("未连接网络!");
           // }

        }

        public string GetAddressIP()
        {
            string AddressIP = string.Empty;
            foreach (IPAddress IP in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (IP.AddressFamily.ToString() == "InterNetwork")
                {
                    if (IP.ToString() != "" && IP.ToString() != "0" && IP.ToString() != "127.0.0.1")
                    {
                        AddressIP = IP.ToString();
                    }

                }
            }

            return AddressIP;
        }

        private void Tcp_CilentConnect(object sender,TcpClientConnectedEventArgs e)
        {
            TCPConnected = true;
            ServerTcp.Send(e.TcpClient, "Connection Success");
        }
        private void Tcp_CilentDisconnect(object sender, TcpClientDisconnectedEventArgs e)
        {
            TCPConnected = false;
        }
        /// <summary>
        /// 自动接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tcp_PlainByteReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        { 
            object locked= new object();
            lock (locked)
            {
                string Msg = Encoding.UTF8.GetString(e.Datagram);
                if (Msg == this.ClientHeartbeatPpacketMsg)
                {
                    ServerTcp.Send(e.TcpClient, this.ClientHeartbeatPpacketVerificationMsg);
                }
                else
                {
                    msReceiveData = Msg;
                }
            }
        }
        /// <summary>
        /// 结束TCP
        /// </summary>
        public void CloseServer()
        {
            if (ServerTcp != null)
            {
                if (msReceiveData != null)
                {
                    ServerTcp.SendAll(this.ServerClose);
                    Thread.Sleep(500);
                    if (ServerTcp.IsRunning)
                    {
                        ServerTcp.Stop();
                    }
                }
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="context"></param>
        public void SendMsg(string context)
        {
            ServerTcp.SendAll(context);
        }

    }
}
