#define INFO

using Butterfly;

namespace server.component.clientManager.component
{
    public sealed class WorldManager : Controller
    {
        public const string NAME = "WorldManager:";

        public struct BUS
        {
            public struct Echo
            {
                public const string REGISTER_IN_ROOM = NAME + "RegisterInRoom.";
            }
        }

        void Construction()
        {
            listen_echo_2_0<string, ConnectData>(BUS.Echo.REGISTER_IN_ROOM)
                .output_to((name, connect, @return) =>
                {
#if INFO
                    SystemInformation($"Получена новая заявка на подписку в комнату {name}.");
#endif
                    if (try_obj(name, out Room room))
                    {
#if INFO
                        SystemInformation($"Комната с именем {name} найдена.");
#endif
                        room.Subscribe(connect);
                    }
                    else
                    {
#if INFO
                        SystemInformation($"Комнаты с именем {name} не сущесвует, создадим ее.");
#endif

                        obj<Room>(name, new RoomSettings
                        (
                            Header.Event.ROOM_1
                        ))
                        .Subscribe(connect);
                    }
                }, Header.Event.WORK_OBJECT);
        }

        void Start()
        {
        }
    }
}
