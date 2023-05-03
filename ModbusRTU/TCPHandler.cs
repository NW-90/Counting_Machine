
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DiawModbus
{
  
    internal class TCPHandler
    {
        private NetworkConnectionParameter networkConnectionParameter;
        private TcpListener server = null;
        private List<Client> tcpClientLastRequestList = new List<Client>();
        public event DataChanged dataChanged;
        public event NumberOfClientsChanged numberOfClientsChanged;
        public TCPHandler(int port)
        {
            IPAddress any = IPAddress.Any;
            this.server = new TcpListener(any, port);
            this.server.Start();
            this.server.BeginAcceptTcpClient(new AsyncCallback(this.AcceptTcpClientCallback), null);
        }

        private void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient = this.server.EndAcceptTcpClient(asyncResult);
                tcpClient.ReceiveTimeout = 0xfa0;
            }
            catch (Exception)
            {
            }
            try
            {
                this.server.BeginAcceptTcpClient(new AsyncCallback(this.AcceptTcpClientCallback), null);
                Client state = new Client(tcpClient);
                NetworkStream networkStream = state.NetworkStream;
                networkStream.ReadTimeout = 0xfa0;
                networkStream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(this.ReadCallback), state);
            }
            catch (Exception)
            {
            }
        }
        public void Disconnect()
        {
            try
            {
                foreach (Client client in this.tcpClientLastRequestList)
                {
                    client.NetworkStream.Close(0);
                }
            }
            catch (Exception)
            {
            }
            this.server.Stop();
        }
        private int GetAndCleanNumberOfConnectedClients(Client client)
        {
            bool flag = false;
            foreach (Client client2 in this.tcpClientLastRequestList)
            {
                if (client.Equals(client2))
                {
                    flag = true;
                }
            }
            try
            {
                this.tcpClientLastRequestList.RemoveAll(c => (DateTime.Now.Ticks - c.Ticks) > 0x2625a00L);
            }
            catch (Exception)
            {
            }
            if (!flag)
            {
                this.tcpClientLastRequestList.Add(client);
            }
            return this.tcpClientLastRequestList.Count;
        }
        private void ReadCallback(IAsyncResult asyncResult)
        {
            Client asyncState = asyncResult.AsyncState as Client;
            asyncState.Ticks = DateTime.Now.Ticks;
            this.NumberOfConnectedClients = this.GetAndCleanNumberOfConnectedClients(asyncState);
            if (this.numberOfClientsChanged != null)
            {
                this.numberOfClientsChanged();
            }
            if (asyncState != null)
            {
                int num;
                NetworkStream networkStream = null;
                try
                {
                    networkStream = asyncState.NetworkStream;
                    num = networkStream.EndRead(asyncResult);
                }
                catch (Exception)
                {
                    return;
                }
                if (num != 0)
                {
                    byte[] dst = new byte[num];
                    Buffer.BlockCopy(asyncState.Buffer, 0, dst, 0, num);
                    this.networkConnectionParameter.bytes = dst;
                    this.networkConnectionParameter.stream = networkStream;
                    if (this.dataChanged != null)
                    {
                        this.dataChanged(this.networkConnectionParameter);
                    }
                    try
                    {
                        networkStream.BeginRead(asyncState.Buffer, 0, asyncState.Buffer.Length, new AsyncCallback(this.ReadCallback), asyncState);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        public int NumberOfConnectedClients { get; set; }
        internal class Client
        {

            private readonly byte[] buffer;
            private readonly System.Net.Sockets.TcpClient tcpClient;

            public Client(System.Net.Sockets.TcpClient tcpClient)
            {
                this.tcpClient = tcpClient;
                int receiveBufferSize = tcpClient.ReceiveBufferSize;
                this.buffer = new byte[receiveBufferSize];
            }

            public byte[] Buffer
            {
                get
                {
                    return this.buffer;
                }
            }

            public System.Net.Sockets.NetworkStream NetworkStream
            {
                get
                {
                    return this.tcpClient.GetStream();
                }
            }

            public System.Net.Sockets.TcpClient TcpClient
            {
                get
                {
                    return this.tcpClient;
                }
            }

            public long Ticks { get; set; }
        }
        public delegate void DataChanged(object networkConnectionParameter);
        public delegate void NumberOfClientsChanged();
    }
}

