#define EXCEPTION

using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace Server
{
    public sealed class ReceiveUDPPacketForClients : Controller.LocalField<string[]>
    {
        private string _receiveUDPPacketName;

        private readonly Dictionary<ulong, Client.Main.IReceiveUDPPacket> _clients
            = new Dictionary<ulong, Client.Main.IReceiveUDPPacket>();

        /// <summary>
        /// Количесво попыток перезапуска обьекта ReceiveUDPPacket.
        /// По истечению всех возможных попыток сервер прекратит свою работу.
        /// </summary>
        private const uint NUMBER_RESTART_OF_ATTEMPTS = 10;
        private uint _currentNumberOfAttermpts = 0;

        void Construction()
        {
            /*
                Обьект для нового клинта создан, теперь подпишим его на получения UDP пакетов.
                После чего оповестим об окончании подписании.
            */
            listen_echo_2_0<ulong, Client.Main.IReceiveUDPPacket>(BUS.LE_SUBSCRIBE_CLIENT_RECEIVE_UDP_PACKET)
                .output_to((id, client, @return) =>
                {
                    if (_clients.ContainsKey(id))
                    {
                        Exception(EX.x00, id);
                    }
                    else
                    {
                        _clients.Add(id, client);

                        @return.To();
                    }
                },
                Header.UDP_WORK_EVENT);

            listen_message<ulong[], int[], byte[][]>(BUS.LM_RECEIVE_UDP_PACKETS)
                .output_to((names, m, packets) =>
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        if (_clients.TryGetValue(names[i], out Client.Main.IReceiveUDPPacket client))
                            client.ReceiveUDPPacket(packets[i]);
                    }
                },
                Header.UDP_WORK_EVENT);

            listen_impuls(BUS.LI_RESTART_RECEIVE_UDP)
                .output_to((infoObj) =>
                {
                    if (infoObj.GetKey() == _receiveUDPPacketName)
                    {
                        if (try_obj(_receiveUDPPacketName))
                        {
                            Exception(EX.x05);
                        }
                        else
                        {
                            if (_currentNumberOfAttermpts < NUMBER_RESTART_OF_ATTEMPTS)
                            {
                                obj<ReceiveUDPPacket>(_receiveUDPPacketName, Field);
                            }
                            else
                            {
                                SystemInformation
                                    ($"Превышено количество попыток перезапуска обьекта {infoObj.GetKey()}",
                                        ConsoleColor.Red);

                                destroy();
                            }
                        }
                    }
                    else Exception(EX.x04, infoObj.GetKey(), _receiveUDPPacketName);
                },
                Header.WORK_WITCH_OBJECTS_EVENT);

            listen_impuls(BUS.LI_RESET_NUMBER_OF_ATTEMPTS)
                .output_to((infoObj) =>
                {
                    if (infoObj.GetKey() == _receiveUDPPacketName)
                    {
                        _currentNumberOfAttermpts = 0;
                    }
                    else Exception(EX.x04, infoObj.GetKey(), _receiveUDPPacketName);
                },
                Header.WORK_WITCH_OBJECTS_EVENT);
        }

        void Configurate()
        {
            if (ReceiveUDPPacket.LOCAL_FIELD_COUNT == Field.Length)
            {
                _receiveUDPPacketName = $"{Field[ReceiveUDPPacket.ADDRESS_INDEX]}" +
                    $"{ReceiveUDPPacket._}{Field[ReceiveUDPPacket.PORT_INDEX]}";
            }
            else Exception(EX.x03, ReceiveUDPPacket.LOCAL_FIELD_COUNT, Field.Length);
        }

        void Start()
        {
            obj<ReceiveUDPPacket>(_receiveUDPPacketName, Field);
        }

        void Stop()
        {
            if (_clients.Count != 0) Exception(EX.x02);
        }

        public struct BUS
        {
            ///<summary>
            /// Принимает UDP пакеты для клиeнтов и имена клентов для которых они предназначены.
            /// listen_message
            /// [in string[] имена клинтов, in byte[][] пакеты для клиетов]
            ///</summary>
            public const string LM_RECEIVE_UDP_PACKETS = "Receive udp packets";

            ///<summary>
            /// Принимает от клиeнта его имя и способ передачи ему UDP пакетов.
            /// После чего отправляет сигнализирует клинту о, том что он подписался.
            /// listen_echo
            /// [in string имя клиета, Client.Main.IReceivePacket способ передачи пакета клиeнту.]
            ///</summary>
            public const string LE_SUBSCRIBE_CLIENT_RECEIVE_UDP_PACKET = "Subscribe client receive udp packet";

            ///<summary>
            /// Отписывает клинта от прослушки UDP пакетов.
            ///</summary>
            public const string LM_UNSUBSCRIBE_CLIENT_RECEIVE_UDP_PACKET = "Unsubscribe client receive udp packet";

            ///<summary>
            /// Перезапускает плослушку UDP пакетов.
            ///</summary>
            public const string LI_RESTART_RECEIVE_UDP = "Restart receive udp";

            /// <summary>
            /// Сбрабывает счетчик возможных попыток запуска ReceiveUPDPacket.
            /// </summary>
            public const string LI_RESET_NUMBER_OF_ATTEMPTS = "Reset number of attempts";
        }

        private struct EX
        {
            public const string x00 = @"Вы пытатесь подписать клиета {0} на получение UDP пакетов, но клиент с таким id уже сущесвует.";
            public const string x01 = @"Вы пытаетесь отписать клинта {0} от прослушки UDP пакетов, но клинта с таким id не подписан.";
            public const string x02 = @"При уничтожении обьекта не все клинты отключились!";
            public const string x03 = @"Неверное количесво локальных данных для ReceiveUDPPacket." +
                @"Ожидалось {0}, но было получено {1}.";
            public const string x04 = @"Пуступил запрос на перезапуск ReceiveUDPPackets."
                        + @"Запрос поступил от обьекта с именем {0}, но текущий обьект отвечает за "
                        + @"ReceiveUDPPacket под именем {1}";
            public const string x05 = "В момент когда ReceiveUDPPacket отправит запрос на перезапуск он должен быть уничтожен.";
            public const string x06 = @"";
            public const string x07 = @"";

        }
    }

    public sealed class ReceiveUDPPacket : Controller.Board.LocalField<string[]>
    {
        public const string _ = "/";

        public const int LOCAL_FIELD_COUNT = 2;
        public const int ADDRESS_INDEX = 0;
        public const int PORT_INDEX = 1;

        private UdpClient _server;
        private IPEndPoint _remoteIpEndPoint = null;

        private bool _isRunning = true;

        private IInput<string[], int[], byte[][]> i_sendPacketsToClients;
        private IInput i_restartCurrentObject;
        private IInput i_resetNumberOfAttempts;

        void Construction()
        {
            //send_message(ref i_sendPacketsToClients, ReceiveUDPPacketForClients.BUS.LM_RECEIVE_UDP_PACKETS);
            send_impuls(ref i_restartCurrentObject, ReceiveUDPPacketForClients.BUS.LI_RESTART_RECEIVE_UDP);
            send_impuls(ref i_resetNumberOfAttempts, ReceiveUDPPacketForClients.BUS.LI_RESET_NUMBER_OF_ATTEMPTS);

            add_thread(GetKey(), () =>
            {
                if (_isRunning == false) return;

                try
                {
                    while (_isRunning)
                    {
                        int available = _server.Available;

                        if (available > 0)
                        {
                            string[] clientAddressBuffers = new string[1024];
                            int[] clientPortBuffers = new int[1024];
                            byte[][] clientBytesBuffers = new byte[1024][];

                            int index = 0;
                            do
                            {
                                clientBytesBuffers[index] = _server.Receive(ref _remoteIpEndPoint);
                                available -= clientBytesBuffers[index].Length;

                                // Проверяем пришедшее сообщение на соответвие минимально
                                // допустимому размеру сообщения.
                                if (clientBytesBuffers[index].Length > UDPHeader.MIN_LENGTH)
                                {
                                    // Проверяем пришедшее сообщение на соответвие максимально
                                    // допустимому размеру сообщения.
                                    if (clientBytesBuffers[index].Length <= UDPHeader.MAX_LENGTH)
                                    {
                                        // Получаем ip адресс клиeнта и его порт.
                                        clientAddressBuffers[index] = _remoteIpEndPoint.Address.ToString();
                                        clientPortBuffers[index] = _remoteIpEndPoint.Port;

                                        if ((++index) == 1024) break;
                                    }
#if EXCEPTION                           
                                    else
                                    {
                                        Exception(Ex.x02, UDPHeader.MAX_LENGTH, clientBytesBuffers[index].Length);
                                    }
#endif
                                }
#if EXCEPTION
                                else
                                {
                                    Exception(Ex.x01, UDPHeader.MIN_LENGTH, clientBytesBuffers.Length);
                                }
#endif
                            }
                            while (available > 0);

                            i_sendPacketsToClients.To
                                (clientAddressBuffers, clientPortBuffers, clientBytesBuffers);
                        }
                    }
                }
                catch
                {
                    destroy();

                    i_restartCurrentObject.To();
                }
            },
            1, Thread.Priority.Highest);
        }

        void Start()
        {
            SystemInformation("Start receive udp packets.", ConsoleColor.Green);
        }

        void Destruction()
        {
            _isRunning = false;
        }

        void Configurate()
        {
            try
            {
                IPEndPoint _localPoint = new IPEndPoint
                    (IPAddress.Parse(Field[ADDRESS_INDEX]), Convert.ToInt32(Field[PORT_INDEX]));

                _server = new UdpClient(_localPoint);

                i_resetNumberOfAttempts.To();
            }
            catch (Exception ex)
            {
                SystemInformation(ex.ToString(), ConsoleColor.Red);

                destroy();

                i_restartCurrentObject.To();
            }
        }


        private struct Ex
        {
            public const string x01 = @"Пришедшее сообщения не удовлетворяет условию, которое допускает " +
                @"минимальный размер равный {0}, его размер {1}";
            public const string x02 = @"Пришедшее сообщения не удовлетворяет условию, которое допускает " +
                @"максимальный размер равный {0}, его размер {1}";
            public const string x03 = @"";
            public const string x04 = @"";
            public const string x05 = @"";
            public const string x06 = @"";
            public const string x07 = @"";
            public const string x08 = @"";
            public const string x09 = @"";
        }

    }
}