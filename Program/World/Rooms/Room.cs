#define EXCEPTION
#define INFORMATION

using Butterfly;

/// <summary>
/// Комната создается из комнаты. 
/// </summary>
public sealed class Room : RoomService
{
    private IInput<string, uint, byte[]> i_receiveFromRoomManager;

    /// <summary>
    /// Получает от клиeнта его позицию.
    /// </summary>
    IInput<uint, byte[]> i_receiveClientPosition;

    void Construction()
    {
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

    public interface IReceiveMessage 
    {
        void SendPosition(uint id, int positionX, int positionY);
    }

    public struct BUS
    {
        public const string LE_SUBSCRIBE_IN_ROOM = "Subscribe in room";
    }

    public struct EX
    {
    }
}