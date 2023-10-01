#define INFORMATION
#define EXCEPTION

namespace Client
{
    public sealed class Main : Services, Main.IReceiveRoomMessage
    {
        void Construction()
        {
            add_event(Header.RECEIVE_TCP_SOCKET_EVENT, ReceiveTCPSocket);
            input_to(ref I_sendTCP, Header.SEND_TCP_SOCKET_EVENT, SendTCP);
            input_to(ref I_TCPMessageProcessing, Header.MESSAGE_PROCESSING_EVENT, TCPMessageProcess);
            input_to(ref I_UDPMessageProcessing, Header.MESSAGE_PROCESSING_EVENT, TCPMessageProcess);
            send_echo_2_0(ref I_subscribeToReceiveUDPPacket, 
                Server.ReceiveUDPPacketForClients.BUS.LE_SUBSCRIBE_CLIENT_RECEIVE_UDP_PACKET)
                    .output_to(EndSubscribeToReceiveUDPPacket);
        }

        void Start()
        {
            SystemInformation($"ID:{GetID()} creating.", ConsoleColor.Green);

            Task.Run(RequestPort);
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
                I_sendTCP.To(new byte[] { 0, 1, ServiceTCPMessage.ServerToClient.CLIENT_DISCONNECTING });
            }

            IsRunning = false;
        }

        #region Hellpers

        /// <summary>
        /// Отправляем сообщение по TCP.
        /// </summary>
        /// <param name="message"></param>
        private void SendTCP(byte[] message)
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

        private void TCPMessageProcess(byte[] message)
        {
#if INFORMATION
            SystemInformation("TCPMessageProcess", ConsoleColor.Green);
#endif

            byte[][] messages = SplitTCPMessage(message);

            for (int i = 0; i < messages.Length; i++)
            {
                if (messages[i].Length == 0) continue;

                if (message[TCPMessage.TYPE_INDEX] ==
                    ServiceTCPMessage.ClientToServer.TRANSFER_PORT)
                {
#if INFORMATION
                    SystemInformation("TCPMessageProcess - TransferPort", ConsoleColor.Green);

                    SubscribeToReceiveUDPPacket(messages[i]);
#endif
                }
#if EXCEPTION
                else throw new Exception(messages[TCPMessage.TYPE_INDEX].ToString());
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

                    messages[^1] = message[(index + 2) .. (length + 2)];

                    index = length + 2;
                }
                else
                {
#if EXCEPTION
                    Exception(Ex.x003, index, String.Join(" ", message), ConsoleColor.Red);
#endif
                    return new byte[0][];
                }
            }
            while ((messageLength -= index) > 0);

#if EXCEPTION
            if (messageLength < 0)
            {
                Exception(Ex.x002, message.Length, index, messageLength, ConsoleColor.Red);
            }
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
            if ((message.Length - startIndex) >= ServiceTCPMessage.MIN_LENGTH)
            {
                int i = message[startIndex + TCPMessage.LENGTH_INDEX_2byte] ^
                    (message[startIndex + TCPMessage.LENGTH_INDEX_1byte] << 8);

#if INFORMATION
                SystemInformation("GetTCPMessageLength:" + i);
#endif

                return i;
            }
#if EXCEPTION
            else
            {
                Exception(Ex.x001, ServiceTCPMessage.MIN_LENGTH, message.Length, String.Join(" ", message));
            }
#endif
            return 0;
        }

        #endregion

        public interface IReceiveUDPPacket
        {
            /// <summary>
            /// Данный метод реализует прослушку UDP пакетов от клиeнта.
            /// </summary>
            /// <param name="packet"></param>
            void ReceiveUDPPacket(byte[] packet);
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
}