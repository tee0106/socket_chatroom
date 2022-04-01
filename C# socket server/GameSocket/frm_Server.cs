using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace GameSocket
{

    public class SocketT2h
    {
        public Socket _Socket { get; set; }
        public string _Name { get; set; }
        public SocketT2h(Socket socket)
        {
            this._Socket = socket;
        }
    }
    public partial class frm_Server : Form
    {
        private  byte[] _buffer = new byte[1024];
        public List<SocketT2h> __ClientSockets { get; set; }
        List<string> _names = new List<string>();
        private  Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public frm_Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            __ClientSockets = new List<SocketT2h>();
        }

        private void frm_Server_Load(object sender, EventArgs e)
        {
            SetupServer();
        }
        private  void SetupServer()
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(1);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private  void AppceptCallback(IAsyncResult ar)
        {
            Socket socket = _serverSocket.EndAccept(ar);
            __ClientSockets.Add(new SocketT2h(socket));
            list_Client.Items.Add(socket.RemoteEndPoint.ToString());

            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        
        private  void ReceiveCallback(IAsyncResult ar)
        {
            
            Socket socket = (Socket)ar.AsyncState;
            if (socket.Connected)
            {
                int received;
                try
                {
                    received = socket.EndReceive(ar);
                }
                catch (Exception)
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                        }
                    }
                    return;
                }
                if (received!=0)
                {
                    byte[] dataBuf = new byte[received];
                    Array.Copy(_buffer, dataBuf, received);
                    string text = Encoding.UTF8.GetString(dataBuf);
                    lb_stt.Text = "Text received: " + text;
                    string reponse = string.Empty;

                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                        {
                            String str = __ClientSockets[i]._Socket.RemoteEndPoint.ToString();
                            int find = str.IndexOf(":",0);
                            String str_ip = str.Substring(0, find);
                            rich_Text.AppendText(str_ip + " : " + text + "\n");
                            if (text=="Disconnected!")
                            {
                                list_Client.Items.Remove(__ClientSockets[i]._Socket.RemoteEndPoint.ToString());
                            }
                            if (text.Contains("##"))
                            {
                                for (int k = 0; k < __ClientSockets.Count; k++)
                                {
                                    if (list_Client.Items[k].Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                                    {
                                        list_Client.SetItemChecked(k, false);
                                    }
                                    else
                                    {
                                        list_Client.SetItemChecked(k, true);
                                    }
                                }
                                for (int k = 0; k < list_Client.Items.Count; k++)
                                {
                                    if (list_Client.GetItemCheckState(k) == CheckState.Checked)
                                    {
                                        Sendata(__ClientSockets[k]._Socket, text);
                                    }
                                }
                            }
                        }
                    }
                    
                }
                else
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                        }
                    }
                }
            }
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
        void Sendata(Socket socket,string noidung)
        {
            byte[] data = Encoding.UTF8.GetBytes(noidung);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private  void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(txt_Text.Text != "")
            {
                for (int i = 0; i < list_Client.Items.Count; i++)
                {
                    if (list_Client.GetItemCheckState(i) == CheckState.Checked)
                    {
                        Sendata(__ClientSockets[i]._Socket, txt_Text.Text);
                    }
                }
                rich_Text.AppendText("Server: " + txt_Text.Text + "\n");
                txt_Text.Text = "";
            }
        }
    }
}
