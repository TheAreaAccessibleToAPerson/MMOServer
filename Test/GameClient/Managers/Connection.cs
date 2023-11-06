namespace gameClient.manager 
{
    public sealed class Connection : Connection.ISSL
    {
        public readonly SSL _sslManager;
        public readonly TCP _tcpManager;
        public readonly UDP _udpManager;

        public Connection()
        {
            _udpManager = new(Header1.ADDRESS, Header1.UDP_PORT);

            _tcpManager = new(Header1.ADDRESS, Header1.TCP_PORT);
            _sslManager = new(this, _tcpManager);
        }

        public void Start(string login, string password, 
            string sslAddress, int sslPort)
        {
            if (_sslManager.Connect(sslAddress, sslPort))
            {
                _sslManager.Authorization(login, password);
            }
        }

        void ISSL.EndConnection(int result)
        {
        }

        public interface ISSL 
        {
            void EndConnection(int result);
        }
    }
}