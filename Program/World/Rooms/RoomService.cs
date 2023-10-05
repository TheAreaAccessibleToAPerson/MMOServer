using Butterfly;

public abstract class RoomService : Controller.LocalField<RoomSetting>,
    Room.IReceiveClientMessage
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

    void Room.IReceiveClientMessage.Send(string senderName, byte[] message)
    {

    }

    public struct EX
    {

    }
}