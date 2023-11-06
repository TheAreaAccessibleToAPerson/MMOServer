#define INFO

using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace gameClient.manager
{
    public sealed class SSL : handler.Message, Thread.IUpdate
    {
        private readonly Socket _TCPSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        private readonly ConcurrentQueue<byte[]> _messages = new();
        private int _messagesCount = 0;

        private readonly Connection.ISSL _connectionResult;
        private readonly TCP.IConnection _tcpConnection;
        private readonly UDP.IConnection _udpConnection;

        private int IDConnection = -1;

        /// <summary>
        /// Соединение установлено.
        /// </summary>
        private bool _isConnect = false;

        public SSL(Connection.ISSL connectionResult, TCP.IConnection tcpConnection, 
            UDP.IConnection udpConnection) 
            : base("SSL//:")
        {
            _connectionResult = connectionResult;
            _tcpConnection = tcpConnection;
            _udpConnection = udpConnection;
        }

        #region Update

        void Thread.IUpdate.Update()
        {
            if (_messages.Count > 0)
            {
                for (int i = 0; i < _messagesCount; i++)
                {
                    if (_messages.TryDequeue(out byte[] buffer))
                    {
#if INFO
                        SystemInformation($"SEND:\n" + string.Join(" ", buffer));
#endif

                        _TCPSocket.Send(buffer);

                        Interlocked.Decrement(ref _messagesCount);
                    }
                }
            }

            int available = _TCPSocket.Available;
            if (available > 0)
            {
                do
                {
                    byte[] buffer = new byte[8192];

                    int count = _TCPSocket.Receive(buffer);

                    Process(buffer, count);

                    available -= count;
                }
                while (available > 0);
            }
        }

        #endregion

        #region Output

        public void Authorization(string login, string password)
        {
            SystemInformation($"Отправляем Login:{login}, Password:{password}.", ConsoleColor.Yellow);

            if (login.Length > ssl.Data.ClientToServer.Connection.Step.LOGIN_LENGTH)
                SystemInformation("Длина логина привышена.", ConsoleColor.Yellow);

            if (password.Length > ssl.Data.ClientToServer.Connection.Step.PASSWORD_LENGTH)
                SystemInformation("Длина пароля привышена.", ConsoleColor.Yellow);

            byte[] message = new byte[ssl.Data.ClientToServer.Connection.Step.LENGTH];
            {
                message[ssl.Header.DATA_LENGTH_INDEX_1byte]
                    = ssl.Data.ClientToServer.Connection.Step.LENGTH >> 8;
                message[ssl.Header.DATA_LENGTH_INDEX_2byte]
                    = ssl.Data.ClientToServer.Connection.Step.LENGTH;

                message[ssl.Header.DATA_TYPE_INDEX_1byte]
                    = ssl.Data.ClientToServer.Connection.Step.TYPE >> 8;
                message[ssl.Header.DATA_TYPE_INDEX_2byte]
                    = ssl.Data.ClientToServer.Connection.Step.TYPE;
            }

            byte[] l = Encoding.ASCII.GetBytes(login); int loginIndex = 0;
            for (int i = ssl.Data.ClientToServer.Connection.Step.LOGIN_START_INDEX;
                i < (ssl.Data.ClientToServer.Connection.Step.LOGIN_START_INDEX + l.Length); i++)
                message[i] = l[loginIndex++];

            byte[] p = Encoding.ASCII.GetBytes(password); int passwordIndex = 0;
            for (int i = ssl.Data.ClientToServer.Connection.Step.PASSWORD_START_INDEX;
                i < (ssl.Data.ClientToServer.Connection.Step.PASSWORD_START_INDEX + p.Length); i++)
                message[i] = p[passwordIndex++];

            _messages.Enqueue(message); Interlocked.Increment(ref _messagesCount);
        }

        #endregion

        #region Input

        public void Process(byte[] message, int size)
        {
            byte[][] messages = SplitTCPMessage(message, size);

            if (messages.Length > 0)
            {
                for (int i = 0; i < messages.Length; i++)
                {
                    int type = messages[i][ssl.Header.DATA_TYPE_INDEX_1byte] << 8 ^
                        messages[i][ssl.Header.DATA_TYPE_INDEX_2byte];


                    if (type == ssl.Data.ServerToClient.Connection.Step1.TYPE)
                    {
                        int result = messages[i][ssl.Data.ServerToClient.Connection.Step1.RESULT_INDEX];

                        SystemInformation(result.ToString());

                        if (result == ssl.Data.ServerToClient.Connection.Step1.Result.SUCCESS)
                        {
                            // Извлекаем ID для дальнейшей работы.
                            IDConnection = messages[i][ssl.Data.ServerToClient.Connection.Step1.RECEIVE_ID_INDEX_1byte] << 24 ^
                                messages[i][ssl.Data.ServerToClient.Connection.Step1.RECEIVE_ID_INDEX_2byte] << 16 ^
                                messages[i][ssl.Data.ServerToClient.Connection.Step1.RECEIVE_ID_INDEX_3byte] << 8 ^
                                messages[i][ssl.Data.ServerToClient.Connection.Step1.RECEIVE_ID_INDEX_4byte];
#if INFO
                            SystemInformation($"Вы удачно авторизовались.Ваш ID {IDConnection}.");
#endif

                            // Создаем TCP соединение.
                            _tcpConnection.Connect();
                        }
                    }
                    else if (type == ssl.Data.ServerToClient.Connection.Step2.TYPE)
                    {
                        Task.Run(() => 
                        {
                            int step = 25;

                            while (_isConnect == false && step-- > 0)
                            {
                                _udpConnection.SendFirstPacket(IDConnection);

                                System.Threading.Thread.Sleep(50);
                            }
                        });
                    }
                    else if (type == ssl.Data.ServerToClient.Connection.Step3.TYPE)
                    {
                        _isConnect = true;
                        SystemInformation("Connect");
                    }
#if INFO
                    else
                    {
                        SystemInformation($"Неизвестный тип[{type}] сообщения");
                    }
#endif
                }
            }
#if INFO
            else
            {
                SystemInformation("Непришло неодного сообщения.");
            }
#endif
        }

        #endregion

        #region ConnectionResult

        #endregion

        #region Connect

        public bool Connect(string address, int port)
        {
            try
            {
                SystemInformation("Connect to server ...", ConsoleColor.Yellow);

                _TCPSocket.Connect(address, port);

                return true;
            }
            catch (Exception ex)
            {
                SystemInformation(ex.ToString(), ConsoleColor.Yellow);

                return false;
            }
        }

        #endregion

    }
}