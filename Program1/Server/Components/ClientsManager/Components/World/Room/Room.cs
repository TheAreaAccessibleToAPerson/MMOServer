namespace server.component.clientManager.component
{
    public sealed class Room : RoomController
    {
        private string EVENT_NAME = Header.Event.ROOM_1;

        void Construction()
        {
            add_event(EVENT_NAME, Update);

            input_to(ref I_downMove, EVENT_NAME, DownMove);
            input_to(ref I_subscribe, EVENT_NAME, Subscribe);
        }
        
        void Configurate()
        {
            Define();
        }
    }
}