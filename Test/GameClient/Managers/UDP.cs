using System.Net.Sockets;

namespace gameClient.manager
{
    public sealed class UDP : Socket, Thread.IUpdate
    {
        private readonly string _address;
        private readonly int _port;

        public UDP(string address, int port) 
            : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            _address = address;
            _port = port;
        }

        void Thread.IUpdate.Update()
        {
        }

        public void SendFirstPacket()
        {
        }
    }
}
