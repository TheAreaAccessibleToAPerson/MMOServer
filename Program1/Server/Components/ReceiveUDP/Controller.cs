#define INFO
#define SCL

using Butterfly;
using gameClient.manager;

namespace server.component.ReceiveUDP
{
    public abstract class Controller : Butterfly.Controller.LocalField<Settings.ReceiveUDP>
    {
#if SCL
        /// <summary>
        /// Отправляет сообщение в раздел логгера 
        /// хранящего информацию о компонентах сервера.
        /// </summary>
        protected IInput<string> i_componentLogger;
#endif
        /// <summary>
        /// Сюда пописываются клиеты которое ожидают получения UDP пакетов.
        /// </summary>
        /// <returns></returns>
        protected readonly Dictionary<string, clientManager.component.UDP.IReceivePackets>
            _clientsReceiveUDPPackets = new();

        /// <summary>
        /// Сюда подписываются клиенты ожидающие первый UDP пакет.
        /// </summary>
        /// <typeparam name="ulong">id клинта.</typeparam>
        /// <typeparam name="Client.Main.IReceiveFirstUDPPacket">Описывает способ получение пакета.</typeparam>
        /// <returns></returns>
        protected readonly Dictionary<string, clientManager.component.clientShell.ConnectionController.IReceiveFirstUDPPacket>
            _clientsReceiveFirstUDPPackets = new();

        protected void ReceivePackets(int[] types, string[] addresses, int[] ports,
            byte[][] packets, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (types[i] == udp.Data.ClientToServer.Message.TYPE)
                {
                    if (_clientsReceiveUDPPackets.TryGetValue(addresses[i],
                        out clientManager.component.UDP.IReceivePackets client))
                    {
                        client.Receive(packets[i]);
                    }
#if INFO
                    else SystemInformation($"Клиента [address:{addresses[i]}, " +
                        $"port{ports[i]}] ожидаеющего" +
                        "UDP пакета не сущесвует.", ConsoleColor.Red);
#endif
                }
                else if (types[i] == udp.Data.ClientToServer.Connection.Step.TYPE)
                {
                    if (packets[i].Length == udp.Data.ClientToServer.Connection.Step.LENGTH)
                    {
                        if (_clientsReceiveFirstUDPPackets.TryGetValue(addresses[i],
                            out clientManager.component.clientShell.ConnectionController.IReceiveFirstUDPPacket client))
                        {
                            client.Receive(packets[i], addresses[i], ports[i]);
                        }
                    }
                }
            }
        }

        protected void SubscribeReceiveUDPPackets(string address,
            clientManager.component.UDP.IReceivePackets client, IReturn @return)
        {
            if (_clientsReceiveUDPPackets.ContainsKey(address))
            {
#if SCL
                _logger($"Неудачная попытка подписать клиента {@return.GetKey()} по ключу {address}" +
                    $"на прослушку UDP пакетов, так как клиент с таким ключом уже подписан.");
#endif
            }
            else
            {
#if INFO
                SystemInformation($"Клиент {@return.GetKey()} подписался на прослушку " +
                    $"входящих UDP пакетов.");
#endif

                _clientsReceiveUDPPackets.Add(address, client);

                @return.To();
            }
        }


        protected void UnsubscribeReceiveUDPPackets(string address, IReturn @return)
        {
            if (_clientsReceiveUDPPackets.Remove(address))
            {
#if INFO
                    SystemInformation
                        ($"Клиент {@return.GetKey()}/" +
                            $"{address} отписался от прослушки UDP пакетов.",
                                ConsoleColor.Green);
#endif
            }
#if SCL
            else 
                _logger($"Неудалось отписаться клиенту {@return.GetKey()} по ключу {address} " + 
                    "от прослушки udp пакетов.");
#endif
        }

        protected void SubscribeReceiveFirstUDPPacket(string address,
            clientManager.component.clientShell.ConnectionController.IReceiveFirstUDPPacket client,
                IReturn @return)
        {
            if (_clientsReceiveFirstUDPPackets.ContainsKey(address))
            {
#if SCL
                _logger($"Клиент {@return.GetKey()} не смог зарегистрироваться в список ожидания " +
                    $" первого UDP пакета по id {address}");
#endif
            }
            else
            {
                _clientsReceiveFirstUDPPackets.Add(address, client);

#if INFO
                SystemInformation
                    ($"Клиент id:{address}, key:{@return.GetKey()} подписался на получение " +
                        "первого UDP пакета", ConsoleColor.Green);
#endif
                // Сигнализируем что мы успешно подписались.
                @return.To();
            }
        }

        protected void UnsubscribeReceiveFirstUDPPacket(string address, IReturn @return)
        {
            if (_clientsReceiveFirstUDPPackets.Remove(address))
            {
#if INFO
                SystemInformation
                    ($"Клиент {@return.GetKey()} отписался от прослушки первого UDP пакетa.",
                            ConsoleColor.Green);
#endif

                // Оповестим что подписка окончена.
                @return.To();
            }
#if SCL
            else _logger($"Неудлось отписать клиeнта {@return.GetKey()} по ключу {address} " +
                $"от прослушки первого UDP пакета.");
#endif
        }

#if SCL
        protected void _logger(string info)
        {
            if (StateInformation.IsCallConstruction)
                i_componentLogger.To($"{GetExplorer()}:{info}");
        }
#endif
    }
}