using Butterfly;

public sealed class Room : RoomService
{
    private readonly Dictionary<string, Client> _clients
        = new Dictionary<string, Client>();

    void Construction()
    {

    }

    void Start()
    {

    }

    void Configurate()
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