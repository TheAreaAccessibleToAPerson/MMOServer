using System.Net.Sockets;

namespace gameClient.manager.handler
{
    public abstract class Packet : Socket
    {
        private readonly string _address;
        private readonly int _port;
        private bool _isConnect;

        protected Packet(string address, int port)
            : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            _address = address;
            _port = port;
        }

        #region Connection

        public byte[] CreatingFirstPacket(int connectionID)
        {
            return new byte[udp.Data.ClientToServer.Connection.Step.LENGTH]
            {
             /*********************HEADER***********************/
             udp.Data.ClientToServer.Connection.Step.LENGTH >> 8,
             udp.Data.ClientToServer.Connection.Step.LENGTH,

            udp.Data.ClientToServer.Connection.Step.TYPE,
            /**************************************************/

            /*********************DATA*************************/
            (byte)(connectionID >> 24), (byte)(connectionID >> 16),
            (byte)(connectionID >> 8), (byte)connectionID
                /**************************************************/
            };
        }


        #endregion

        protected void SystemInformation(string info)
            => Console.WriteLine(info);
    }
}