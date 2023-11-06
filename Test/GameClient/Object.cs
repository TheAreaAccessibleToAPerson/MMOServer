namespace gameClient
{
    public sealed class Client : ShellController
    {
        private readonly manager.Connection _connection = new();
        private readonly manager.Thread _thread;

        public Client()
        {
            _thread = new(11, _connection._sslManager, 111,
                _connection._tcpManager, 111, _connection._udpManager, StopThread);
        }

        public void Start(string login, string password, string sslAddress, int sslPort, 
            int tcpPort, int udpPort)
        {
            _thread.Start();

            _connection.Start(login, password, sslAddress, sslPort);
        }

        private void StopThread()
        {
        }
    }
}