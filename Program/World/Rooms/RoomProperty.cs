using Butterfly;

public abstract class RoomProperty : Controller.LocalField<RoomSetting>
{
    /// <summary>
    /// Под данным ключом комната будет хранится в RoomManager и доступна для 
    /// клиентов.
    /// </summary>
    protected ulong RoomsManagerKey = 0;

    protected string Name = "";

    protected uint PositionX = 0;
    protected uint PositionY = 0;

    protected uint ClientCount = 0;

    protected bool IsCreatingUpperRoom = false;
    protected bool IsCreatingLowerRoom = false;
    protected bool IsCreatingLeftRoom = false;
    protected bool IsCreatingRightRoom = false;

    protected void SetPosition(uint x, uint y)
    {
    }

    public struct EX
    {

    }
}