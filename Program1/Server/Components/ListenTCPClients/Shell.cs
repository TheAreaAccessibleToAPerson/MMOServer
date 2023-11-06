#define CSL
#define INFO

using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace server.component
{
    /// <summary>
    /// Оболочка обьекта прослушивающего подключающихся клинтов.
    /// </summary>
    public sealed class ListenTCPClientsShell : Controller.LocalField<Settings.ListenTCPClients>
    {
        public const string NAME = "ListenTCPClients:Shell:";

        public struct BUS
        {
            public struct Impuls
            {
                /// <summary>
                /// Прослушка запущена.
                /// </summary>
                public const string START = NAME + "Start";

                /// <summary>
                /// Попытка перезапустить прослушку.
                /// </summary>
                public const string RESTART = NAME + "Restart";
            }

            public struct Message
            {
                public const string ADD_TCP_CONNECT = NAME + "Add Tcp connect";
            }

            public struct Echo
            {
                /// <summary>
                /// Регистрируемся и ожидаем TCP соединения от клиента.
                /// </summary>
                public const string SUBSCRIBE_TO_RECEIVE_CONNECTION
                    = NAME + "Subscribe to receive connection";

                /// <summary>
                /// Отписываемся из ожидания TCP соединения от клиента.
                /// </summary>
                public const string UNSUBSCRIBE_TO_RECEIVE_CONNECTION
                    = NAME + "Unsubscribe to receive connection";
            }
        }

        /// <summary>
        /// Максимальное количесво попыток перезапуска обьекта.
        /// </summary>
        private const int MAX_NUMBER_OF_ATTEMPTS_RESTARTING = 25;

        /// <summary>
        /// Номер текущей попытки перезапуска.
        /// </summary>
        private int _currentAttemptsRestarting = 0;

#if CSL
        /// <summary>
        /// Отправляет сообщение в раздел логгера 
        /// хранящего информацию о компонентах сервера.
        /// </summary>
        private IInput<string> i_componentLogger;
#endif

        private const string LOG = "Обьект прослушивающий подключающихся клиeнтов отчитался";

        private readonly Dictionary<string,
            clientManager.component.clientShell.ConnectionController.IReceiveTCPConnection>
             _receiveTCPConnection = new();

        void Construction()
        {
#if CSL
            send_message(ref i_componentLogger, Logger.BUS.Message.CLIENT_SHELL_COMPONENT);
#endif

            listen_echo_2_0<string, clientManager.component.clientShell.ConnectionController.IReceiveTCPConnection>
                (BUS.Echo.SUBSCRIBE_TO_RECEIVE_CONNECTION)
                .output_to((key, client, @return) =>
                {
                    if (_receiveTCPConnection.ContainsKey(key))
                    {
#if CSL
                        _logger($"Клиент {@return.GetKey()} уже подписан по ключу {key} и ожидает соединения.");
#endif
                    }
                    else
                    {
#if INFO
                        SystemInformation($"Клиент {@return.GetKey()} подписался и ожидается соединение.");
#endif

                        _receiveTCPConnection.Add(key, client);

                        @return.To();
                    }
                },
                Header1.Event.WORK_OBJECT);

            listen_echo_1_0<string>(BUS.Echo.UNSUBSCRIBE_TO_RECEIVE_CONNECTION)
                .output_to((key, @return) =>
                {
                    if (_receiveTCPConnection.Remove(key))
                    {
#if INFO
                        SystemInformation($"Клиент {@return.GetKey()} отписался.");
#endif

                        @return.To();
                    }
                    else
                    {
#if CSL
                        _logger($"Клиент {@return} не был подписан по ключу {key} и не ожидает соединения.");
#endif
                    }
                },
                Header1.Event.WORK_OBJECT);

            listen_message<TcpClient>(BUS.Message.ADD_TCP_CONNECT)
                .output_to((tcpConnect) =>
                {
                    string address = ((IPEndPoint)tcpConnect.Client.RemoteEndPoint).Address.ToString();

                    if (_receiveTCPConnection.TryGetValue(address,
                        out clientManager.component.clientShell.ConnectionController.IReceiveTCPConnection client))
                    {
#if INFO
                        SystemInformation($"Получено новое TCP подключение {address}.");
#endif

                        client.Receive(tcpConnect);
                    }
#if INFO
                    else
                    {
                        SystemInformation($"Ни один клиент не ожидает нового TCP соединение {address}.");
                    }
#endif
                },
                Header1.Event.WORK_OBJECT);

            listen_impuls(BUS.Impuls.START)
                .output_to((infoObj) =>
                {
#if CSL
                    _logger($"{LOG} о начале работы в свою оболочку.");
#endif

                    _currentAttemptsRestarting = 0;
                });

            listen_impuls(BUS.Impuls.RESTART)
                .output_to((infoObj) =>
                {
                    if (StateInformation.IsDestroy) return;

                    if (_currentAttemptsRestarting++ < MAX_NUMBER_OF_ATTEMPTS_RESTARTING)
                    {
#if CSL
                        _logger($"{LOG} запросил свой перезапуск " +
                            $"{_currentAttemptsRestarting}/{MAX_NUMBER_OF_ATTEMPTS_RESTARTING}");
#endif

                        obj<Object>($"{GetKey()}[{Field.Address}:{Field.Port}]");
                    }
                    else
                    {
#if CSL
                        _logger($"{LOG} запросил свой перезапуск, но доступные попытки закончились.");
#endif

                        destroy();
                    }
                });
        }

        void Start()
            => obj<Object>($"{GetKey()}[{Field.Address}:{Field.Port}]", Field);

        void Destroying()
        {
#if CSL
            if (_receiveTCPConnection.Count > 0)
            {
                _logger("В конце жизни обьекта осталить неотписавшиеся клинты.");
            }
#endif
        }

#if CSL
        void _logger(string info)
        {
            if (StateInformation.IsCallConfigurate)
                i_componentLogger.To($"ListenTCPClients:Shell:{info}");
        }
#endif

        /// <summary>
        /// Обьект прослушивающий подключающихся клиeнтов.
        /// </summary>
        private sealed class Object : Board.LocalField<Settings.ListenTCPClients>
        {
            /// <summary>
            /// Отправляет сообщение в раздел логгера 
            /// хранящего информацию о компонентах сервера.
            /// </summary>
            private IInput<string> i_componentLogger;

            /// <summary>
            /// Сообщает оболочке о запуске.
            /// </summary>
            private IInput i_start;

            /// <summary>
            /// Сообщает о необходимости перезапустить.
            /// </summary>
            private IInput i_restart;

            /// <summary>
            /// Добавляет клинта в менеджер клиeнтов.
            /// </summary>
            private IInput<TcpClient> i_addTCPConnect;

            private bool _isRunning = false;

            private TcpListener _listener;

            private const int MAX_ONE_TIME_CONNECTION_CLIENT = 25;

            void Construction()
            {
                send_message(ref i_componentLogger, Logger.BUS.Message.SERVER_COMPONENTS);

                send_impuls(ref i_start, BUS.Impuls.START);
                send_impuls(ref i_restart, BUS.Impuls.RESTART);

                send_message(ref i_addTCPConnect, BUS.Message.ADD_TCP_CONNECT);

                add_thread("ListenTCPClients", () =>
                {
                    if (_isRunning == false) return;

                    try
                    {
                        if (_listener.Pending())
                        {
                            do
                            {
                                TcpClient client = _listener.AcceptTcpClient();

#if INFO
                                SystemInformation("New client.");
#endif

                                i_addTCPConnect.To(client);
                            }
                            while (_listener.Pending());
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger(ex.ToString());

                        destroy();
                    }
                },
                Field.ThreadTimeDelay, Thread.Priority.Lowest);
            }

            void Start()
            {
                _listener.Start();

                i_start.To();

                _isRunning = true;

                _logger("Start listen clients.");
            }

            void Configurate()
            {
                _logger("Start configurate.");

                try
                {
                    _listener =
                        new TcpListener(IPAddress.Parse(Field.Address), Field.Port);

                    _logger("End configurate.");
                }
                catch (Exception ex)
                {
                    _logger(ex.ToString());

                    destroy();
                }
            }

            void Stop()
            {
                _logger("Start stopping.");

                if (StateInformation.IsCallConfigurate)
                {
                    try
                    {
                        _listener.Stop();

                        _logger("End stopping.");
                    }
                    catch { /* ... */ }
                }
            }

            void Destruction()
            {
                _logger("Start destroy.");

                _isRunning = false;
            }

            void Destroyed()
            {
                _logger("End destroy.");

                if (StateInformation.IsContruction) i_restart.To();
            }

            void _logger(string info)
            {
                if (StateInformation.IsCallConstruction)
                    i_componentLogger.To($"ListenTCPClients:Object:{info}");
            }
        }
    }
}
