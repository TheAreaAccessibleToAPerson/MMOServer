#define INFORMATION
#define EXCEPTION

using Butterfly;

public sealed class RoomsManager : RoomsManagerServices
{
    public const string ROOMS_MANAGER_WORK_EVENT = "Rooms manager work";

    public const string ROOMS_WORKS_1 = "Rooms works 1";

    void Construction()
    {
    }

    void Start()
    {
#if INFORMATION
        SystemInformation("RoomManager start");
#endif
    }

    void Configurate()
    {
    }

    public struct BUS
    {
    }
}