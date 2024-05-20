using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Person2
{
    public partial class Form1 : Form
    {

        Socket clientSocket;
        Socket serverSocket;
        Socket receiveSocket;
        public const string postIp = "127.0.0.1";
        public const int postPort = 8080;
        public const string reciveIp = "127.0.0.1";
        public const int recivePort = 8081;
        public Form1()
        {
            InitializeComponent();
            InitNet();
        }
        private void InitNet()
        {
            // 初始化服务器端Socket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 绑定IP地址和端口
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(reciveIp), recivePort));

            // 监听队列
            serverSocket.Listen(10);

            // 开始接受客户端连接
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // 完成接受连接
                Socket receiveSocket = serverSocket.EndAccept(ar);

                // 开始接收数据
                ThreadPool.QueueUserWorkItem(ReceiveData, receiveSocket);

                // 继续接受新的连接
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting client connection: {ex.Message}");
            }
        }

        private void ReceiveData(object clientSocketObj)
        {            
            receiveSocket = (Socket)clientSocketObj;
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int bytesRead = receiveSocket.Receive(buffer);
                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        UpdateTextBox($"Data received from baby: " + receivedData+"\n");                        
                    }
                    else
                    {                                                
                        break;
                    }
                }
            }
            catch (Exception ex)
            {                
                receiveSocket.Close();
            }
        }
        private void UpdateTextBox(string message)
        {
            if (MessageShowBox.InvokeRequired)
            {
                MessageShowBox.
                    Invoke(new Action(() =>
                    MessageShowBox.AppendText(message + Environment.NewLine)));
            }
            else
            {
                MessageShowBox.AppendText(message + Environment.NewLine);
            }
        }

        private void btnConnect_click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(postIp), postPort));
            UpdateTextBox("connect success:" +postIp+":"+postPort);
            //ThreadPool.QueueUserWorkItem(ReceiveDataAsync);
        }

        private void ReceiveDataAsync(Object state)
        {
            while (clientSocket.Connected)
            {
                try
                {
                    byte[] byteToReceive = new byte[1024];
                    int bytesRead = clientSocket.Receive(byteToReceive);
                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(byteToReceive, 0, bytesRead);
                        MessageShowBox.Text = "Data received: " + receivedData;
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    MessageShowBox.Text += "Error recive data: " + e.Message;
                }
            }
        }

        private void btnPost_click(object sender, EventArgs e)
        {
            try
            {
                byte[] byteData = Encoding.UTF8.GetBytes(MessagePostBox.Text);
                clientSocket.Send(byteData);
                MessagePostBox.Text = "";
            }
            catch (Exception ex)
            {
                MessageShowBox.Text += "Error sending data: " + ex.Message;
            }
        }
        private void btnClose_click(object sender, EventArgs e)
        {
            clientSocket.Close();
            serverSocket.Close();
            receiveSocket.Close();
        }
    }
}
