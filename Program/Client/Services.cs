#define INFORMATION
#define EXCEPTION

using System.Net;
using System.Net.Sockets;
using Butterfly;

namespace Client
{
    public abstract class Services : Property
    {
        protected bool IsRunning = true;

        public enum State
        {
            None = 0,

            // Запрашиваем порт с которого клиент будет отправлять UDP пакеты.
            RequestPort = 1,

            // Ожидаем пока клиент вышлет порт.
            WaitingPort = 2,

            // Получили порт подписываемся на получение UDP пакетов.
            SubscribeToReceiveUDPPacket = 4
        }

        private State CurrentState = State.None;

        protected Socket Socket;

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
                    int available = Socket.Available;
                    if (available > 0)
                    {
                        byte[] buffer = new byte[available];
                        Socket.Receive(buffer, 0, available, SocketFlags.None, out SocketError error);

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

        public void ReceiveUDPPacket(byte[] packet)
        {
            Console(packet.Length);
        }


        /// <summary>
        /// Подписываемся на получение UDP пакетов.
        /// </summary>
        protected void SubscribeToReceiveUDPPacket(byte[] message)
        {
            if (CurrentState.HasFlag(State.WaitingPort))
            {
                try
                {
#if INFORMATION
                    Console(Message.Show("SubscribeToReceiveUDPPacket", message, 40));
#endif
                    if (message.Length == 3)
                    {
                        Console(String.Join(" ", message));
                    }
                    else
                    {
                        #if EXCEPTION
                        #endif

                        destroy();
                    }

                }
                catch { destroy(); }
            }
#if EXCEPTION
            else Exception(Ex.x005, State.WaitingPort, CurrentState, ConsoleColor.Red);
#endif
        }

        /// <summary>
        /// Запрашиваем у клинта с которого тот будет отправлять UDP пакеты.
        /// </summary>
        protected async void RequestPort()
        {
            if (CurrentState.HasFlag(State.None))
            {
#if INFORMATION
                SystemInformation("RequestPort", ConsoleColor.Green);
#endif

                I_sendTCP.To(new byte[] { 0, 1, ServiceTCPMessage.ServerToClient.REQUEST_UDP_PORT });

                CurrentState = State.WaitingPort;

                /*
                await Task.Run(() =>
                {
                    Task.Delay(2000);

                    if (CurrentState.HasFlag(State.WaitingPort))
                    {
#if INFORMATION
                        SystemInformation("RequestPort - time out.", ConsoleColor.Green);
#endif

                        I_sendTCP.To(new byte[] { 0, 1, ServiceTCPMessage.ServerToClient.TIME_OUT });

                        destroy();
                    }
                });
                */
            }
            else Exception(Ex.x004, State.None, CurrentState);
        }


        #region Configurate

        protected void SetSocket()
        {
            try
            {
                Socket = Field.GetStream().Socket;
            }
            catch { destroy(); }
        }

        #endregion

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
            public const string x006 = @"";
            public const string x007 = @"";
            public const string x008 = @"";
            public const string x009 = @"";
        }
    }
}