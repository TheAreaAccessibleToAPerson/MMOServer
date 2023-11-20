#define INFO

using Butterfly;

namespace server.component.clientManager.component
{
    public sealed class ClientState
    {
        public enum Enum
        {
            None = 8,

            RequestDB = 16,

            SubscribeReceiveUDPPacket = 32,

            ConnectToRoom = 64,
        }

        public Enum CurrentState { private set; get; } = Enum.None;

        private object _locker = new object();

        private const string DESTROYING_ERROR = "Невозможно сменить состояние обьекта, как он уничтожается.";

        private const string ERROR = @"Назначить состояние {0} можно при условии что текущее состояние {1}." +
            @"В данный момент состояние {2}";

        private bool _isDestroy = false;

        public void Destroy()
        {
            lock (_locker) _isDestroy = true;
        }

        public bool IsDestroy()
        {
            lock (_locker) return _isDestroy;
        }

        /// <summary>
        /// Нужно ли отписаться из прослушки входящих UDP пакетов.
        /// </summary>
        private bool _isUnsubscribeReceiveUDPPacket = false;

        public bool IsUnsubscribeReceiveUDPPacket
        {
            get { lock (_locker) return _isUnsubscribeReceiveUDPPacket; }
        }

        private bool _isDisconnectFromRoom = false;

        public bool IsDisconnectFromRoom
        {
            get { lock (_locker) return _isDisconnectFromRoom; }
        }

        public bool HasNone()
        {
            lock (_locker)
            {
                if (CurrentState.HasFlag(Enum.None))
                {
                    return true;
                }
                else return false;
            }
        }

        public bool RequestDB(out string error)
        {
            lock (_locker)
            {
                if (_isDestroy)
                {
                    error = DESTROYING_ERROR;

                    return false;
                }


                if (CurrentState.HasFlag(Enum.None))
                {
                    error = null;

                    CurrentState = Enum.RequestDB;

                    return true;
                }
                else
                {
                    error = string.Format(ERROR, Enum.RequestDB,
                        Enum.None, CurrentState);

                    return false;
                }
            }
        }

        public bool HasRequestDB()
        {
            lock (_locker)
            {
                if (CurrentState.HasFlag(Enum.RequestDB))
                {
                    return true;
                }
                else return false;
            }
        }

        public bool SubscribeReceiveUDPPackets(out string error)
        {
            lock (_locker)
            {
                if (_isDestroy)
                {
                    error = DESTROYING_ERROR;

                    return false;
                }


                if (CurrentState.HasFlag(Enum.RequestDB))
                {
                    error = null;

                    _isUnsubscribeReceiveUDPPacket = true;

                    CurrentState = Enum.SubscribeReceiveUDPPacket;

                    return true;
                }
                else
                {
                    error = string.Format(ERROR, Enum.SubscribeReceiveUDPPacket,
                        Enum.RequestDB, CurrentState);

                    return false;
                }
            }
        }

        public bool HasSubscribeReceiveUDPPackets()
        {
            lock (_locker)
            {
                if (CurrentState.HasFlag(Enum.SubscribeReceiveUDPPacket))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool ConnectToRoom(out string error)
        {
            lock (_locker)
            {
                if (_isDestroy)
                {
                    error = DESTROYING_ERROR;

                    return false;
                }


                if (CurrentState.HasFlag(Enum.SubscribeReceiveUDPPacket))
                {
                    error = null;

                    CurrentState = Enum.ConnectToRoom;

                    _isDisconnectFromRoom = true;

                    return true;
                }
                else
                {
                    error = string.Format(ERROR, Enum.ConnectToRoom,
                        Enum.SubscribeReceiveUDPPacket, CurrentState);

                    return false;
                }
            }
        }
    }

    public abstract class ClientController : Controller.LocalField<ConnectData>
    {
        protected IInput I_process;

        protected IInput<ConnectData> I_requestDB;
        protected IInput<string, ConnectData> I_registerInRoom;
        protected IInput<string, ConnectData> I_subscribeReceiveUDPPackets;
        protected IInput<string> I_unsubscribeReceiveUDPPackets;

        private ClientState _state = new();


        protected void Starting()
        {
            if (_state.HasNone())
            {
                if (_state.RequestDB(out string error))
                {
                    I_requestDB.To(Field);
                }
                else Destroy(error);
            }
            else if (_state.HasRequestDB())
            {
                if (_state.SubscribeReceiveUDPPackets(out string error))
                {
                    I_subscribeReceiveUDPPackets.To(Field.UDPAddress, Field);
                }
                else Destroy(error);
            }
            else if (_state.HasSubscribeReceiveUDPPackets())
            {
                if (_state.ConnectToRoom(out string error))
                {
                    //..
                }
            }
            else Destroy($"Неудалось произвести смену состояния с {_state.CurrentState}.");
        }

        private void Destroy(string info)
        {
#if INFO
            SystemInformation(info, ConsoleColor.Red);
#endif

            destroy();
        }
    }
}