#define EXCEPTION
#define INFORMATION

using System.Net;
using System.Net.Sockets;
using Butterfly;

public sealed class ReceiveUDPPacketForClients : Controller.LocalField<string[]>
{
    /// <summary>
    /// Сюда пописываются клиеты которое ожидают получения UDP пакетов.
    /// </summary>
    /// <typeparam name="ulong">Аддресс и порт.</typeparam>
    /// <typeparam name="Client.Main.IReceiveUDPPackets">Описывают способ получения пакетов.</typeparam>
    /// <returns></returns>
    private readonly Dictionary<ulong, Client.IReceiveUDPPackets> _clientsReceiveUDPPackets
        = new Dictionary<ulong, Client.IReceiveUDPPackets>();

    /// <summary>
    /// Сюда подписываются клиенты ожидающие первый UDP пакет.
    /// </summary>
    /// <typeparam name="ulong">id клинта.</typeparam>
    /// <typeparam name="Client.Main.IReceiveFirstUDPPacket">Описывает способ получение пакета.</typeparam>
    /// <returns></returns>
    private readonly Dictionary<uint, Client.IReceiveFirstUDPPacket> _clientsReceiveFirstUDPPackets
        = new Dictionary<uint, Client.IReceiveFirstUDPPacket>();

    /// <summary>
    /// Количесво попыток перезапуска обьекта ReceiveUDPPacket.
    /// По истечению всех возможных попыток сервер прекратит свою работу.
    /// </summary>
    private const uint NUMBER_RESTART_OF_ATTEMPTS = 10;
    private uint _currentNumberOfAttermpts = 0;
    private string _receiveUDPPacketName;

    void Construction()
    {
        /*
            Обьект для нового клинта создан, теперь подпишим его на получения UDP пакетов.
            После чего оповестим об окончании подписании.
        */
        listen_echo_2_0<ulong, Client.IReceiveUDPPackets>(BUS.LE_SUBSCRIBE_CLIENT_RECEIVE_UDP_PACKETS)
            .output_to((key, receiveUDPPackets, @return) =>
            {
                if (_clientsReceiveUDPPackets.ContainsKey(key))
                {
#if INFORMATION
                    Exception(Ex.x00, key);
#endif
                }
                else
                {
#if INFORMATION
                    SystemInformation
                        ($"Клиент {@return.GetKey()} подписался на прослушкy UDP пакетов.",
                                ConsoleColor.Green);
#endif
                    _clientsReceiveUDPPackets.Add(key, receiveUDPPackets);

                    // Оповестим что подписка окончена.
                    @return.To();
                }
            },
            Header.UDP_WORK_EVENT);

        /*
            Отписываем клиента от прослушки входящих UDP пакетов.
        */
        listen_message<uint>(BUS.LM_UNSUBSCRIBE_CLIENT_RECEIVE_UDP_PACKETS)
            .output_to((key) =>
            {
                if (_clientsReceiveUDPPackets.Remove(key))
                {
#if INFORMATION
                    SystemInformation
                        ($"Клиент {key << 32}/" +
                            $"{(int)key} отписался от прослушки UDP пакетов.",
                                ConsoleColor.Green);
#endif
                }
#if EXCEPTION
                else Exception(Ex.x06, key);
#endif
            },
            Header.UDP_WORK_EVENT);

        listen_echo_2_0<uint, Client.IReceiveFirstUDPPacket>
            (BUS.LE_SUBSCRIBE_CLIENT_RECEIVE_FIRST_UDP_PACKET)
                .output_to((idClient, receive, @return) =>
            {
                if (_clientsReceiveFirstUDPPackets.ContainsKey(idClient))
                {
#if EXCEPTION
                    Exception(Ex.x09, @return.GetKey(), idClient);
#endif
                }
                else
                {
                    _clientsReceiveFirstUDPPackets.Add(idClient, receive);

#if INFORMATION
                    SystemInformation
                        ($"Клиент id:{idClient}, key:{@return.GetKey()} подписался на получение " +
                            "первого UDP пакета", ConsoleColor.Green);
#endif
                    // Сигнализируем что мы успешно подписались.
                    @return.To();
                }
            },
            Header.UDP_WORK_EVENT);

        listen_message<byte[], ulong[], byte[][], int>(BUS.LM_RECEIVE_UDP_PACKETS)
            .output_to((type, keysClient, buffers, length) =>
            {
                for (int index = 0; index < length; index++)
                {
                    if (type[index] == ServiceUDPMessage.ClientToServer.Data.TYPE)
                    {
                        if (_clientsReceiveUDPPackets.TryGetValue(keysClient[index],
                            out Client.IReceiveUDPPackets client))
                        {
                            client.Receive(buffers[index]);
                        }
#if EXCEPTION
                        else Exception(Ex.x08, keysClient[index] >> 32, (int)keysClient[index]);
#endif
                    }
                    else if (type[index] == ServiceUDPMessage.ClientToServer.Connecting.TYPE)
                    {
                        if (buffers[index].Length == ServiceUDPMessage.ClientToServer.Connecting.LENGTH)
                        {
                            uint idClient = buffers[index][UDPHeader.ID_CLIENT_4byte] ^
                                (uint)(buffers[index][UDPHeader.ID_CLIENT_3byte] << 8) ^
                                (uint)(buffers[index][UDPHeader.ID_CLIENT_2byte] << 16) ^
                                (uint)(buffers[index][UDPHeader.ID_CLIENT_1byte] << 24);

                            if (_clientsReceiveFirstUDPPackets.Remove(idClient,
                                out Client.IReceiveFirstUDPPacket client))
                            {
                                client.Receive(buffers[index]);
                            }
#if EXCEPTION
                            else Exception(Ex.x07, idClient);
#endif
                        }
#if EXCEPTION
                        else Exception(Ex.x11, ServiceUDPMessage.ClientToServer.Connecting.LENGTH,
                            buffers[index].Length);
#endif
                    }
#if EXCEPTION
                    else Exception("Неизвестный тип UDP пакета.");
#endif
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
                        Exception(Ex.x05);
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
                else Exception(Ex.x04, infoObj.GetKey(), _receiveUDPPacketName);
            },
            Header.WORK_WITCH_OBJECTS_EVENT);

        listen_impuls(BUS.LI_RESET_NUMBER_OF_ATTEMPTS)
            .output_to((infoObj) =>
            {
                if (infoObj.GetKey() == _receiveUDPPacketName)
                {
                    _currentNumberOfAttermpts = 0;
                }
                else Exception(Ex.x04, infoObj.GetKey(), _receiveUDPPacketName);
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
        else Exception(Ex.x03, ReceiveUDPPacket.LOCAL_FIELD_COUNT, Field.Length);
    }

    void Start()
    {
        obj<ReceiveUDPPacket>(_receiveUDPPacketName, Field);
    }

    void Stop()
    {
        if (_clientsReceiveUDPPackets.Count != 0) Exception(Ex.x02);
    }

    public struct BUS
    {
        ///<summary>
        /// Принимает UDP пакеты для клиeнтов и имена клентов для которых они предназначены.
        /// listen_message
        /// [in byte[] тип сообщений, in ulong[] ключи клинтов содержащие аддрес и порт клинта,
        /// in byte[][] сообщения, in int длина массива массивов]
        ///</summary>
        public const string LM_RECEIVE_UDP_PACKETS = "Receive udp packets";

        /// <summary>
        /// Подписывает клиетa на получение первого UDP сообщения.
        /// listen_echo
        /// [in ulong - id клиента, Client.Main.IReceiveFirstUDPPacket способ передач пакета клиенту.
        /// out пустое эхо сообщающее что клинт подписан]
        /// </summary>
        public const string LE_SUBSCRIBE_CLIENT_RECEIVE_FIRST_UDP_PACKET
            = "Subscribe/Unsubscribe client receive first udp packet";

        ///<summary>
        /// Принимает от клиeнта его имя и способ передачи ему UDP пакетов.
        /// После чего отправляет сигнализирует клинту о, том что он подписался.
        /// listen_echo
        /// [in ulong аддресс и порт клинта, Client.Main.IReceiveUDPPackets способ передачи пакета клиeнту.]
        /// [out пустой это для оповещения]
        ///</summary>
        public const string LE_SUBSCRIBE_CLIENT_RECEIVE_UDP_PACKETS
            = "Subscribe client receive udp packet";

        ///<summary>
        /// Отписывает клинта от прослушки UDP пакетов.
        /// listen_message
        /// [in ulong - аддрес и порт с клиента]
        ///</summary>
        public const string LM_UNSUBSCRIBE_CLIENT_RECEIVE_UDP_PACKETS
            = "Unsubscribe client receive udp packet";

        ///<summary>
        /// Перезапускает плослушку UDP пакетов.
        ///</summary>
        public const string LI_RESTART_RECEIVE_UDP = "Restart receive udp";

        /// <summary>
        /// Сбрабывает счетчик возможных попыток запуска ReceiveUPDPacket.
        /// </summary>
        public const string LI_RESET_NUMBER_OF_ATTEMPTS = "Reset number of attempts";
    }

    private struct Ex
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
        public const string x06 = @"Вы пытаетесь отписать клинта от прослушки UDP пакетов, но клиента с таким адрресом и портом {0} не существует.";
        public const string x07 = @"Вы пытатесь передать первый UDP пакет клиенту с id:{0}, но данный клинт не прослушивает такое сообщение.";
        public const string x08 = @"Вы получили UDP пакет который предназначен для клиента подписаного на сообщения с аддресса {0} и порта {1}.";
        public const string x09 = @"Клиент {0} с таким id:{1} уже подписался и ожидает первый UDP пакет.";
        public const string x10 = @"Вы пытаетесь отписать клинта {0} с id:{1}, " +
            @"но данный клинт не подписан и не ожидает первый UDP пакет.";
        public const string x11 = @"Длина первого UDP пакета должна быть равна {0}, " +
                            @" вместо этого пришол покет длиной равной {1}";
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

    private IInput<byte[], ulong[], byte[][], int> i_sendPacketsToClients;
    private IInput i_restartCurrentObject;
    private IInput i_resetNumberOfAttempts;

    void Construction()
    {
        send_message(ref i_sendPacketsToClients, ReceiveUDPPacketForClients.BUS.LM_RECEIVE_UDP_PACKETS);
        send_impuls(ref i_restartCurrentObject, ReceiveUDPPacketForClients.BUS.LI_RESTART_RECEIVE_UDP);
        send_impuls(ref i_resetNumberOfAttempts, ReceiveUDPPacketForClients.BUS.LI_RESET_NUMBER_OF_ATTEMPTS);

        add_thread(GetKey(), () =>
        {
            if (_isRunning == false) return;

            try
            {
                //while (_isRunning)
                {
                    int available = _server.Available;
                    if (available > 0)
                    {
                        Console("AVAILABLE:" + available);

                        byte[] clientMessageTypeBuffers = new byte[2048];
                        ulong[] clientMessageAddressAndPortBuffers = new ulong[2048];
                        byte[][] clientBytesBuffers = new byte[2048][];

                        int index = 0;
                        do
                        {
                            clientBytesBuffers[index] = _server.Receive(ref _remoteIpEndPoint);
                            available -= clientBytesBuffers[index].Length;

                            // Проверяем пришедшее сообщение на соответвие минимально
                            // допустимому размеру сообщения.
                            if (clientBytesBuffers[index].Length >= UDPHeader.MIN_LENGTH)
                            {
                                // Проверяем пришедшее сообщение на соответвие максимально
                                // допустимому размеру сообщения.
                                if (clientBytesBuffers[index].Length <= UDPHeader.MAX_LENGTH)
                                {
                                    // Узнаем тип сообщения. Получаем левые два бита.
                                    byte messageType = (byte)(clientBytesBuffers[index][UDPHeader.TYPE_INDEX] >> 7);

                                    clientMessageTypeBuffers[index] = messageType;

#if INFORMATION
                                    SystemInformation("PACKET", ConsoleColor.Yellow);
#endif

                                    if (messageType == ServiceUDPMessage.ClientToServer.Data.TYPE)
                                    {
#if INFORMATION
                                        SystemInformation("Пришел пакет с полезными данными.", ConsoleColor.Green);
#endif
                                        clientMessageAddressAndPortBuffers[index] =
                                            (ulong)(_remoteIpEndPoint.Address.Address << 32) ^
                                            (ulong)_remoteIpEndPoint.Port;
                                    }
#if INFORMATION
                                    /*
                                     Пришол первый пакет. Он нужен для того что бы узнать какой аддресс
                                     и порт выделил NAT. Мы просто передаем содержимое массива клиeнту
                                     который уже ожидает его.
                                    */
                                    else if (messageType == ServiceUDPMessage.ClientToServer.Connecting.TYPE)
                                    {
                                        SystemInformation("Пришел первый UDP пакет.", ConsoleColor.Green);
                                    }
#endif

#if EXCEPTION
                                    else Exception(Ex.x03, messageType);
#endif

                                    if ((++index) == 2048) break;
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
                                Exception(Ex.x01, UDPHeader.MIN_LENGTH, clientBytesBuffers[index].Length);
                            }
#endif
                        }
                        while (available > 0);

                        i_sendPacketsToClients.To
                            (clientMessageTypeBuffers, clientMessageAddressAndPortBuffers,
                                clientBytesBuffers, index);
                    }
                }
            }
            catch
            {
                destroy();

                i_restartCurrentObject.To();
            }
        },
        5, Thread.Priority.Highest);
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
        public const string x03 = @"Пришоло сообщение неизвестного типа {0}.";
        public const string x04 = @"";
        public const string x05 = @"";
        public const string x06 = @"";
        public const string x07 = @"";
        public const string x08 = @"";
        public const string x09 = @"";
    }

}