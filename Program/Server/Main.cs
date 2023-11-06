using Butterfly;

public sealed class Server1 : Controller.Board.LocalField<string[]>, ReadLine.IInformation
{
    private readonly List<string> _clientsListens = new List<string>();

    void Construction()
    {
        obj<ClientsManager>("ClientsManager");

        listen_message<string, string>(BUS.LM_RESTART_CLIENTS_LISTEN)
            .output_to((address, port) =>
            {
                string name = $"{address}:{port}";

                if (address == "") Exception(EX.x1000001, name);
                if (port == "") Exception(EX.x1000006, name);
                if (try_obj(name)) Exception(EX.x1000005, name);

                obj<ClientsListen>(name, new string[]
                {
                        address, port
                });
            },
            Header.WORK_WITCH_OBJECTS_EVENT);

        listen_message<string, bool>(BUS.LM_ADD_OR_REMOVE_LISTEN_CLIENTS)
            .output_to((name, type) =>
            {
                if (type)
                {
                    if (_clientsListens.Contains(name))
                    {
                        Exception(EX.x1000003, name);
                    }
                    else _clientsListens.Add(name);
                }
                else
                {
                    if (_clientsListens.Remove(name))
                    {
                        /* ... */
                    }
                    else Exception(EX.x1000004, name);
                }
            },
            Header.WORK_WITCH_OBJECTS_EVENT);
    }

    void Start()
    {
        ReadLine.Start(this);

        if (Field.Length == ClientsListen.LOCAL_FIELD_COUNT)
        {
            obj<ClientsListen>
                ($"{Field[ClientsListen.ADDRESS_INDEX]}:{Field[ClientsListen.PORT_INDEX]}", Field);
        }
        else Exception(EX.x1000002, ClientsListen.LOCAL_FIELD_COUNT, Field.Length);
    }

    void Stop()
    {
        if (StateInformation.IsCallStart)
        {
            ReadLine.Stop(this);
        }
    }

    public void Command(string command)
    {
    }

    public struct BUS
    {
        /* 
            Если прослушка сломается, то она попытается вастановится.
            [listen_message<in string:адрес прослушки, in int: порт прослушки>]
        */
        public const string LM_RESTART_CLIENTS_LISTEN = "Restart clients listen";

        /*
            Если прослушка была успешно запущена или прекратила свою работу, 
            то она сообщит серверу об этом.
            [listen_message<in string:имя прослушки, bool:true - add, false - remove]
        */
        public const string LM_ADD_OR_REMOVE_LISTEN_CLIENTS = "Add or remove client listen";
    }

    public struct CMD
    {
    }

    private struct EX
    {
        public const string x1000001 = @"Вы пытаетесь перезапустить прослушку {0}, но получили пустую строку в качесве адреса.";
        public const string x1000002 = @"Вы пытаетесь создать прослушку клиетов, которая ожидает на вход {0} локальных параметра, но передано лишь {1}.";
        public const string x1000003 = @"ClientsListener дважды попытался записать свое имя {0} в список.";
        public const string x1000004 = @"Несущесвyюий ClientListener пытается удалить свое имя {0} из списка.";
        public const string x1000005 = @"Вы пытаетесь перезапустить запущеный ClientsListen {0}.";
        public const string x1000006 = @"Вы пытаетесь перезапустить прослушку {0}, но получили пустую строку в качесве порта.";
        public const string x1000007 = "";
        public const string x1000008 = "";
        public const string x1000009 = "";
    }
}
