namespace gameClient.manager
{
    public sealed class Thread
    {
        private readonly IUpdate _ssl; private readonly int _sslTimeDelay;
        private readonly IUpdate _tcp; private readonly int _tcpTimeDelay;
        private readonly IUpdate _udp;

        private readonly int _timeDelay;

        private readonly System.Threading.Thread _thread;

        private System.DateTime d_sslDateTime = System.DateTime.Now;
        private System.DateTime d_tcpDateTime = System.DateTime.Now;

        private bool _isRunning = false;

        private readonly Action _continueEnd;

        public Thread(int timeDelay, IUpdate sslUpdate, int sslTimeDelay,
            IUpdate tcpUpdate, int tcpTimeDelay, IUpdate udpUpdate, Action _continueEnd)
        {
            _ssl = sslUpdate; _sslTimeDelay = sslTimeDelay;
            _tcp = tcpUpdate; _tcpTimeDelay = tcpTimeDelay;
            _udp = udpUpdate;

            _timeDelay = timeDelay;

            _thread = new(Update);
        }

        public void Start()
        {
            _isRunning = true;

            _thread.Start();
        }

        private void Update()
        {
            while (_isRunning)
            {
                int sslDelta = (System.DateTime.Now.Subtract(d_sslDateTime).Seconds * 1000)
                    + System.DateTime.Now.Subtract(d_sslDateTime).Milliseconds;

                if (sslDelta >= _sslTimeDelay)
                {
                    d_sslDateTime = System.DateTime.Now;

                    _ssl.Update();
                }

                int tcpDelta = (System.DateTime.Now.Subtract(d_tcpDateTime).Seconds * 1000)
                    + System.DateTime.Now.Subtract(d_tcpDateTime).Milliseconds;

                if (tcpDelta >= _tcpTimeDelay)
                {
                    d_tcpDateTime = System.DateTime.Now;

                    _tcp.Update();
                }

                _udp.Update();

                System.Threading.Thread.Sleep(_timeDelay);
            }

            _continueEnd();
        }

        private void Stop()
        {
            _isRunning = false;
        }

        public interface IUpdate 
        {
            void Update();
        }
    }
}
