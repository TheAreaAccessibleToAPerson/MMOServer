using Butterfly;

namespace server.component.clientManager.component
{
    public abstract class ConnectControl : WorldController,
        ConnectControl.IReceiveUDPPackets
    {
        protected bool _isRunning = false;

        protected void InputSendSSL(byte[] message)
        {
            if (_isRunning)
            {
                try
                {
                    Field.SSL.GetStream().Write(message);
                }
                catch { destroy(); }
            }
        }

        protected void InputSendTCP(byte[] message)
        {
            if (_isRunning)
            {
                try
                {
                    Field.TCP.GetStream().Write(message);
                }
                catch { destroy(); }
            }
        }

        protected void InputSendUDP(byte[] packet)
        {
            if (_isRunning)
            {
                Field.UDP.Send(packet);
            }
        }

        void IReceiveUDPPackets.Receive(byte[] message)
        {
        }

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