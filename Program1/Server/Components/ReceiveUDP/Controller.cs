#define INFO
#define SCL

using Butterfly;

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
        /*
        /// <summary>
        /// Сюда пописываются клиеты которое ожидают получения UDP пакетов.
        /// </summary>
        /// <returns></returns>
        protected readonly Dictionary<string, clientShell.ConnectionController.IReceiveFirstUDPPacket> 
            _clientsReceiveUDPPackets = new ();
            */

        /// <summary>
        /// Сюда подписываются клиенты ожидающие первый UDP пакет.
        /// </summary>
        /// <typeparam name="ulong">id клинта.</typeparam>
        /// <typeparam name="Client.Main.IReceiveFirstUDPPacket">Описывает способ получение пакета.</typeparam>
        /// <returns></returns>
        protected readonly Dictionary<string, clientManager.component.clientShell.ConnectionController.IReceiveFirstUDPPacket>
            _clientsReceiveFirstUDPPackets = new();

        protected void ReceivePackets(int[] types, string[] addresses, int[] ports, byte[][] packets, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (types[i] == udp.Data.ClientToServer.Message.TYPE)
                {
                    /*
                    if (_clientsReceiveUDPPackets.TryGetValue(clientKeys[i],
                        out Client.ConnectController.IReceiveUDPPackets client))
                    {
                        client.Receive(packets[i]);
                    }
#if INFO
                    else SystemInformation($"Клиента [address:{clientKeys[i] >> 16}, " +
                        $"port{ushort.MaxValue & (int)clientKeys[i]}] ожидаеющего" +
                        "UDP пакета не сущесвует.", ConsoleColor.Red);
#endif
*/
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

        /*
                protected void Subscribe(ulong key, clientShell.ConnectionController.IReceiveFirstUDPPacket client,
                    IReturn @return)
                {
                    if (_clientsReceiveUDPPackets.ContainsKey(key))
                    {
        #if SCL
                        _logger($"Неудачная попытка подписать клиента {@return.GetKey()} по ключу {key}" +
                            $"на прослушку UDP пакетов, так как клиент с таким ключом уже подписан.");
        #endif
                    }
                    else
                    {
                        if (_clientsReceiveFirstUDPPackets.Remove(id))
                        {
        #if INFO
                            SystemInformation
                                ($"Клиент {@return.GetKey()} отписался от прослушки первого UDP пакетa.",
                                        ConsoleColor.Green);

                            SystemInformation
                                ($"Клиент {@return.GetKey()} подписался на прослушкy UDP пакетов.",
                                        ConsoleColor.Green);
        #endif
                            _clientsReceiveUDPPackets.Add(key, client);

                            // Оповестим что подписка окончена.
                            @return.To();
                        }
        #if SCL
                        else _logger($"Неудлось отписать клинта {@return.GetKey()} по ключу {id} " +
                            $"от прослушки первого UDP пакета.");
        #endif
                    }
                }
                */

        protected void SubscribeReceiveFirstUDPPacket(string address,
            clientManager.component.clientShell.ConnectionController.IReceiveFirstUDPPacket client, IReturn @return)
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

        /*
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
        */

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