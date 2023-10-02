#define INFORMATION
#define EXCEPTION

using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace Client
{
    public abstract class Services : Property, Main.IReceiveUDPPacket
    {
        protected bool IsRunning = true;

        public enum State
        {
            None = 0,

            // Запрашиваем порт с которого клиент будет отправлять UDP пакеты.
            RequestPort = 1,

            // Ожидаем пока клиент вышлет UDP пакет.
            WaitingPort = 2,
        }

        private State CurrentState = State.None;

        protected Socket TCPSocket;

        /// <summary>
        /// Аддрес клиента.
        /// </summary>
        protected IPAddress RemoteIPAddress;

        /// <summary>
        /// UDP порт с которого будут приходить сообщения от клинта.
        /// </summary>
        protected int RemoteUDPPort;

        /// <summary>
        /// По данному ключу клиeнт будет подписан в прослушку UDP пакетов.
        /// </summary>
        private ulong SubscribeKeyToReceiveUDPPackets = 0;

        protected IInput<ulong, Main.IReceiveUDPPacket> I_subscribeToReceiveUDPPacket;

        /// <summary>
        /// Отправляет сообщение по TCP сокету.
        /// </summary>
        protected IInput<byte[]> I_sendTCP;

        /// <summary>
        /// Обрабатывает входящие TCP сообщения.
        /// </summary>
        protected IInput<byte[]> I_TCPMessageProcessing;

        /// <summary>
        /// Обрабатывает входящие UDP сообщения.
        /// </summary>
        protected IInput<byte[]> I_UDPMessageProcessing;

        /// <summary>
        /// Проверяем не пришло ли, нам сообщение.
        /// </summary>
        protected void ReceiveTCPSocket()
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

        void Main.IReceiveUDPPacket.ReceiveUDPPacket(byte[] packet)
        {
            Console(packet.Length);
        }


        /// <summary>
        /// Запрашиваем у клинта с которого тот будет отправлять UDP пакеты.
        /// </summary>
        protected async void RequestPort()
        {
#if EXCEPTION
            if (CurrentState.HasFlag(State.None))
#endif
            {
#if INFORMATION
                SystemInformation("RequestPort", ConsoleColor.Green);
#endif

                I_sendTCP.To(new byte[] 
                    { 
                        0, 1, 
                        ServiceTCPMessage.ServerToClient.SEND_ID_CLIENT_AND_REQUEST_UDP_PACKET
                    });

                CurrentState = State.WaitingPort;
            }
            else Exception(Ex.x004, State.None, CurrentState);
        }


        protected struct Ex
        {
            public const string x001 = @"Минимально возможный размер сообщения который приходит " +
                @"по протоколу TCP {0}, но прeшедшее сообщение равно {1} и имеет содержание: \n{2}\n";
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
}