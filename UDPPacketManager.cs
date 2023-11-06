#define INFORMATION
#define EXCEPTION

using Butterfly;

public class UDPPacketManager : Controller
{
    private string WriteProcessName = "WriteUDPMessage";
    void Write(byte[] packet)
    {
        // Узнаем соответвует ли размер минимальному.
        // в пришедшем пакете может быть болше одного сообщения.
        if (packet.Length >= udp.Header.LENGTH && packet.Length <= udp.Header.MAX_LENGTH)
        {
#if INFORMATION
            SystemInformation($"Вы получили UDP пакет {Message.Show(WriteProcessName, packet, 40)}");
#endif

            /****************** Начинаем извлекать капуслy/ы ***********************/
            {
                byte[][] result = SplitPacket(packet);

#if INFORMATION
                SystemInformation($"Из пакета извлечено {result.Length} капсул.");
#endif

                foreach (byte[] capsule in result)
                {
                    int id = capsule[Capsule.Header.ID_INDEX_1byte] << 24 ^
                        capsule[Capsule.Header.ID_INDEX_2byte] << 16 ^
                        capsule[Capsule.Header.ID_INDEX_3byte] << 8 ^
                        capsule[Capsule.Header.ID_INDEX_4byte];

                    if (capsule[Capsule.Header.TYPE_INDEX] == Capsule.Header.ACK.TYPE)
                    {
#if INFORMATION
                        Console($"********************ACK:CAPSULE[ID:{id}]***********************");
#endif

                        if (capsule.Length == Capsule.Header.ACK.LENGTH)
                        {
                            int ackIDMessage = capsule[Capsule.Header.ACK.ACKNOLEDGMENT_MESSAGE_ID_1byte] << 24 ^
                                capsule[Capsule.Header.ACK.ACKNOLEDGMENT_MESSAGE_ID_2byte] << 16 ^
                                capsule[Capsule.Header.ACK.ACKNOLEDGMENT_MESSAGE_ID_3byte] << 8 ^
                                capsule[Capsule.Header.ACK.ACKNOLEDGMENT_MESSAGE_ID_4byte];

#if INFORMATION
                            Console($"Номер");
#endif
                        }
#if EXCEPTION
                        else
                        {
                            throw Exception($"Капсула {Capsule.Header.ACK.NAME} не имеет полезных данных " +
                                $"и состоит только из заголовка длиной {Capsule.Header.ACK.LENGTH}, но " +
                                $"размер поступивших данных {capsule.Length}," +
                                $"{Message.Show(WriteProcessName, capsule, 40)}");
                        }
#endif

#if INFORMATION
                        Console("******************************************************");
#endif
                    }
                    else if (capsule[Capsule.Header.TYPE_INDEX] == Capsule.Header.FIN.TYPE)
                    {
                        if (capsule.Length < Capsule.Header.FIN.LENGTH)
                        {
#if EXCEPTION
                            throw Exception($"Минимальн длина {Capsule.Header.PSH.NAME} капсулы равена {Capsule.Header.PSH.LENGTH}," +
                                $"но полученая капсула имеет длину {capsule.Length}.");
#endif

                            break;
                        }

#if INFORMATION
                        Console($"********************FIN:CAPSULE[ID:{id}]***********************");
#endif



#if INFORMATION
                        Console("******************************************************");
#endif
                    }
                    else
                    {
                        /******************************PUSH*******************************/

                        if (capsule.Length < Capsule.Header.PSH.LENGTH)
                        {
#if EXCEPTION
                            throw Exception($"Минимальн длина {Capsule.Header.PSH.NAME} капсулы равена {Capsule.Header.PSH.LENGTH}," +
                                $"но полученая капсула имеет длину {capsule.Length}.");
#endif

                            break;
                        }

#if INFORMATION
                        Console($"*******************PUSH:CAPSULE[ID:{id}]***********************");
#endif
                        {

                            int age = capsule[Capsule.Header.PSH.DATA_AGE_INDEX];
                            int mounth = capsule[Capsule.Header.PSH.DATA_MOUNTH_INDEX];
                            int day = capsule[Capsule.Header.PSH.DATA_DAY_INDEX];
                            int hour = capsule[Capsule.Header.PSH.TIME_HOUR_INDEX];
                            int min = capsule[Capsule.Header.PSH.TIME_MIN_INDEX];
                            int sec = capsule[Capsule.Header.PSH.TIME_SEC_INDEX];
                            int mill = capsule[Capsule.Header.PSH.TIME_MILL_INDEX_1byte] << 8 ^
                                        capsule[Capsule.Header.PSH.TIME_MILL_INDEX_2byte];
#if INFORMATION
                            Console($"DATA:{day}/{mounth}/20{age} {hour}:{min}:{sec}:[{mill}]");
#endif

                            int positionX = capsule[Capsule.Header.PSH.POSITION_X_INDEX_1byte] << 24 ^
                                capsule[Capsule.Header.PSH.POSITION_X_INDEX_2byte] << 16 ^
                                capsule[Capsule.Header.PSH.POSITION_X_INDEX_3byte] << 8 ^
                                capsule[Capsule.Header.PSH.POSITION_X_INDEX_4byte];

                            int positionY = capsule[Capsule.Header.PSH.POSITION_Y_INDEX_1byte] << 24 ^
                                capsule[Capsule.Header.PSH.POSITION_Y_INDEX_2byte] << 16 ^
                                capsule[Capsule.Header.PSH.POSITION_Y_INDEX_3byte] << 8 ^
                                capsule[Capsule.Header.PSH.POSITION_Y_INDEX_4byte];
#if INFORMATION
                            Console($"Position: x[{positionX}], y[{positionY}]");
#endif

                            int direction = capsule[Capsule.Header.PSH.DIRECTIONG_INDEX];
                        }
#if INFORMATION
                        Console("*****************************************************");
#endif
                    }
                }
            }
            /**********************************************************************/


#if INFORMATION
            SystemInformation($"");
#endif
        }
#if EXCEPTION
        else
        {
            if (packet.Length < udp.Header.LENGTH)
                throw Exception($"UDP пакет {Message.Show(WriteProcessName, packet, 40)} длиной {packet.Length} " +
                    $"не соответвует минимально допустимому размеру {udp.Header.LENGTH}");

            if (packet.Length > udp.Header.MAX_LENGTH)
                throw Exception($"UDP пакет {Message.Show(WriteProcessName, packet, 40)} длиной {packet.Length} " +
                    $"не соответвует максимально допустимому размеру {udp.Header.MAX_LENGTH}");
        }
#endif
    }

    /// <summary>
    /// Получаем капсулы из сообщения.
    /// Когда вызывается данный метод предпологается что UDP пакет содержит ходябы одину капсулу.
    /// </summary>
    /// <param name="packet">Пришедшее сообщение.</param>
    /// <returns></returns>
    byte[][] SplitPacket(byte[] packet)
    {
        byte[][] capsules = new byte[0][];

#if INFORMATION
        SystemInformation($"Приступаем к извлечению капсул/ы из UDP пакета.");
#endif
        // Читаем длину UDP пакета, она хранится в первых двух байтах.
        int lengthPacket = packet[udp.Header.DATA_LENGTH_INDEX_1byte] << 8 ^
                (packet[udp.Header.DATA_LENGTH_INDEX_2byte]);

#if INFORMATION
        SystemInformation($"Длина пришедшего сообщения в UDP пакeте {lengthPacket}.");
#endif

        // Вычитаем длину заголовка UDP пакета из общей длины сообщения.
        lengthPacket -= udp.Header.LENGTH;

        // Проверяем есть ли там хотябы одна капсула.
        if (lengthPacket >= Capsule.Header.LENGTH)
        {
            // Выставляем индекс на первый байт в капсуле.
            int indexPacket = udp.Header.LENGTH;

            do
            {
                // Получаем длину капсулы.
                int lengthCapsule = packet[indexPacket + Capsule.Header.LENGTH_INDEX];

#if INFORMATION
                SystemInformation($"Длина капсулы равна {lengthCapsule}.");
#endif
                // Проверям что бы карсула была получена целиком.
                if (indexPacket + lengthCapsule <= packet.Length)
                {
                    // Записываем карсулу.Подразумевается что в одном сообщении одна капсула.
                    // Но может быть и больше. В этом случае увеличиваем размер массива.
                    if (capsules.Length >= 1)
                        Array.Resize(ref capsules, capsules.Length + 1);

                    capsules[^1] = packet[indexPacket..lengthCapsule];

                    // И переставляем идекс на конец капсулы.
                    indexPacket = lengthCapsule;
                }
                else
                {
#if EXCEPTION
                    throw Exception($"В заголовке полученой карсулы в пакете {Message.Show(WriteProcessName, packet, 40)}" +
                        $" указано что ее длина состовляет {lengthCapsule}, но начиная с индекса {indexPacket}," +
                            $" до {lengthPacket} всего лишь {packet.Length - indexPacket} байт.");
#endif
                    break;
                }
            }
            // Если идекс вышел за границы пакета, то выходим.
            while (indexPacket == packet.Length);

            return capsules;
        }

        return new byte[0][];
    }

    private int AcknoledgmentMessageID = 0;
}
