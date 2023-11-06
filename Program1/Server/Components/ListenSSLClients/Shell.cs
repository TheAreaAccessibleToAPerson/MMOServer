using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace server.component
{
    /// <summary>
    /// Оболочка обьекта прослушивающего подключающихся клинтов.
    /// </summary>
    public sealed class ListenSSLClientsShell : Controller.LocalField<Settings.ListenSSLClients>
    {
        public const string NAME = "ListenSSLClients:Shell:";

        private struct BUS
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
        }

        /// <summary>
        /// Максимальное количесво попыток перезапуска обьекта.
        /// </summary>
        private const int MAX_NUMBER_OF_ATTEMPTS_RESTARTING = 25;

        /// <summary>
        /// Номер текущей попытки перезапуска.
        /// </summary>
        private int _currentAttemptsRestarting = 0;

        /// <summary>
        /// Отправляет сообщение в раздел логгера 
        /// хранящего информацию о компонентах сервера.
        /// </summary>
        private IInput<string> i_componentLogger;

        private const string LOG = "Обьект прослушивающий подключающихся клиeнтов отчитался";

        void Construction()
        {
            send_message(ref i_componentLogger, Logger.BUS.Message.SERVER_COMPONENTS);

            listen_impuls(BUS.Impuls.START)
                .output_to((infoObj) =>
                {
                    _logger($"{LOG} о начале работы в свою оболочку.");

                    _currentAttemptsRestarting = 0;
                });

            listen_impuls(BUS.Impuls.RESTART)
                .output_to((infoObj) =>
                {
                    if (StateInformation.IsDestroy) return;

                    if (_currentAttemptsRestarting++ < MAX_NUMBER_OF_ATTEMPTS_RESTARTING)
                    {
                        _logger($"{LOG} запросил свой перезапуск " +
                            $"{_currentAttemptsRestarting}/{MAX_NUMBER_OF_ATTEMPTS_RESTARTING}");

                        obj<Object>($"{GetKey()}[{Field.Address}:{Field.Port}]");
                    }
                    else
                    {
                        _logger($"{LOG} запросил свой перезапуск, но доступные попытки закончились.");

                        destroy();
                    }
                });
        }

        void Start()
            => obj<Object>($"{GetKey()}[{Field.Address}:{Field.Port}]", Field);

        void _logger(string info)
        {
            if (StateInformation.IsCallConfigurate)
                i_componentLogger.To($"ListenSSLClients:Shell:{info}");
        }

        /// <summary>
        /// Обьект прослушивающий подключающихся клиeнтов.
        /// </summary>
        private sealed class Object : Board.LocalField<Settings.ListenSSLClients>
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
            private IInput<TcpClient> i_addClient;

            private bool _isRunning = false;

            private TcpListener _listener;

            private const int MAX_ONE_TIME_CONNECTION_CLIENT = 25;

            void Construction()
            {
                send_message(ref i_componentLogger, Logger.BUS.Message.SERVER_COMPONENTS);

                send_impuls(ref i_start, BUS.Impuls.START);
                send_impuls(ref i_restart, BUS.Impuls.RESTART);

                send_message(ref i_addClient, ClientsManager.BUS.Message.ADD_CLIENT);

                add_thread("ListenClients", () =>
                {
                    if (_isRunning == false) return;

                    try
                    {
                        if (_listener.Pending())
                        {
                            do
                            {
                                TcpClient client = _listener.AcceptTcpClient();

                                i_addClient.To(client);
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
                    i_componentLogger.To($"ListenSSLClients:Object:{info}");
            }
        }
    }
}
