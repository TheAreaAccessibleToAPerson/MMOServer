#define INFORMATION
#define EXCEPTION

using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace Test
{
    public sealed class TestClient : Controller.Board.LocalField<string[]>, ReadLine.IInformation
    {
        private bool _isRunning = true;

        private IInput<byte[]> i_sendTCP;
        private IInput<byte[]> i_sendUDP;

        private readonly Socket _TCPSocket =
            new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private readonly Socket _UDPSocket =
            new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        private int _UDPPort;

        void Construction()
        {
            input_to(ref i_sendTCP, Header.SEND_SSL_MESSAGE_EVENT, SendTCP);
            input_to(ref i_sendUDP, Header.SEND_UDP_MESSAGE_EVENT, SendUDP);

            add_event(Header.RECEIVE_SSL_EVENT, () =>
            {
                if (_isRunning)
                {
                    try
                    {
                        int available = _TCPSocket.Available;
                        if (available > 0)
                        {
                            byte[] buffer = new byte[available];
                            _TCPSocket.Receive(buffer, 0, available, SocketFlags.None);
                            TCPMessageProcess(buffer);
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemInformation(ex.ToString(), ConsoleColor.Red);

                        destroy();
                    }
                }
            });
        }

        void SendTCP(byte[] message)
        {
            if (_isRunning)
            {
#if INFORMATION
                Console(Message.Show("SendTCP", message, 40));
#endif
                try
                {
                    _TCPSocket.Send(message);
                }
                catch { destroy(); }
            }
        }

        void SendUDP(byte[] message)
        {
            if (_isRunning)
            {
#if INFORMATION
                Console(Message.Show($"SendUDP Length:{message.Length}", message, 40));
#endif
                try
                {
                    _UDPSocket.Send(message);
                }
                catch { destroy(); }
            }
        }

        void Start()
        {
            ReadLine.Start(this);
        }

        void Configurate()
        {
            try
            {
                _TCPSocket.Connect(Field[FieldIndex.ADDRESS],
                    Convert.ToInt32(Field[FieldIndex.TCP_PORT]));

                _UDPSocket.Connect(Field[FieldIndex.ADDRESS],
                    Convert.ToInt32(Field[FieldIndex.UDP_PORT]));
            }
            catch (Exception ex)
            {
                SystemInformation(ex.ToString(), ConsoleColor.Red);

                destroy();
            }
        }

        void Stop()
        {
            if (StateInformation.IsCallConfigurate)
            {
                try
                {
                    _TCPSocket.Close();
                    _UDPSocket.Close();
                }
                catch { /* ... */ }
            }

            if (StateInformation.IsStart)
            {
                ReadLine.Stop(this);
            }
        }

        void Destruction()
        {
            _isRunning = false;
        }

        public void Command(string command)
        {
            switch (command)
            {
                case Commands.SEND_TCP_MESSAGE:


                    break;

                default:
                    Console($"{Commands.SEND_TCP_MESSAGE}:отправить tcp сообщение.");
                    break;
            }
        }

        private struct Commands
        {
            public const string SEND_TCP_MESSAGE = "send_tcp_message";
        }

        private struct FieldIndex
        {
            public const int ADDRESS = 0;
            public const int TCP_PORT = 1;
            public const int UDP_PORT = 2;
        }


        #region Hellper

        private void TCPMessageProcess(byte[] message)
        {
            Console("Message");
            byte[][] messages = SplitTCPMessage(message);
            Console("MessageLength:" + message.Length);

            for (int i = 0; i < messages.Length; i++)
            {
                if (messages[i].Length == 0) continue;

                if (message[TCPHeader.TYPE_INDEX] ==
                    ServiceTCPMessage.ServerToClient.Connecting.TYPE)
                {
                    SystemInformation("RequestFirstUDPPacket", ConsoleColor.Green);

                    byte[] t = new byte
                    [
                        ServiceUDPMessage.ClientToServer.Connecting.LENGTH
                    ];

                    int messageLength = ServiceTCPMessage.ServerToClient.Connecting.LENGTH;
                    int messageType = ServiceTCPMessage.ServerToClient.Connecting.TYPE;

                    t[2] = message[2]; t[3] = message[3];

                    // В первый байт нужно указать что это не зашифрованое сообщение.
                    t[UDPHeader.TYPE_INDEX] = (byte)(t[UDPHeader.TYPE_INDEX] ^ 
                        (byte)(ServiceUDPMessage.ClientToServer.Connecting.TYPE << 7));

                    try
                    {
                        i_sendUDP.To(t);
                    }
                    catch { destroy(); }
                }
#if EXCEPTION
                else throw new Exception(messages[TCPHeader.TYPE_INDEX].ToString());
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

                // Если сообщение равно 0, то проигнорируем его.
                if (length > 0)
                {
#if INFORMATION
                    SystemInformation($"SplitTCPMessage:Length:{length}.");
#endif
                    if (message.Length == messagesIndex++)
                        Array.Resize(ref messages, messages.Length + 1);

                    messages[^1] = message;
                }
                else
                {
#if EXCEPTION
                    string m = "";
                    foreach (byte b in message) m += $"{b} ";
                    Exception(Ex.x003, index, m);
#endif
                    return new byte[0][];
                }
            }
            while ((messageLength -= length) > 0);

#if EXCEPTION
            if (messageLength < 0)
            {
                Exception(Ex.x002, message.Length, index, messageLength);
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
                return message[TCPHeader.LENGTH_INDEX_2byte] ^
                    (message[TCPHeader.LENGTH_INDEX_1byte] << 8);
#if EXCEPTION
            else
            {
                string m = "";
                for (int i = startIndex; i < message.Length; i++) m += $"{message[i]} ";

                Exception(Ex.x001, ServiceTCPMessage.MIN_LENGTH, message.Length, m);
            }
#endif
            return 0;
        }

        #endregion

        protected struct Ex
        {
            public const string x001 = @"Минимально возможный размер сообщения который приходит " +
                @"по протоколу TCP {0}, но прeшедшее сообщение равно {1} и имеет содержание: {2}";
            public const string x002 = @"В поступающем TCP сообщении имеется зоголовок в который "
                + @"записывается длина сообщения, на случай если призайдет слипание."
                + @"Длина пришедшего сообщения не сходится с сумой длин указаных в заголовкe/ах."
                + @"Размер массива равен {0}, индекс указывающий на начало сообщения в массиве "
                + @"указывает на позицию {1}, разность от идекса и до конца массива равна {2}.";
            public const string x003 = @"В поступающем TCP сообщении содержется заголовок отвечающий " +
                @"за длину, которая не может быть равна нулю.Индекс начала:{0} Сообщение:{1}";

            public const string x004 = @"Вы можете запросить порт только когда состояние Services 
                {0}, но в данный момент флаг выставлен в {1}.";
            public const string x005 = @"";
            public const string x006 = @"";
            public const string x007 = @"";
            public const string x008 = @"";
            public const string x009 = @"";
        }
    }
}