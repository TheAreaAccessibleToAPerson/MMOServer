public sealed class RoomSetting
{
    /// <summary>
    /// Имя событие которое будет обрабатывать данную комнату.
    /// </summary>
    /// <value></value>
    public string ROOM_UPDATE_EVENT_NAME { get; init;}
}

public sealed class RoomSettingJSON
{
        public string Name { get; init; }

        public int PositionX { get; init; }
        public int PositionY { get; init; }

        public int LeftWidth { get; init; }
        public int RightWidth { get; init; }
        public int TopHeight { get; init; }
        public int BottomHeight { get; init; }

        public bool IsSafeClients { get; init; }
        public bool IsSafeNPC { get; init; }
        public bool IsSafeMobs { get; init; }

        public string[] IORoomsName { get; init; }

        public string[][] MobsName { get; init; }
}