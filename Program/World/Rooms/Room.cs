#define EXCEPTION
#define INFORMATION

using Butterfly;

/// <summary>
/// Комната создается из комнаты. 
/// </summary>
public sealed class Room : RoomService, Room.IReceiveMessage
{
    private IInput<string, uint, byte[]> i_receiveFromRoomManager;

    /// <summary>
    /// Получает от клиeнта его позицию.
    /// </summary>
    IInput<uint, byte[]> i_receiveClientPosition;

    void Construction()
    {
        //input_to(ref i_receiveFromRoomManager, Header.ROOMS_WORK_EVENT, Receive);
        send_message(ref I_subscribeIsRoomsManager, RoomsManager.BUS.SUBSCRIBE_IN_MANAGER);

        input_to(ref i_receiveClientPosition, Field.ROOM_UPDATE_EVENT_NAME, ReceiveMessage);
    }

    void Start()
    {
#if INFORMATION
        SystemInformation($"Room Name:{Name}, RoomsKey:{RoomsManagerKey}," + 
            $" PositionX:{PositionX}, PositionY:{PositionY}.", ConsoleColor.Green);
#endif

        I_subscribeIsRoomsManager.To(RoomsManagerKey, i_receiveFromRoomManager);
    }

    void Configurate()
    {
    }

    void IReceiveMessage.Send(uint id, byte[] message)
        => i_receiveClientPosition.To(id, message);

    public interface IReceiveMessage 
    {
        void Send(uint id, byte[] message);
    }

    public struct BUS
    {
        /// <summary>
        /// Подписываемся в комнату.
        /// </summary>
        public const string LE_SUBSCRIBE_TO_ROOM = "Register in room";

        /// <summary>
        /// Отписываемся от комнаты.
        /// </summary>
        public const string LE_UNSUBSCRIBE_FROM_ROOM = "Register from room";
    }

    public struct EX
    {

    }
}