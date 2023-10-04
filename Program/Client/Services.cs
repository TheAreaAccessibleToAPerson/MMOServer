#define INFORMATION
#define EXCEPTION

using System.Net;
using System.Net.Sockets;

using Butterfly;

public abstract class ClientService : ClientProperty,
    Client.IReceiveUDPPackets, Client.IReceiveFirstUDPPacket
{
    protected bool IsRunning = true;

    /// <summary>
    /// 
    /// </summary>
    protected uint ID = 0;

    /// <summary>
    /// Первый UDP пакет от клинта должен быть помечен данным ID. Под данному ID мы зарегистрируемся
    /// ReceiveUDPMessage и будет ожидать этот пакет.
    /// </summary>
    protected uint RegisterFirstUDPPacketID = 0;

    /// <summary>
    /// В первом UDP пакете в качесве полезных данных должно придти данное сообщение в 
    /// зашифровоном виде.
    /// </summary>
    protected byte[] FirstUDPPacketData = new byte[ServiceUDPMessage.VERIFICATION_MESSAGE];

    private StateType CurrentState = StateType.None;

    protected Socket TCPSocket;

    /// <summary>
    /// Аддрес клиента.
    /// </summary>
    protected IPAddress RemoteIPAddress;

    /// <summary>
    /// UDP порт с которого будут приходить сообщения от клинта.
    /// </summary>
    protected int RemoteUDPPort;

    protected ulong RemoteUDPAddressAndPort;

    /// <summary>
    /// По данному ключу клиeнт будет подписан в прослушку UDP пакетов.
    /// </summary>
    private ulong SubscribeKeyToReceiveUDPPackets = 0;

    /// <summary>
    /// Подписывает клинта на прослушку входящих UDP пакетов.
    /// </summary>
    protected IInput<ulong, Client.IReceiveUDPPackets> I_subscribeToReceiveUDPPackets;

    /// <summary>
    /// Отписывает клиента из прослушки входящих UDP пакетов.
    /// </summary>
    protected IInput<ulong> I_unsubscribeFromReceiveUDPPacket;

    /// <summary>
    /// Подписывает/Отписывает клиента от ожидания первого UDP пакета.
    /// </summary>
    protected IInput<uint,  Client.IReceiveFirstUDPPacket> I_subscribeOrUnsubscribeToReceiveFirstUDPPacket;

    /// <summary>
    /// Отправляет сообщение по TCP сокету.
    /// </summary>
    protected IInput<byte[]> I_sendSSL;

    /// <summary>
    /// Обрабатывает входящие TCP сообщения.
    /// </summary>
    protected IInput<byte[]> I_TCPMessageProcessing;

    /// <summary>
    /// Обрабатывает входящие UDP сообщения.
    /// </summary>
    protected IInput<byte[]> I_UDPMessageProcessing;

    /// <summary>
    /// Запрос у клиента по TCP.
    /// </summary>
    protected IInput<RequestTCPType> I_requestSSL;

    /// <summary>
    /// Проверяем не пришло ли, нам сообщение.
    /// </summary>
    protected void ReceiveSSL()
    {
        if (IsRunning)
        {
            try
            {
                int available = TCPSocket.Available;
                if (available > 0)
                {
                    byte[] buffer = new byte[available];
                    TCPSocket.Receive(buffer, 0, available, SocketFlags.None, out SocketError error);

                    if (error == SocketError.Success)
                    {
#if INFORMATION
                        Console(Message.Show("ReceiveTCPSocket", buffer, 40));
#endif

                        I_TCPMessageProcessing.To(buffer);
                    }
                    else SystemInformation($"SocketError:{error}", ConsoleColor.Red);
                }
            }
            catch { destroy(); }
        }
    }

    public void ReceiveRoomMessage(string roomName, byte[] message)
    {
    }

    void Client.IReceiveUDPPackets.Receive(byte[] packet)
    {
#if INFORMATION
        SystemInformation("ReceiveUDPPacket");
#endif
    }

    void Client.IReceiveFirstUDPPacket.Receive(byte[] packet)
    {
#if INFORMATION
        SystemInformation("ReceiveFirstUDPPacket");
#endif
    }


    /// <summary>
    /// В данном методе будет произведена настройка соединения с клиентом.
    /// </summary>
    protected void SettingConnection()
    {
#if INFORMATION
        SystemInformation($"SettingConnection:{CurrentState}", ConsoleColor.Yellow);
#endif

        if (IsRunning == false) return;

        // Подписываемся на получение первого UDP пакета.
        if (CurrentState.HasFlag(StateType.None))
        {
            CurrentState = StateType.SubscribeToReceiveFirstUDPPacket;

#if INFORMATION
            SystemInformation("run subscribe to receiveFirst udp packet.", ConsoleColor.Yellow);
#endif

            I_subscribeOrUnsubscribeToReceiveFirstUDPPacket.To(RegisterFirstUDPPacketID, 
                this);
        }
        else if (CurrentState.HasFlag(StateType.SubscribeToReceiveFirstUDPPacket))
        {
#if INFORMATION
            SystemInformation("end subscribe to receiveFirst udp packet.", ConsoleColor.Yellow);
#endif

            CurrentState = StateType.RequestFirstUDPPacket;

#if INFORMATION
            SystemInformation("run request client first udp packet.", ConsoleColor.Yellow);
#endif
            byte[] m = new byte
            [
                TCPHeader.LENGTH_BYTE_COUNT +
                ServiceTCPMessage.ServerToClient.Connecting.LENGTH
            ];

            int messageLength = ServiceTCPMessage.ServerToClient.Connecting.LENGTH;
            int messageType = ServiceTCPMessage.ServerToClient.Connecting.TYPE;

            byte[] t = new byte[messageLength];

            t[0] = (byte)(messageLength << 8); t[1] = (byte)messageLength; 
            t[2] = (byte)(messageType << 8); t[3] = (byte)messageType; 

            t[4] = (byte)(RegisterFirstUDPPacketID << 24); t[5] = (byte)(RegisterFirstUDPPacketID << 16);
            t[6] = (byte)(RegisterFirstUDPPacketID << 8); t[7] = (byte)RegisterFirstUDPPacketID;

            int index = 0;
            for (int i = 8; i < messageLength; i++)
                t[i] = FirstUDPPacketData[index++];

            I_sendSSL.To(t);
        }
        else if (CurrentState.HasFlag(StateType.RequestFirstUDPPacket))
        {
#if INFORMATION
            SystemInformation("end request client first udp packet.", ConsoleColor.Yellow);
#endif

            CurrentState = StateType.SubscribeToReceiveUDPPackets;

#if INFORMATION
            SystemInformation("run subscribe receive udp packets.", ConsoleColor.Yellow);
#endif

            I_subscribeToReceiveUDPPackets.To(RemoteUDPAddressAndPort, this);
        }
        else if (CurrentState.HasFlag(StateType.SubscribeToReceiveUDPPackets))
        {
#if INFORMATION
            SystemInformation("end subscribe receive udp packets.", ConsoleColor.Yellow);
#endif

            // Сообщаем клинту что он может отправлять UDP пакеты.

        }
        else throw new Exception();
    }

    protected struct Ex
    {
        public const string x001 = @"Минимально возможный размер сообщения который приходит " +
            @"по протоколу TCP {0}, но прeшедшее сообщение равно {1} и имеет содержание: {2}.";
        public const string x002 = @"В поступающем TCP сообщении имеется зоголовок в который "
            + @"записывается длина сообщения, на случай если призайдет слипание."
            + @"Длина пришедшего сообщения не сходится с сумой длин указаных в заголовкe/ах."
            + @"Размер массива равен {0}, индекс указывающий на начало сообщения в массиве "
            + @"указывает на позицию {1}, разность от идекса и до конца массива равна {2}.";
        public const string x003 = @"В поступающем TCP сообщении содержется заголовок отвечающий " +
            @"за длину, которая не может быть равна нулю.Индекс начала:{0} \n Сообщение:\n{1}\n";
        public const string x004 = @"Вы можете запросить порт только когда состояние Services 
                {0}, но в данный момент флаг выставлен в {1}.";
        public const string x005 = @"Подписаться на получение UDP можно лишь однажды в самом " +
            @" начале формирования клиента когда состояние выставлено в {0}, но в данный момент оно {1}.";
        public const string x006 = @"Когда придет ответ о том что клиент подписался на получение UDP " +
            @"пакетов его состояние должно быть {0}, но оно {1}.";
        public const string x007 = @"";
        public const string x008 = @"";
        public const string x009 = @"";
        public const string x010 = @"";
        public const string x011 = @"";
        public const string x012 = @"";
        public const string x013 = @"";
        public const string x014 = @"";
        public const string x015 = @"";
        public const string x016 = @"";
        public const string x017 = @"";
        public const string x018 = @"";
        public const string x019 = @"";
        public const string x020 = @"";
        public const string x021 = @"";
    }

}