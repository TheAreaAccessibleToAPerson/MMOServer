using System.Net.Sockets;

namespace gameClient.manager
{
    public sealed class TCP : Socket, Thread.IUpdate, TCP.IConnection
    {
        private readonly string _address;
        private readonly int _port;

        public TCP(string address, int port) 
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            _address = address;
            _port = port;
        }

        void Thread.IUpdate.Update()
        {
        }

        void IConnection.Connect()
        {
            try 
            {
                Connect(_address, _port);
            }
            catch (Exception ex)
            {
            }
        }

        public interface IConnection 
        {
            public void Connect();
        }
    }
}