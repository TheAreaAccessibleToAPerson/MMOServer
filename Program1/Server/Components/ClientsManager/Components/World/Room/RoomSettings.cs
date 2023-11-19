namespace server.component.clientManager.component
{
    public sealed class RoomSettings 
    {
        public readonly string EventName;

        public RoomSettings(string eventName)
        {
            EventName = eventName;
        }
    }
}