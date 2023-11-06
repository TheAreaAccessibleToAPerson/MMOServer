using System.Net.Sockets;

namespace gameClient.manager.handler
{
    public abstract class Packet : Socket
    {
        protected Packet(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) 
            : base(addressFamily, socketType, protocolType)
        {
        }
    }
}