namespace server.component.clientManager.component
{
    /// <summary>
    /// Сообщение приходящие из комнаты.
    /// </summary>
    public struct RoomMessage
    {
        public const string SUBSCRIBE_IN_ROOM 
            = "Вы успешно подписались в комнату.";
        
        public const string UNSUBSCRIBE_FROM_ROOM 
            = "Вы успешно отписались из комнаты.";
    }
}