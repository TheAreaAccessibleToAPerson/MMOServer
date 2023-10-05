using Butterfly;

public sealed class World : Controller
{
    void Construction()
    {
        obj<RoomsManager>("RoomsManager");
    }

    void Start()
    {
    }

    void Configurate()
    {
    }

    public struct BUS
    {
    }
}