#define EX
#define INFO

using System.Net.Sockets;
using Butterfly;

namespace server.component.clientManager.component
{
    public sealed class ConnectData : PacketHandler
    {
        public readonly int ClientID;
        public readonly int UnitID;

        public readonly TcpClient SSL; 
        public readonly TcpClient TCP;


        public ConnectData(Action<string> destroy, int clientID, int unitID,
            TcpClient ssl, TcpClient tcp, string udpAddress, int udpPort)
                : base(destroy, udpAddress, udpPort, unitID)
        {
            ClientID = clientID;
            UnitID = unitID;
            SSL = ssl;
            TCP = tcp;
        }

    }

    public abstract class PacketHandler : UDP
    {
        private readonly byte _unitID_1b, _unitID_2b, _unitID_3b, _unitID_4b;

        public const int YEAR_INDEX_1b = 0, YEAR_INDEX_2b = 1, MOUNTH_INDEX = 2,
        DAY_INDEX = 3, HOUR_INDEX = 4, MIN_INDEX = 5, SEC_INDEX = 6,
        MILL_INDEX_1b = 7, MILL_INDEX_2b = 8;

        public PacketHandler(Action<string> destroy, string address, int port,
            int unitID)
            : base(destroy, address, port)
        {
            _unitID_1b = (byte)(unitID >> 24); _unitID_2b = (byte)(unitID >> 16);
            _unitID_3b = (byte)(unitID >> 8); _unitID_4b = (byte)unitID;
        }

        public byte[] GetPositionX(int position, byte[] dateTime)
        {
            byte[] result = new byte[Capsule.Header.PSH.NextXPosition.LENGTH]
            {
                Capsule.Header.PSH.NextXPosition.LENGTH,
                Capsule.Header.PSH.TYPE,

                _unitID_1b, _unitID_2b, _unitID_3b, _unitID_4b,

                // Уникальный ID капсулы.
                0, 0,

                dateTime[YEAR_INDEX_1b], dateTime[YEAR_INDEX_2b],
                dateTime[MOUNTH_INDEX], dateTime[DAY_INDEX],
                dateTime[HOUR_INDEX], dateTime[MIN_INDEX],
                dateTime[SEC_INDEX],
                dateTime[MILL_INDEX_1b], dateTime[MILL_INDEX_2b],

                Capsule.Header.PSH.NextXPosition.TYPE,

                (byte)(position >> 24), (byte)(position >> 16),
                (byte)(position >> 8), (byte)position
            };

            return result;
        }
    }

    public abstract class UDP : DDD, UDP.IReceivePackets
    {
        public IInput<byte[]> I_sendUDP;

        private int _capsuleID = 1;

        /// <summary>
        /// Тип UDP сообщение которое хранит капсулы.
        /// </summary>
        /// <returns></returns>
        private byte _packetCapsuleType = (byte)udp.Data.Type.Capsules;

        public UDP(Action<string> destroy, string address, int port)
            : base(destroy, address, port)
        { }

        // Максимальное количесво пакетов.
        private const int MAX_COUNT_PACKETS = 64;
        // Содержит пакеты.
        private byte[][] _packets = new byte[MAX_COUNT_PACKETS][];
        // Содержит длину каждого пакета.
        private int[] _packetsLength = new int[MAX_COUNT_PACKETS];
        // Количесво пакетов.
        private int _packetsCount = 0;

        // 1)ID - уникальный id выданый капсуле
        // 2)Индекс пакета в котором записана капсула.
        // 3)Стартовая позиция в массиве байт.
        // 4)Длина капсулы.
        private const int CAPSULE_ID = 0, CAPSULE_PACKET_INDEX = 1,
            CAPSULE_START_INDEX = 2, CAPSULE_LENGTH = 3;

        ///  <summary>
        /// 1)ID - уникальный id выданый капсуле
        /// 2)Индекс пакета в котором записана капсула.
        /// 3)Стартовая позиция в массиве байт.
        /// 4)Длина капсулы.
        /// </summary>
        private int[][] _capsulesInformation = new int[MAX_ACK_LENGTH][];
        private int _capsulesInformationCount = 0;

        /// <summary>
        /// Максимально возможное количесво ожиданий подтверждений.
        /// </summary>
        private const int MAX_ACK_LENGTH = 1024;
        /// <summary>
        /// Записываем номера ID капсул от которых ожидается 
        /// подтверждение доставки.
        /// </summary>
        private int[] _acknoledgments = new int[1024];
        /// <summary>
        /// Количесво ожидаемых подтвержедений о доставки карсул.
        /// </summary>
        private int _acknoledgmentsLength = 0;

        public void Receive(byte[] packet)
        {
            if (packet.Length < udp.Header.LENGTH)
            {
                // Получаем длину всего пакета.
                int packetLength = packet[udp.Header.DATA_LENGTH_INDEX_1byte] << 8 ^
                    packet[udp.Header.DATA_LENGTH_INDEX_2byte];

                if (packet.Length != packetLength)
                {
#if EX
                    throw new Exception("Длина пакета не соответсвует длине указаной " +
                        "в заголовке.");
#endif
                    return;
                }

                // Получаем тип пакета.
                int packetType = packet[udp.Header.DATA_TYPE_INDEX];

                if (packetType == _packetCapsuleType)
                {
                    // Тип капсулы указывается стразуже после заголовка пакета.
                    int index = udp.Header.LENGTH;
                    while ((++index) < packetLength)
                    {
                        int capsuleType = packet[index];
                        if (capsuleType == Capsule.Header.ACK.TYPE || capsuleType == Capsule.Header.FIN.TYPE)
                        {
                            if (index + Capsule.Header.ACK.LENGTH < packetLength)
                            {
                                // Получаем номер подтверждения.
                                int acknoledgmentNumber
                                    = packet[index + Capsule.Header.ACK.ACKNOLEDGMENT_ID_1byte] << 8 ^
                                        packet[index + Capsule.Header.ACK.ACKNOLEDGMENT_ID_2byte];

#if INFO
                                SystemInformation($"ACK:{acknoledgmentNumber}.");
#endif

                                // Ищем капсулу.
                                for (int i = 0; i < _capsulesInformationCount; i++)
                                {
                                    if (_capsulesInformation[i][CAPSULE_ID] == acknoledgmentNumber)
                                    {
                                        // Извлекли данные отправленой капсулы ожидающей подтверждения.
                                        int packetIndex = _capsulesInformation[i][CAPSULE_PACKET_INDEX];
                                        int startIndex = _capsulesInformation[i][CAPSULE_START_INDEX];
                                        int capsuleLength = _capsulesInformation[i][CAPSULE_LENGTH];

                                        // Декриментируем количесво капсул.
                                        // И получаем индекс крайней капсулы.
                                        int lastCapsuleIndex = --_capsulesInformationCount;
                                        // Удалилям данныe о капсуле.
                                        if (lastCapsuleIndex > 0 && lastCapsuleIndex > i && lastCapsuleIndex != i)
                                        {
                                            // Крайний записываем в удаленый.
                                            _capsulesInformation[i] = _capsulesInformation[lastCapsuleIndex];
                                        }

#if INFO
                                        SystemInformation($"Капсула для которой прибыло подтверждение:" +
                                            $"Index:{i}" +
                                            $"IndexPacket:{packetIndex}" +
                                            $"Start:{startIndex}" +
                                            $"Length:{capsuleLength}");
#endif
                                        // Вычесляем следующий индекс после текущуй касулы.
                                        int endCapsuleIndex = startIndex + capsuleLength;
                                        // Узнаем следует ли за капсулой следующая капсула.
                                        if (endCapsuleIndex < _packetsLength[i])
                                        {
#if INFO
                                            SystemInformation($"За капсулой записана еще капсула/ы." +
                                                $"Размер пакета:{_packetsLength[i]} " +
                                                $" последний байт капуслы распалагается в позиции" +
                                                $" {startIndex + capsuleLength}.");
#endif
                                            // Сдвигаем все капсулы в лево.
                                            for (int p = endCapsuleIndex; p < _packetsLength[i]; p++)
                                            {
                                                _packets[i][startIndex++] = _packets[i][p];
                                            }
                                        }

                                        // Перезаписываем длину пакета.
                                        _packetsLength[i] = startIndex;
#if INFO
                                        SystemInformation($"Новая длина пакета {_packetsLength[i]}.");
#endif
                                        // Если пакет содержит только заголовок, то удалим пакет.
                                        if (_packetsLength[i] == udp.Header.LENGTH)
                                        {
#if INFO
                                            SystemInformation("Пакет больше не содержит капсул.");
#endif
                                            _packets[i] = null;

                                            // Индекс крайнего пакета, который переедит на новое место.
                                            // Дикрементируем количесво пакетов.
                                            int lastIndexPacket = --_packetsCount;

                                            // Если пакетов более чем 1 и текущий пакет не является крайним.
                                            // то поставим в данное место крайний пакет.
                                            if (lastIndexPacket > 1)
                                            {
#if INFO
                                                SystemInformation("Пакетов более чем один.");
#endif
                                                // Переносим крайний пакет в освободившийся слот.
                                                _packets[i] = _packets[lastIndexPacket];

                                                // Перезаписываем данные о место положении капсул 
                                                // которые хранятся в только что переехавшем пакете.
                                                for (int m = 0; m < _capsulesInformationCount; m++)
                                                {
                                                    if (lastIndexPacket
                                                        == _capsulesInformation[m][CAPSULE_PACKET_INDEX])
                                                    {
                                                        _capsulesInformation[m][CAPSULE_PACKET_INDEX] = i;
                                                    }
                                                }
                                            }
                                        }

                                        if (capsuleType == Capsule.Header.ACK.TYPE)
                                        {
                                            AddCapsule(new byte[]
                                            {
                                                Capsule.Header.FIN.TYPE,
                                            });
                                        }
                                    }
                                }
                            }
#if EX
                            else
                            {
                                throw new Exception("Прибыла ACK карпсула, но размер " +
                                    "пакета размер пакета не соответсвуте ожидаемой длине." +
                                    $"PacketLength:{packet.Length}\n" +
                                    $"IndexInPacket:{index}\n" +
                                    $"ACK Length:{Capsule.Header.ACK.LENGTH}\n");
                            }
#endif
                        }
                        else if (capsuleType == Capsule.Header.PSH.NextXPosition.TYPE)
                        {
                        }
#if EX
                        else
                        {
                            throw new Exception("Неизветсный тип капсулы.");
                        }
#endif
                    }
                }
#if EX
                else
                {
                    throw new Exception("Неизвестный тип пакета. " +
                        "Ожидалось что тип пакета капсула.");
                }
#endif
            }
#if EX
            else
            {
                throw new Exception($"Размер пришедшего пакета меньше размера заголовка." +
                    $"Размер пакета {packet.Length}, размер заголовка {udp.Header.LENGTH}.");
            }
#endif
        }

        private void AddCapsule(byte[] capsule)
        {
            // Сюда запишим интекс пакета в который будет
            // записана текущая капсула. По умолчанию предпологаем
            // что капсула не куда не влезла.
            int packetIndex = _packetsCount;
            // Проверяем имеется ли место в уже созданых
            // пакетах для новой капсулы.
            for (int u = 0; u < _packetsCount; u++)
            {
                if ((_packetsLength[u] + capsule.Length)
                    <= udp.Header.MAX_LENGTH)
                {
                    packetIndex = u;
                    break;
                }
            }

            // Капсула некуда не влезла, создаем 
            // новый пакет.
            if (packetIndex == _packetsCount)
            {
                byte[] packet = new byte[udp.Header.MAX_LENGTH];

                // Формируем заголовок пакета.
                // Длина заголовка.
                packet[udp.Header.DATA_LENGTH_INDEX_1byte]
                    = udp.Header.LENGTH >> 8;
                packet[udp.Header.DATA_LENGTH_INDEX_2byte]
                    = udp.Header.LENGTH;

                // Тип UDP пакета. Данный пакет содержит капсулы.
                packet[udp.Header.DATA_TYPE_INDEX] = _packetCapsuleType;

                // Добавим пакет в массив.
                _packets[_packetsCount] = packet;
                // Укажим текущюю длину данного пакета.
                _packetsLength[_packetsCount++] = udp.Header.LENGTH;
            }

            // Присваиваем капсуле ID.
            capsule[Capsule.Header.ID_INDEX_1b]
                = (byte)(_capsuleID >> 8);
            capsule[Capsule.Header.ID_INDEX_2b]
                = (byte)_capsuleID;

            if (_acknoledgmentsLength + 1 == MAX_ACK_LENGTH)
            {
                Destroy.Invoke("Превышено максимально возможноe количесво " +
                    "ожидаемых подтверждений доставки капусл.");

                return;
            }

            // Добавляем ID капсулы в массив ACK.
            _acknoledgments[_acknoledgmentsLength++]
                = _capsuleID;

            // Записываем в пакет нашу капсулу.
            // 1) Получаем крайний байт с которого начнется запись.
            int startIndexInPacket = _packetsLength[packetIndex];

            // 2) Записываем расположение капсулы.
            _capsulesInformation[_capsulesInformationCount++] = new int[]
            {
                _capsuleID,
                packetIndex,
                startIndexInPacket,
                capsule.Length
            };

            for (int p = 0; p < capsule.Length; p++)
            {
                _packets[packetIndex][startIndexInPacket++] = capsule[p];
            }

            // Запишим дляну пакета.
            _packetsLength[packetIndex] = startIndexInPacket;

            // Перезапишим длину пакета.
            _packets[packetIndex][udp.Header.DATA_LENGTH_INDEX_1byte]
                = (byte)(startIndexInPacket >> 8);
            _packets[packetIndex][udp.Header.DATA_LENGTH_INDEX_2byte]
                = (byte)startIndexInPacket;

            if ((++_capsuleID) == ushort.MaxValue)
                _capsuleID = 1;

        }

        public void AddCapsules(byte[][] capsules)
        {
            if (_packetsCount == 0)
                _packets[_packetsCount++] = new byte[udp.Header.MAX_LENGTH];

            for (int i = 0; i < capsules.Length; i++)
            {
                AddCapsule(capsules[i]);
            }
        }


        public void Process()
        {
        }

        void IReceivePackets.Receive(byte[] message)
        {
        }

        private void SystemInformation(string info)
        {
            Console.WriteLine("UDP://" + info);
        }

        public interface IReceivePackets
        {
            /// <summary>
            /// Данный метод реализует прослушку UDP пакетов от клиeнта.
            /// </summary>
            /// <param name="message"></param>
            void Receive(byte[] message);
        }
    }

    public abstract class DDD : Socket
    {
        protected readonly Action<string> Destroy;
        private readonly string _address;
        private readonly int _port;

        protected bool _isRunning { private set; get; } = false;

        // ID клиента в ROOM.
        public int Index = -1;
        public Action<int> DownMove;

        public DDD(Action<string> destroy, string address, int port)
            : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            Destroy = destroy;

            _address = address;
            _port = port;
        }

        public void Connect()
        {
            try
            {
                Connect(_address, _port);

                _isRunning = true;
            }
            catch (Exception ex)
            {
                Destroy.Invoke(ex.ToString());
            }
        }

        protected void SendPacket(byte[] packet)
        {
            if (_isRunning)
            {
                try
                {
                    base.Send(packet);
                }
                catch (Exception ex)
                {
                    Destroy.Invoke(ex.ToString());
                }
            }
        }

        protected void Stop()
        {
            _isRunning = false;
        }
    }
}
