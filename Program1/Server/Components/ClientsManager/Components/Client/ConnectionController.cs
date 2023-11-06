#define CSL
#define INFO
#define EX

using System.Net;
using System.Net.Sockets;
using Butterfly;
using gameClient.manager;

namespace server.component.clientManager.component.clientShell
{
    public abstract class ConnectionController : Handler.Messages,
        ConnectionController.IReceiveFirstUDPPacket, ConnectionController.IReceiveTCPConnection
    {
#if CSL
        /// <summary>
        /// Отправка сообщений в логгер.
        /// </summary>
        protected IInput<string> i_logger;

        protected void _logger(string info)
        {
            if (StateInformation.IsCallConstruction)
                i_logger.To($"{GetExplorer()}:{info}");
        }

#endif

        protected IInput I_process;

        /// <summary>
        /// Прослушиваем SSL.
        /// </summary>
        protected IInput I_receiveSSL;

        /// <summary>
        /// Прослушиваем SSL.
        /// </summary>
        protected IInput<byte[]> I_sendSSL;

        /// <summary>
        /// Верификация пользователя.
        /// </summary>
        protected IInput<information.Client> I_verificationBD;

        /// <summary>
        /// Ожидает первый UDP пакет.
        /// </summary>
        protected IInput<string, IReceiveFirstUDPPacket> I_subscribeToReceiveFirstUDPPacket;

        /// <summary>
        /// Ожидает первый UDP пакет.
        /// </summary>
        protected IInput<string> I_unsubscribeToReceiveFirstUDPPacket;

        /// <summary>
        /// Ожидает новое TCP соединение.
        /// </summary>
        protected IInput<string, IReceiveTCPConnection> I_subscribeReceiveToTCPConnection;

        /// <summary>
        /// Отписывается и больше не ожидает нового подключения.
        /// </summary>
        protected IInput<string> I_unsubscribeReceiveToTCPConnection;

        /// <summary>
        /// Отправляет данные клиeнта в бд.
        /// </summary>
        protected IInput<information.Client> I_verification;

        /// <summary>
        /// Текущее состояние подключения.
        /// </summary>
        /// <returns></returns>
        protected readonly information.State ConnectionState = new();
        protected information.Client ClientInformation;

        /// <summary>
        /// Данный ID должен придти в первом UDP пакете.
        /// </summary>
        private int ConnectionID = 155;

        private TcpClient _tcpConnection;

        protected void Process()
        {
            if (ConnectionState.IsDestroy()) return;

            if (ConnectionState.HasNone())
            {
                if (ConnectionState.SetReceiveLoginAndPassword
                    (out string receiveLoginAndPasswordError))
                {
#if CSL
                    _logger("Сервер ожидает логина и пароля от клинта.");
#endif

                    Task.Run(() => TimeDelay(I_receiveSSL.To, 1000));
                }
                else Destroy(receiveLoginAndPasswordError);
            }
            else if (ConnectionState.HasReceiveLoginAndPassword())
            {
                if (ConnectionState.SetAuthorization
                    (out string setAuthorizationError))
                {
#if CSL
                    _logger("Логин и пароль получен, проводим вирификацию данных.");
#endif
                    I_verificationBD.To(ClientInformation);
                }
                else Destroy(setAuthorizationError);
            }
            else if (ConnectionState.HasAuthorization())
            {
                if (ClientInformation.IsSuccessVerification())
                {
                    if (ConnectionState.SetSubscribeReceiveTCPConnection
                        (out string setRegisterReceiveTCPConnectionError))
                    {
#if CSL
                        _logger("Регистрируем ожидание нового TCP соединения.");
#endif
                        // Регистрируемся и ожидаем новое TCP соединение.
                        I_subscribeReceiveToTCPConnection.To
                            (((IPEndPoint)Field.Client.RemoteEndPoint).Address.ToString(), this);
                    }
                    else Destroy(setRegisterReceiveTCPConnectionError);
                }
                else destroy();
            }
            else if (ConnectionState.HasSubscribeReceiveTCPConnection())
            {
                if (ConnectionState.SetCreatingTCPConnection
                    (out string setCreatingTCPConnectionError))
                {
#if CSL
                    _logger("Сообщаем клинту что бы тот подключился по TCP.");
#endif
                    I_sendSSL.To(new byte[ssl.Data.ServerToClient.Connection.Step1.LENGTH]
                    {
                        /*********************HEADER***********************/
                        ssl.Data.ServerToClient.Connection.Step1.LENGTH >> 8,
                        ssl.Data.ServerToClient.Connection.Step1.LENGTH,

                        ssl.Data.ServerToClient.Connection.Step1.TYPE >> 8,
                        ssl.Data.ServerToClient.Connection.Step1.TYPE,
                        /**************************************************/

                        /*********************DATA*************************/
                        ssl.Data.ServerToClient.Connection.Step1.Result.SUCCESS,

                        (byte)(ConnectionID << 24), (byte)(ConnectionID << 16),
                        (byte)(ConnectionID << 8), (byte)ConnectionID,
                        /**************************************************/
                    });

                    Task.Run(() =>
                    {
                        TimeDelay(() =>
                        {
                            if (ConnectionState.HasCreatingTCPConnection())
                                Destroy("Превышен временой лимит создания TCP соединения.");
                        }, 1000);
                    });
                }
                else Destroy(setCreatingTCPConnectionError);
            }
            else if (ConnectionState.HasCreatingTCPConnection())
            {
                if (ConnectionState.SetUnsubscribeReceiveTCPConnection
                    (out string setUnsubscribeReceiveTCPConnection))
                {
#if CSL
                    _logger("Отписываемся из места ожидания новых TCP поключений.");
#endif
                    I_unsubscribeReceiveToTCPConnection.To
                        (((IPEndPoint)Field.Client.RemoteEndPoint).Address.ToString());
                }
                else Destroy(setUnsubscribeReceiveTCPConnection);
            }
            else if (ConnectionState.HasUnsubscribeReceiveTCPConnection())
            {
                if (ConnectionState.SetSubscribeReceiveFirstUDPPacket
                    (out string setRegistractionReceiveUDPConnectionError))
                {
#if CSL
                    _logger("Подписываемся и ожидаем первый UDP пакет.");
#endif
                    I_subscribeToReceiveFirstUDPPacket.To
                        (((IPEndPoint)Field.Client.RemoteEndPoint).Address.ToString(), this);
                }
                else Destroy(setRegistractionReceiveUDPConnectionError);
            }
            else if (ConnectionState.HasSubscribeReceiveFirstUDPPacket())
            {
                if (ConnectionState.SetCreatingUDPConnection
                    (out string setCreatingUDPConnectionError))
                {
#if CSL
                    _logger("Сообщаем клинту что бы тот отправил первый UDP пакет.");
#endif
                    I_sendSSL.To(new byte[ssl.Data.ServerToClient.Connection.Step2.LENGTH]
                    {
                        /*********************HEADER***********************/
                        ssl.Data.ServerToClient.Connection.Step2.LENGTH >> 8,
                        ssl.Data.ServerToClient.Connection.Step2.LENGTH,

                        ssl.Data.ServerToClient.Connection.Step2.TYPE >> 8,
                        ssl.Data.ServerToClient.Connection.Step2.TYPE,
                        /**************************************************/
                    });

                    Task.Run(() =>
                    {
                        TimeDelay(() =>
                        {
                            if (ConnectionState.HasCreatingUDPConnection())
                                Destroy("Превышен временой лимит создания UDP соединения.");

                        }, 1000);
                    });
                }
                else Destroy(setCreatingUDPConnectionError);
            }
            else if (ConnectionState.HasCreatingUDPConnection())
            {
                if (ConnectionState.SetUnsubscribeReceiveFirstUDPPacket
                    (out string setUnsubscribeReceiveFirstUDPPacketError))
                {
#if CSL
                    _logger("Отписываемся из места получения первого UDP пакета.");
#endif
                    I_unsubscribeToReceiveFirstUDPPacket.To
                        (((IPEndPoint)Field.Client.RemoteEndPoint).Address.ToString());
                }
                else Destroy(setUnsubscribeReceiveFirstUDPPacketError);
            }
            else if (ConnectionState.HasUnsubscribeReceiveFirstUDPPacket())
            {
                if (ConnectionState.SetConnect(out string setConnectError))
                {
#if CSL
                    _logger("Connect.");
#endif
                    // Проинформируем о что соединение установленно.
                    I_sendSSL.To(new byte[ssl.Data.ServerToClient.Connection.Step3.LENGTH]
                    {
                        /*********************HEADER***********************/
                        ssl.Data.ServerToClient.Connection.Step3.LENGTH >> 8,
                        ssl.Data.ServerToClient.Connection.Step3.LENGTH,

                        ssl.Data.ServerToClient.Connection.Step3.TYPE >> 8,
                        ssl.Data.ServerToClient.Connection.Step3.TYPE,
                        /**************************************************/
                    });
                }
                else Destroy(setConnectError);
            }
        }

        protected async void TimeDelay(Action action, int timeDelay)
        {
            await Task.Delay(timeDelay);

            action.Invoke();
        }
        protected void InputReceive()
        {
            try
            {
                byte[] buffer = new byte[256];

                if (Field.GetStream().Socket.Available > 0)
                {
#if INFO
                    SystemInformation($"State:{ConnectionState}." +
                        "Сообщение получено.");
#endif
                    int size = Field.GetStream().Read(buffer);

                    TCPProcess(buffer, size);
                }
                else
                {
#if INFO
                    SystemInformation($"State:{ConnectionState}." +
                        "Время ожидания сообщения истекло истекло");
#endif
                    destroy();
                }
            }
            catch { destroy(); }
        }
        protected void InputSend(byte[] message)
        {
            try
            {
                Field.GetStream().Write(message);
            }
            catch { destroy(); }
        }
        public void TCPProcess(byte[] message, int size)
        {
#if INFO
            SystemInformation($"В обработчик TCP сообщений поступил массив байт длиной {message.Length}.\n" +
                $"{String.Join(" ", message)}", ConsoleColor.Green);
#endif

            byte[][] messages = SplitTCPMessage(message, size);

            if (messages.Length > 0)
            {
                for (int i = 0; i < messages.Length; i++)
                {
                    // В массиве как мимум находится заголовок SSL сообщения.
                    // Сразу после заголовка идет Type сообщения.
                    // Проверим есть ли он там.
                    if (messages[i].Length < ssl.Header.LENGTH + 1) continue;

                    int sslMessageType = messages[i][ssl.Header.DATA_TYPE_INDEX_1byte] >> 8 ^
                        messages[i][ssl.Header.DATA_TYPE_INDEX_2byte];

#if INFO
                    SystemInformation($"Message type number:{sslMessageType}");
#endif

                    // Логин и пароль.
                    if (sslMessageType == ssl.Data.ClientToServer.Connection.Step.TYPE)
                    {
#if INFO
                        SystemInformation("Message connection(in login and password).\n" +
                            string.Join(" ", messages[i]));
#endif
                        // Проверяем длину.
                        if (messages[i].Length == ssl.Data.ClientToServer.Connection.Step.LENGTH)
                        {
                            string login = "";
                            for (int m = ssl.Data.ClientToServer.Connection.Step.LOGIN_START_INDEX;
                                m < (ssl.Data.ClientToServer.Connection.Step.LOGIN_START_INDEX +
                                    ssl.Data.ClientToServer.Connection.Step.LOGIN_LENGTH); m++)
                                login += Convert.ToChar(messages[i][m]).ToString();

                            if (ClientInformation.SetLogin(login, out string setLoginError))
                            {
                                string password = "";
                                for (int m = ssl.Data.ClientToServer.Connection.Step.PASSWORD_START_INDEX;
                                    m < (ssl.Data.ClientToServer.Connection.Step.PASSWORD_START_INDEX +
                                        ssl.Data.ClientToServer.Connection.Step.PASSWORD_LENGTH); m++)
                                    password += Convert.ToChar(messages[i][m]).ToString();

                                if (ClientInformation.SetPassword(password, out string setPasswordError))
                                {
                                    Process();
                                }
                                else Destroy(setPasswordError);
                            }
                            else Destroy(setLoginError);
                        }
                    }
                    else
                    {
#if INFO
                        SystemInformation($"В поступившем сообщении необнаружено не одного системного.",
                            ConsoleColor.Red);
#endif

                        destroy();

                    }
                }
            }
            else
            {
#if INFO
                SystemInformation($"В поступившем сообщении необнаружено не одного системного.",
                    ConsoleColor.Red);
#endif
                destroy();
            }
        }

        void IReceiveFirstUDPPacket.Receive(byte[] message, string address, int port)
        {
            if (message.Length >= udp.Header.LENGTH)
            {
                if (message[udp.Header.DATA_TYPE_INDEX] == udp.Data.ClientToServer.Connection.Step.TYPE)
                {
                    if (message.Length == udp.Data.ClientToServer.Connection.Step.LENGTH)
                    {
                        int id = message[udp.Data.ServerToClient.Connection.Step.RECEIVE_ID_1byte] << 24 ^
                            message[udp.Data.ServerToClient.Connection.Step.RECEIVE_ID_2byte] << 16 ^
                            message[udp.Data.ServerToClient.Connection.Step.RECEIVE_ID_3byte] << 8 ^
                            message[udp.Data.ServerToClient.Connection.Step.RECEIVE_ID_4byte];

                        if (id == ConnectionID) Process();
                    }
                }
            }
        }

        void IReceiveTCPConnection.Receive(TcpClient tcpConnection)
        {
#if INFO
            SystemInformation("Клиент пролучил новое TCP соединение.");
#endif
            _tcpConnection ??= tcpConnection;

            Process();
        }
        private void Destroy(string info)
        {
            destroy();
        }

        public interface IReceiveTCPConnection
        {
            void Receive(TcpClient tcpConnection);
        }

        public interface IReceiveFirstUDPPacket
        {
            void Receive(byte[] message, string address, int port);
        }
    }
}