#define EXCEPTION
#define INFORMATION

using Butterfly;

public abstract class RoomService : RoomProperty
{
    private readonly Dictionary<uint, Client.IReceiveRoomMessage> _clients
        = new Dictionary<uint, Client.IReceiveRoomMessage>();

    /// <summary>
    /// Регистрируется в RoomsManager.
    /// </summary>
    protected IInput<ulong, IInput<string, uint, byte[]>> I_subscribeIsRoomsManager;


    public struct EX
    {
    }
}