#define SCL
#define INFO

using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace server.component
{
    public sealed class ReceiveUDPShell : ReceiveUDP.Controller
    {
        public const string NAME = "ReceiveUDP:Shell:";

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

            public struct Echo
            {
                /// <summary>
                /// Подписаться на получение UDP пакетов.
                /// </summary>
                public const string SUBSCRIBE_TO_RECEIVE_THE_PACKETS
                    = NAME + "Subscribe to receive the packets";

                /// <summary>
                /// Подписаться на получение первого UDP пакета.
                /// </summary>
                public const string SUBSCRIBE_TO_RECEIVE_THE_FIRST_PACKET
                    = NAME + "Subscribe to receive the first packet.";

                /// <summary>
                /// Отписаться от получение UDP пакетов.
                /// </summary>
                public const string UNSUBSCRIBE_TO_RECEIVE_THE_PACKETS
                    = NAME + "Unsubscribe to receive the packets";

                /// <summary>
                /// Отписаться от получение первого UDP пакета.
                /// </summary>
                public const string UNSUBSCRIBE_TO_RECEIVE_THE_FIRST_PACKET
                    = NAME + "Unsubscribe to receive the first packet.";

            }

            public struct Message
            {
                /// <summary>
                /// Прослушивает входящие пакеты, его тип и данные отправителя.
                /// </summary>
                public const string RECEIVE_PACKETS = NAME + "Listen packets";
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


        private const string LOG = "Обьект прослушивающий входящие UDP пакеты";

        void Start()
            => obj<Object>($"{GetKey()}[{Field.Address}:{Field.Port}]", Field);

        void Construction()
        {
            send_message(ref i_componentLogger, Logger.BUS.Message.SERVER_COMPONENTS);

            listen_message<int[], string[], int[], byte[][], int>
                (BUS.Message.RECEIVE_PACKETS)
                    .output_to(ReceivePackets, Header.Event.WORK_UDP_PACKETS);

/*
            listen_echo_2_0<uint, clientShell.ConnectionController.IReceiveFirstUDPPacket>
                (BUS.Echo.SUBSCRIBE_TO_RECEIVE_THE_PACKETS)
                    .output_to(Subscribe, Header1.Event.WORK_UDP_PACKETS);

            listen_echo_1_0<ulong>
                (BUS.Echo.UNSUBSCRIBE_TO_RECEIVE_THE_PACKETS)
                    .output_to(Unsubscribe, Header1.Event.WORK_UDP_PACKETS);
                    */

            listen_echo_2_0<string, clientManager.component.clientShell.ConnectionController.IReceiveFirstUDPPacket>
                (BUS.Echo.SUBSCRIBE_TO_RECEIVE_THE_FIRST_PACKET)
                    .output_to(SubscribeReceiveFirstUDPPacket, Header.Event.WORK_UDP_PACKETS);

            listen_echo_1_0<string>
                (BUS.Echo.UNSUBSCRIBE_TO_RECEIVE_THE_FIRST_PACKET)
                    .output_to(UnsubscribeReceiveFirstUDPPacket, Header.Event.WORK_UDP_PACKETS);

            listen_impuls(BUS.Impuls.START)
                .output_to((infoObj) =>
                {
#if SCL
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
#if SCL
                        _logger($"{LOG} запросил свой перезапуск " +
                            $"{_currentAttemptsRestarting}/{MAX_NUMBER_OF_ATTEMPTS_RESTARTING}");
#endif

                        obj<Object>($"{GetKey()}[{Field.Address}:{Field.Port}]");
                    }
                    else
                    {
#if SCL
                        _logger($"{LOG} запросил свой перезапуск, но доступные попытки закончились.");
#endif

                        destroy();
                    }
                });


        }

        private sealed class Object : Board.LocalField<Settings.ReceiveUDP>
        {
#if SCL
            /// <summary>
            /// Отправляет сообщение в раздел логгера 
            /// хранящего информацию о компонентах сервера.
            /// </summary>
            private IInput<string> i_componentLogger;
#endif

            /// <summary>
            /// Отправляет пакеты их данные.
            /// </summary>
            private IInput<int[], string[], int[], byte[][], int> i_sendPackets;

            /// <summary>
            /// Сообщает оболочке о запуске.
            /// </summary>
            private IInput i_start;

            /// <summary>
            /// Сообщает о необходимости перезапустить.
            /// </summary>
            private IInput i_restart;

            private UdpClient _server;
            private bool _isRunning = false;

            /// <summary>
            /// Адрес клиента отправившего пакет.
            /// </summary>
            private IPEndPoint _remoteIPEndPoint;

            /// <summary>
            /// Максимально возможно количсво покетов которое можно принять за раз.
            /// </summary>
            private const int MAX_ONE_TIME_PACKETS = 512;

            void Construction()
            {
#if SCL
                send_message(ref i_componentLogger, Logger.BUS.Message.SERVER_COMPONENTS);
#endif
                send_message(ref i_sendPackets, BUS.Message.RECEIVE_PACKETS);

                send_impuls(ref i_start, BUS.Impuls.START);
                send_impuls(ref i_restart, BUS.Impuls.RESTART);

                add_thread("ReceiveUDPPackets", () =>
                {
                    if (_isRunning == false) return;

                    try
                    {
                        int available = _server.Available;
                        if (available > 0)
                        {
#if INFO
                            SystemInformation($"Receive packets[{available}]");
#endif
                            int[] types = new int[MAX_ONE_TIME_PACKETS];
                            string[] addresses = new string[MAX_ONE_TIME_PACKETS];
                            int[] ports = new int[MAX_ONE_TIME_PACKETS];
                            byte[][] packets = new byte[MAX_ONE_TIME_PACKETS][];
                            int index = 0;

                            do
                            {
                                packets[index] = _server.Receive(ref _remoteIPEndPoint);
                                available -= packets[index].Length;

                                // Проверяем пришедшее сообщение на соответвие минимально
                                // допустимому размеру сообщения.
                                if (packets[index].Length >= udp.Header.LENGTH)
                                {
                                    int type = packets[index][udp.Header.DATA_TYPE_INDEX];

                                    types[index] = type;

                                    addresses[index] = _remoteIPEndPoint.Address.ToString();
                                    ports[index] = _remoteIPEndPoint.Port;

                                    index++;
                                }
#if INFO
                                else
                                    SystemInformation($"Прибыл пакет не допустимого размера.[{packets.Length}/{udp.Header.LENGTH}]",
                                        ConsoleColor.Red);
#endif
                            }
                            while (index < MAX_ONE_TIME_PACKETS && available > 0);

                            i_sendPackets.To(types, addresses, ports, packets, index);
                        }
                    }
                    catch (Exception ex)
                    {
#if SCL
                        _logger(ex.ToString());
#endif

                        destroy();
                    }
                },
                Field.ThreadTimeDelay, Thread.Priority.Highest);
            }

            void Start()
            {
#if SCL
                _logger("Start receive UDP packet.");
#endif

                i_start.To();

                _isRunning = true;
            }

            void Configurate()
            {
                try
                {
                    _server = new UdpClient
                        (new IPEndPoint(IPAddress.Parse(Field.Address), Field.Port));
                }
                catch (Exception ex)
                {
#if SCL
                    _logger(ex.ToString());
#endif
                }
            }

            void Stop()
            {
#if SCL
                _logger("Stop receive upd packets.");
#endif

                if (StateInformation.IsCallConfigurate)
                {
                    try
                    {
                        _server.Dispose();
                        _server.Close();
                    }
                    catch { /* ... */ }
                }
            }

            void Destruction()
            {
#if SCL
                _logger("Start destroy.");
#endif

                _isRunning = false;
            }

            void Destroyed()
            {
#if SCL
                _logger("End destroy.");
#endif

                i_restart.To();
            }

#if SCL
            void _logger(string info)
                => i_componentLogger.To($"{GetExplorer()}:{info}");
#endif
        }
    }
}
