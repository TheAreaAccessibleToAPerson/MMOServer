#define CSL
#define INFO
#define EX

using System.Net;
using System.Net.Sockets;
using Butterfly;

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
                Console("!!!!!!!!!!!!!!!!!!!!!");
                if (ConnectionState.SetReceiveLoginAndPassword
                    (out string receiveLoginAndPasswordError))
                {
                    Task.Run(() => TimeDelay(I_receiveSSL.To, 2000));
                }
                else Destroy(receiveLoginAndPasswordError);
            }
            else if (ConnectionState.HasReceiveLoginAndPassword())
            {
                if (ConnectionState.SetAuthorization
                    (out string setAuthorizationError))
                {
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
                                destroy();
                        }, 2000);
                    });
                }
                else Destroy(setCreatingTCPConnectionError);
            }
            else if (ConnectionState.HasCreatingTCPConnection())
            {
                if (ConnectionState.SetUnsubscribeReceiveTCPConnection
                    (out string setUnsubscribeReceiveTCPConnection))
                {
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
                                destroy();

                        }, 10000);
                    });
                }
                else Destroy(setCreatingUDPConnectionError);
            }
            else if (ConnectionState.HasCreatingUDPConnection())
            {
                if (ConnectionState.SetUnsubscribeReceiveFirstUDPPacket
                    (out string setUnsubscribeReceiveFirstUDPPacketError))
                {
                    I_unsubscribeToReceiveFirstUDPPacket.To
                        (((IPEndPoint)Field.Client.RemoteEndPoint).Address.ToString());
                }
                else Destroy(setUnsubscribeReceiveFirstUDPPacketError);
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
        public void Receive(byte[] message, string address, int port)
        {
        }

        public void Receive(TcpClient tcpConnection)
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