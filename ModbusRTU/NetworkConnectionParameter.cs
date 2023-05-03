namespace DiawModbus
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NetworkConnectionParameter
    {
        public NetworkStream stream;
        public byte[] bytes;
        public int portIn;
        public IPAddress ipAddressIn;
    }
}

