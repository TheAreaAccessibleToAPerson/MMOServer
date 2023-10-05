using Butterfly;

public sealed class RoomsManager : Controller
{
    private readonly Dictionary<string, Room> _rooms
        = new Dictionary<string, Room>();

    void Construction()
    {
        _rooms.Add("Room_A1", obj<Room>("Room_A1"));
    }

    void Start()
    {
    }

    public struct BUS
    {
        ///<summary>
        /// 
        ///</summary>
    }
}