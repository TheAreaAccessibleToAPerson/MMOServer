namespace server 
{
    public sealed class Settings 
    {
        public readonly ListenSSLClients SSLListenClientsSetting;
        public readonly ListenTCPClients TCPListenClientsSetting;
        public readonly ReceiveUDP ReceiveUDPPacketsSetting;

        public Settings(string sslAddress, int sslPort, uint sslTimeDelay,
            string tcpAddress, int tcpPort, uint tcpTimeDelay,
            string udpAddress, int udpPort, uint udpTimeDelay)
        {
            SSLListenClientsSetting = new ListenSSLClients(sslAddress, sslPort, sslTimeDelay);
            TCPListenClientsSetting = new ListenTCPClients(tcpAddress, tcpPort, tcpTimeDelay);
            ReceiveUDPPacketsSetting = new ReceiveUDP(udpAddress, udpPort, udpTimeDelay);
        }

        public class ListenSSLClients
        {
            public readonly string Address;
            public readonly int Port;
            public readonly uint ThreadTimeDelay;

            public ListenSSLClients(string address, int port, uint threadTimeDelay)
            {
                Address = address;
                Port = port;

                ThreadTimeDelay = threadTimeDelay;
            }
        }

        public class ListenTCPClients
        {
            public readonly string Address;
            public readonly int Port;
            public readonly uint ThreadTimeDelay;

            public ListenTCPClients(string address, int port, uint threadTimeDelay)
            {
                Address = address;
                Port = port;

                ThreadTimeDelay = threadTimeDelay;
            }
        }

        public class ReceiveUDP 
        {
            public readonly string Address;
            public readonly int Port;
            public readonly uint ThreadTimeDelay;

            public ReceiveUDP(string address, int port, uint threadTimeDelay)
            {
                Address = address;
                Port = port;

                ThreadTimeDelay = threadTimeDelay;
            }
        }
    }
}