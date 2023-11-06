#define INFO

using System.Net.Sockets;
using Butterfly;

namespace server.component.clientManager.component.clientShell.Handler
{
    public abstract class Messages : Controller.Board.LocalField<TcpClient>
    {
        /// <summary>
        /// Сообщения могут придти склеиные, нужно их разделить.
        /// </summary>
        public byte[][] SplitTCPMessage(byte[] message, int size)
        {
            if (message.Length == 0)
                return new byte[0][];

            int length = 0;
            int index = 0;

            int messagesIndex = 0;
            byte[][] messages = new byte[1][];
            do
            {
                length = GetTCPMessageLength(message, index);

#if INFO
                SystemInformation($"После проверки на слипание и коректность, длина сообщения равна:" +
                    $"{length}.", ConsoleColor.Green);
#endif

                // Если сообщение равно 0, то проигнорируем его.
                if (length > 0)
                {
                    if (message.Length == messagesIndex++)
                        Array.Resize(ref messages, messages.Length + 1);

                    messages[^1] = message[index..length];

                    index = length;
                }
                else
                {
#if INFO
                    SystemInformation($"Сообщение равно 0.", ConsoleColor.Red);
#endif
                    return new byte[0][];
                }
            }
            while ((size -= index) > 0);

            return messages;
        }
        /// <summary>
        /// Определяет размер пришедшего сообщение по протоколу TCP.
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        private int GetTCPMessageLength(byte[] message, int startIndex)
        {
            if ((message.Length - startIndex) >= ssl.Header.LENGTH)
            {
                int i = message[startIndex + ssl.Header.DATA_LENGTH_INDEX_1byte] << 8 ^
                    (message[startIndex + ssl.Header.DATA_LENGTH_INDEX_2byte]);

                return i;
            }
#if INFO
            SystemInformation($"Индекс начала сообщения больше чем само сообщение " +
                $" index = {startIndex}, message length = {message.Length}", ConsoleColor.Red);
#endif
            return 0;
        }
    }
}