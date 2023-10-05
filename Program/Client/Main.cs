#define INFORMATION
#define EXCEPTION

public sealed class Client : ClientService
{
    void Construction()
    {
        add_event(Header.RECEIVE_SSL_EVENT, ReceiveSSL);

        input_to(ref I_sendSSL, Header.SEND_SSL_MESSAGE_EVENT, SendSSL);
        input_to(ref I_TCPMessageProcessing, Header.SEND_SSL_MESSAGE_EVENT, SSLMessageProcess);
        input_to(ref I_UDPMessageProcessing, Header.SEND_UDP_MESSAGE_EVENT, UDPMessageProcess);

        send_echo_2_0(ref I_subscribeToReceiveFirstUDPPacket, 
            ReceiveUDPPacketForClients.BUS.LE_SUBSCRIBE_CLIENT_RECEIVE_FIRST_UDP_PACKET)
                .output_to(SettingConnection, Header.WORK_WITCH_OBJECTS_EVENT);

        send_echo_3_0(ref I_subscribeToReceiveUDPPackets,
            ReceiveUDPPacketForClients.BUS.LE_SUBSCRIBE_CLIENT_RECEIVE_UDP_PACKETS)
                .output_to(SettingConnection, Header.WORK_WITCH_OBJECTS_EVENT);
    }

    void Start()
    {
        /*****************ПЕРЕДЕЛАТЬ************************/
        if (ClientUniqueID.Get(out uint id))
            ID = id;
        else
        {
            SystemInformation("Привышено значение максимально" +
                "возможного количесво ID для клиентов.");
        }
        /***************************************************/

        SystemInformation($"ID:{GetID()} creating.", ConsoleColor.Green);

        SettingConnection();
    }

    void Configurate()
    {
        try
        {
            TCPSocket = Field.GetStream().Socket;

            RemoteIPAddress = ((System.Net.IPEndPoint)TCPSocket.RemoteEndPoint).Address;
        }
        catch { destroy(); }
    }

    void Destruction()
    {
        if (StateInformation.IsCallStart)
        {
            //I_sendSSL.To(new byte[] { 0, 1, ServiceTCPMessage.ServerToClient.CLIENT_DISCONNECTING });
        }

        IsRunning = false;
    }

    #region Hellpers

    /// <summary>
    /// Отправляем сообщение по TCP.
    /// </summary>
    /// <param name="message"></param>
    private void SendSSL(byte[] message)
    {
        if (IsRunning)
        {
            try
            {
#if INFORMATION
                Console(Message.Show("SendTCP", message, 40));
#endif

                TCPSocket.Send(message);
            }
            catch { destroy(); }
        }
    }

    private void UDPMessageProcess(byte[] message)
    {

    }

    private void SSLMessageProcess(byte[] message)
    {
#if INFORMATION
        SystemInformation("TCPMessageProcess", ConsoleColor.Green);
#endif

        byte[][] messages = SplitTCPMessage(message);

        for (int i = 0; i < messages.Length; i++)
        {
            if (messages[i].Length == 0) continue;

            //if (message[TCPHeader.TYPE_INDEX] ==
            //ServiceTCPMessage.ClientToServer.TRANSFER_PORT)
            {
#if INFORMATION
                SystemInformation("TCPMessageProcess - TransferPort", ConsoleColor.Green);

                //SubscribeToReceiveUDPPacket(messages[i]);
#endif
            }
#if EXCEPTION
            //else throw new Exception(messages[TCPHeader.TYPE_INDEX].ToString());
#endif
        }
    }

    /// <summary>
    /// Сообщения могут придти склеиные, нужно их разделить.
    /// </summary>
    private byte[][] SplitTCPMessage(byte[] message)
    {
        int messageLength = message.Length;

        int length = 0;
        int index = 0;

        int messagesIndex = 0;
        byte[][] messages = new byte[1][];
        do
        {
            length = GetTCPMessageLength(message, index);

#if INFORMATION
            SystemInformation("SplitTCPMessage length:" + length);
#endif

            // Если сообщение равно 0, то проигнорируем его.
            if (length > 0)
            {
                if (message.Length == messagesIndex++)
                    Array.Resize(ref messages, messages.Length + 1);

                messages[^1] = message[index..(length - 1)];

                index = length;
            }
            else
            {
#if EXCEPTION
                throw Exception(Ex.x003, index, String.Join(" ", message), ConsoleColor.Red);
#endif
                return new byte[0][];
            }
        }
        while ((messageLength -= index) > 0);

#if EXCEPTION
        if (messageLength < 0) 
            throw Exception(Ex.x002, message.Length, index, messageLength, ConsoleColor.Red);
#endif

        return messages;
    }

    /// <summary>
    /// Определяет размер пришедшего сообщение по протоколу TCP.
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <returns></returns>
    private int GetTCPMessageLength(byte[] message, int startIndex)
    {
        if ((message.Length - startIndex) >= SSL.Header.LENGTH)
        {
            int i = message[startIndex + SSL.Header.DATA_LENGTH_INDEX_1byte] << 8 ^
                (message[startIndex + SSL.Header.DATA_LENGTH_INDEX_2byte]);

#if INFORMATION
            SystemInformation("GetTCPMessageLength:" + i);
#endif

            return i;
        }
#if EXCEPTION 
        else throw Exception(Ex.x001, SSL.Header.LENGTH, message.Length, String.Join(" ", message));
#endif
        return 0;
    }

    #endregion

    public interface IReceiveUDPPackets
    {
        /// <summary>
        /// Данный метод реализует прослушку UDP пакетов от клиeнта.
        /// </summary>
        /// <param name="message"></param>
        void Receive(byte[] message);
    }

    public interface IReceiveFirstUDPPacket
    {
        /// <summary>
        /// Данный метод реализует прослушку первого UDP пакета от клинта.
        /// ID клиента приходит не в зашифровоном виде.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="addressAndPort"></param>
        void Receive(byte[] message, ulong addressAndPort);
    }

    public interface IReceiveRoomMessage
    {
        /// <summary>
        /// Данный метод реализует прослушкy сообщений от комнат
        /// В которые мы зарегистрировались.
        /// </summary>
        /// <param name="roomName">Имя комнаты отправившая сообщение.</param>
        /// <param name="message">Сообщение</param>
        void ReceiveRoomMessage(string roomName, byte[] message);
    }
}