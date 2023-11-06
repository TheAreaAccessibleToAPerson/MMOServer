using System.Collections.Concurrent;

namespace gameClient.manager
{
    public sealed class UDP : handler.Packet, Thread.IUpdate, UDP.IConnection
    {
        private readonly string _address;
        private readonly int _port;
        private bool _isConnect;

        private readonly ConcurrentQueue<byte[]> _messages = new();
        private int _messagesCount = 0;

        public UDP(string address, int port) : base(address, port)
        {
            _address = address;
            _port = port;
        }

        void Thread.IUpdate.Update()
        {
            if (_isConnect == false) return;

            if (_messages.Count > 0)
            {
                for (int i = 0; i < _messagesCount; i++)
                {
                    if (_messages.TryDequeue(out byte[] buffer))
                    {
#if INFO
                        SystemInformation($"SEND:\n" + string.Join(" ", buffer));
#endif
                        Send(buffer);

                        Interlocked.Decrement(ref _messagesCount);
                    }
                }
            }

            int available = Available;
            if (available > 0)
            {
                do
                {
                    byte[] buffer = new byte[8192];
                    int count = Receive(buffer);

                    Process(buffer, count);

                    available -= count;
                }
                while (available > 0);
            }
        }

        private void Process(byte[] buffer, int count)
        {
        }

        public void SendFirstPacket(int connectionID)
        {
            if (_isConnect == false)
            {
                try
                {
                    Connect(_address, _port);

                    _isConnect = true;
                }
                catch (Exception ex)
                {
                    SystemInformation(ex.ToString());
                }
            }

            if (_isConnect)
            {
                _messages.Enqueue(CreatingFirstPacket(connectionID)); 
                Interlocked.Increment(ref _messagesCount);
            }
        }

        public interface IConnection
        {
            void SendFirstPacket(int connectionID);
        }
    }
}
