
using Butterfly;

namespace server.component.clientManager.component.clientShell.information
{
    public sealed class State
    {
        public enum Enum
        {
            None = 4,
            ReceiveLoginAndPassword = 8,
            Authorization = 16,
            SubscribeReceiveTCPConnection = 32,
            CreatingTCPConnection = 64,
            UnsubscribeReceiveTCPConnection = 128,
            SubscribeReceiveFirstUDPPacket = 256,
            CreatingUDPConnection = 512,
            UnsubscribeReceiveFirstUDPPacket= 1024,
        }

        private const string DESTROYING_ERROR = "Невозможно сменить состояние обьекта, как он уничтожается.";
        private const string SET_ERROR = @"Назначить состояние {0} можно при условии что текущее состояние {1}." +
            @"В данный момент состояние {2}";

        private readonly object _locker = new ();

        /// <summary>
        /// Нужно ли отписаться из списка ожидания TCP соединения.
        /// </summary>
        private bool _isUnsubscribeTCPConnection = false;
        public bool IsUnsubscribeTCPConnection { 
            private set 
            { 
                lock(_locker) _isUnsubscribeTCPConnection = value; 
            }
            get 
            {
                lock(_locker) return _isUnsubscribeTCPConnection;
            }
        }   

        /// <summary>
        /// Нужно ли отписаться из списка ожидания UDP соединения.
        /// </summary>
        private bool _isUnsubscribeUDPConnection = false;
        public bool IsUnsubscribeUDPConnection { 
            private set 
            { 
                lock(_locker) _isUnsubscribeUDPConnection = value; 
            }
            get 
            {
                lock(_locker) return _isUnsubscribeUDPConnection;
            }
        }   

        private bool _isDestroy = false;

        public void Destroy()
        {
            lock (_locker) _isDestroy = true;
        }

        public bool IsDestroy()
        {
            lock(_locker) return _isDestroy;
        }

        public Enum CurrentState { private set; get; } = Enum.None;

        public bool HasNone()
        {
            lock (_locker)
            {
                if (CurrentState.HasFlag(Enum.None))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Считываем логин и пароль.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SetReceiveLoginAndPassword(out string error)
        {
            lock (_locker)
            {
                if (_isDestroy)
                {
                    error = DESTROYING_ERROR;
                    return false;
                }

                error = null;

                if (CurrentState.HasFlag(Enum.None))
                {
                    CurrentState = Enum.ReceiveLoginAndPassword;

                    return true;
                }
                else
                {
                    error = string.Format(SET_ERROR, Enum.ReceiveLoginAndPassword, Enum.None, CurrentState);

                    return false;
                }
            }
        }

        public bool HasReceiveLoginAndPassword()
        {
            lock (_locker)
            {
                if (CurrentState.HasFlag(Enum.ReceiveLoginAndPassword))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SetAuthorization(out string error)
        {
            lock (_locker)
            {
                if (_isDestroy)
                {
                    error = DESTROYING_ERROR;
                    return false;
                }

                error = null;

                if (CurrentState.HasFlag(Enum.ReceiveLoginAndPassword))
                {
                    CurrentState = Enum.Authorization;

                    return true;
                }
                else
                {
                    error = string.Format(SET_ERROR, Enum.Authorization, Enum.ReceiveLoginAndPassword,
                        CurrentState);

                    return false;
                }
            }
        }

        public bool HasAuthorization()
        {
            lock (_locker)
            {
                if (_isDestroy) return false;

                if (CurrentState.HasFlag(Enum.Authorization))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SetSubscribeReceiveTCPConnection(out string error)
        {
            lock (_locker)
            {
                if (_isDestroy)
                {
                    error = DESTROYING_ERROR;
                    return false;
                }

                error = null;

                if (CurrentState.HasFlag(Enum.Authorization))
                {
                    CurrentState = Enum.SubscribeReceiveTCPConnection;

                    IsUnsubscribeTCPConnection = true;

                    return true;
                }
                else
                {
                    error = string.Format(SET_ERROR, Enum.SubscribeReceiveTCPConnection, 
                        Enum.Authorization, CurrentState);

                    return false;
                }
            }
        }

        public bool HasSubscribeReceiveTCPConnection()
        {
            lock (_locker)
            {
                if (_isDestroy) return false;

                if (CurrentState.HasFlag(Enum.SubscribeReceiveTCPConnection))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SetCreatingTCPConnection(out string error)
        {
            lock (_locker)
            {
                if (_isDestroy)
                {
                    error = DESTROYING_ERROR;
                    return false;
                }

                error = null;

                if (CurrentState.HasFlag(Enum.SubscribeReceiveTCPConnection))
                {
                    CurrentState = Enum.CreatingTCPConnection;

                    return true;
                }
                else
                {
                    error = string.Format(SET_ERROR, Enum.CreatingTCPConnection, 
                        Enum.SubscribeReceiveTCPConnection, CurrentState);

                    return false;
                }
            }
        }

        public bool HasCreatingTCPConnection()
        {
            lock (_locker)
            {
                if (_isDestroy) return false;

                if (CurrentState.HasFlag(Enum.CreatingTCPConnection))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SetUnsubscribeReceiveTCPConnection(out string error)
        {
            lock (_locker)
            {
                if (_isDestroy)
                {
                    error = DESTROYING_ERROR;
                    return false;
                }

                error = null;

                if (CurrentState.HasFlag(Enum.CreatingTCPConnection))
                {
                    CurrentState = Enum.UnsubscribeReceiveTCPConnection;

                    IsUnsubscribeTCPConnection = false;

                    return true;
                }
                else
                {
                    error = string.Format(SET_ERROR, Enum.UnsubscribeReceiveTCPConnection, 
                        Enum.CreatingTCPConnection, CurrentState);

                    return false;
                }
            }
        }

        public bool HasUnsubscribeReceiveTCPConnection()
        {
            lock (_locker)
            {
                if (_isDestroy) return false;

                if (CurrentState.HasFlag(Enum.UnsubscribeReceiveTCPConnection))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SetSubscribeReceiveFirstUDPPacket(out string error)
        {
            lock (_locker)
            {
                error = null;

                if (CurrentState.HasFlag(Enum.UnsubscribeReceiveTCPConnection))
                {
                    IsUnsubscribeUDPConnection = true;

                    CurrentState = Enum.SubscribeReceiveFirstUDPPacket;

                    return true;
                }
                else
                {
                    error = string.Format(SET_ERROR, Enum.SubscribeReceiveFirstUDPPacket, 
                        Enum.UnsubscribeReceiveTCPConnection,CurrentState);

                    return false;
                }
            }
        }

        public bool HasSubscribeReceiveFirstUDPPacket()
        {
            lock (_locker)
            {
                if (_isDestroy) return false;

                if (CurrentState.HasFlag(Enum.SubscribeReceiveFirstUDPPacket))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SetCreatingUDPConnection(out string error)
        {
            lock (_locker)
            {
                if (_isDestroy)
                {
                    error = DESTROYING_ERROR;
                    return false;
                }

                error = null;

                if (CurrentState.HasFlag(Enum.SubscribeReceiveFirstUDPPacket))
                {
                    CurrentState = Enum.CreatingUDPConnection;

                    return true;
                }
                else
                {
                    error = string.Format(SET_ERROR, Enum.CreatingUDPConnection, 
                        Enum.SubscribeReceiveFirstUDPPacket, CurrentState);

                    return false;
                }
            }
        }

        public bool HasCreatingUDPConnection()
        {
            lock (_locker)
            {
                if (_isDestroy) return false;

                if (CurrentState.HasFlag(Enum.CreatingUDPConnection))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SetUnsubscribeReceiveFirstUDPPacket(out string error)
        {
            lock (_locker)
            {
                error = null;

                if (CurrentState.HasFlag(Enum.CreatingUDPConnection))
                {
                    CurrentState = Enum.UnsubscribeReceiveFirstUDPPacket;

                    IsUnsubscribeUDPConnection = false;

                    return true;
                }
                else
                {
                    error = string.Format(SET_ERROR, Enum.UnsubscribeReceiveFirstUDPPacket, 
                        Enum.CreatingUDPConnection,CurrentState);

                    return false;
                }
            }
        }

        public bool HasUnsubscribeReceiveFirstUDPPacket()
        {
            lock (_locker)
            {
                if (_isDestroy) return false;

                if (CurrentState.HasFlag(Enum.UnsubscribeReceiveFirstUDPPacket))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}