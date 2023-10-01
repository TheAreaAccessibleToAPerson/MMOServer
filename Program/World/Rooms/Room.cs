using Butterfly;

namespace World
{
    public sealed class Room : Controller.LocalField<RoomSetting>,
        Room.IReceiveClientMessage
    {
        private readonly Dictionary<string, Client.Main> _clients
            = new Dictionary<string, Client.Main>();

        void Construction()
        {

        }

        void Start()
        {

        }

        void Configurate()
        {

        }

        void IReceiveClientMessage.Send(string senderName, byte[] message)
        {

        }

        public struct BUS
        {
            /// <summary>
            /// Подписываемся в комнату.
            /// </summary>
            public const string LE_SUBSCRIBE_IN_ROOM = "Register in room";

            /// <summary>
            /// Отписываемся от комнаты.
            /// </summary>
            public const string LE_UNSUBSCRIBE_FROM_ROOM = "Register from room";
        }

        public struct EX
        {

        }

        public interface IRoom
        {

        }

        public interface IReceiveClientMessage
        {
            void Send(string senderName, byte[] message);
        }
    }
}