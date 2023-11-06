/// <summary>
/// Состояния в котором находится клиент.
/// </summary>
public enum StateType
{
    None = 4,

    // Ожидаем логин и пароль.
    WaitLoginAndPassword = 8,

    // Подписываемся на получение первого UDP пакета.
    SubscribeToReceiveFirstUDPPacket = 16,

    // По SSL соединение запросим у клиента первый UDP пакет.
    RequestFirstUDPPacket = 32,

    // Подписываемся на получение UDP пакетов.
    SubscribeToReceiveUDPPackets = 64 
}
