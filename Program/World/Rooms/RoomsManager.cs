#define INFORMATION
#define EXCEPTION

using Butterfly;

public sealed class RoomsManager : RoomsManagerServices
{
    public const string ROOMS_MANAGER_WORK_EVENT = "Rooms manager work";

    void Construction()
    {
        listen_events(ROOMS_MANAGER_WORK_EVENT, ROOMS_MANAGER_WORK_EVENT);

        listen_message<ulong, IInput<string, uint, byte[], Client.IReceiveRoomMessage>>
            (BUS.SUBSCRIBE_IN_MANAGER)
                .output_to(Subscribe, ROOMS_MANAGER_WORK_EVENT);

        listen_message<ulong, string, uint, byte[], Client.IReceiveRoomMessage>(BUS.SEND_MESSAGE_TO_ROOM)
            .output_to(SendMessageToRoom, ROOMS_MANAGER_WORK_EVENT);
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
        /// <summary>
        /// Подписать Room в менеджере.
        /// </summary>
        public const string SUBSCRIBE_IN_MANAGER = "Room subscribe to manager";

        /// <summary>
        /// Создать комнату или подключится к уже сущесвующей.
        /// </summary>
        public const string CRETING_OR_CONNECTION = "Creating or connection room.";

        /// <summary>
        /// Отправить сообщение в комнату.
        /// </summary>
        public const string SEND_MESSAGE_TO_ROOM = "Send message to room.";
    }
}