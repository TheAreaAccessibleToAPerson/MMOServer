using Butterfly;

namespace server.component.clientManager.component
{
    public abstract class ConnectController : Butterfly.Controller
    {
        public interface IReceiveUDPPackets
        {
            /// <summary>
            /// Данный метод реализует прослушку UDP пакетов от клиeнта.
            /// </summary>
            /// <param name="message"></param>
            void Receive(byte[] message);
        }
    }
}