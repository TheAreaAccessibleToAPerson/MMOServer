namespace server.component.clientManager.component
{
    public sealed class ClientSubscribeInRoomState
    {
        public enum Enum
        {
            None = 0,
            // Загружаем информацию о клинте из BD.
            // Его комнату ...
            LoadingInformationClient = 1,
            // Регистрируемся в комнате.
            SubscribeToRoom = 2,
        }

        public const string ERROR = @"Невозможно сменить состояние клинта с {0}" +
            @"на {1}. Ожидалось что текущее состояние будет {2}.";
    }
}