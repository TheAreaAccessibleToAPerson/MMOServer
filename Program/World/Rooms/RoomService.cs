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


    protected void Receive(string senderName, uint idClient, byte[] message)
    {
    }

    protected void ReceiveMessage(uint idClient, byte[] message)
    {

    }


    public struct EX
    {
    }

    /// <summary>
    /// Проверяет нужно ли уничтожить комнату.
    /// </summary>
    private void UpdateCheckIsDestroy()
    {

    }
}