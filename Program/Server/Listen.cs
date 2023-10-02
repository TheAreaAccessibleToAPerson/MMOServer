using System.Net.Sockets;
using System.Net;

using Butterfly;

public sealed class ClientsListen : Controller.Board.LocalField<string[]>
{
    public const string _ = "/";

    // Количесво ожидаемых локальных значений.
    public const int LOCAL_FIELD_COUNT = 2;

    public const int ADDRESS_INDEX = 0;
    public const int PORT_INDEX = 1;

    private TcpListener _listener;

    private IPEndPoint _localPoint;

    private bool _isRunning = true;

    // Удаляет имя текущего слушателя клинтов из сервера.
    private IInput<string, bool> i_addOrRemoveInServer;
    // Отправляет нового подключеного клиента.
    private IInput<TcpClient> i_addNewClient;
    // Перезапускает текущюю прослушку.
    private IInput<string, string> i_restart;

    void Construction()
    {
        send_message(ref i_addOrRemoveInServer, Server.BUS.LM_ADD_OR_REMOVE_LISTEN_CLIENTS);
        send_message(ref i_restart, Server.BUS.LM_RESTART_CLIENTS_LISTEN);
        send_message(ref i_addNewClient, ClientsManager.BUS.LM_CREATING_CLIENT);

        add_thread($"ListenClients:{GetKey()}", () =>
        {
            if (_isRunning == false) return;

            try
            {
                if (_listener.Pending())
                {
                    do
                    {
                        TcpClient client = _listener.AcceptTcpClient();

                        i_addNewClient.To(client);
                    }
                    while (_listener.Pending());
                }
            }
            catch (Exception ex)
            {
                SystemInformation(ex.ToString(), ConsoleColor.Red);

                destroy();

                i_restart.To(Field[ADDRESS_INDEX], Field[PORT_INDEX]);
            }
        }, 100, Thread.Priority.Highest);
    }

    void Start()
    {
        SystemInformation("starting ...", ConsoleColor.Green);

        _listener.Start();

        i_addOrRemoveInServer.To(GetKey(), true);

        SystemInformation("start.", ConsoleColor.Green);
    }

    void Configurate()
    {
        SystemInformation("start configurate ...", ConsoleColor.Green);
        {
            if (Field.Length == LOCAL_FIELD_COUNT)
            {
                _localPoint = new IPEndPoint
                    (IPAddress.Parse(Field[ADDRESS_INDEX]), Convert.ToInt32(Field[PORT_INDEX]));

                _listener = new TcpListener(_localPoint);
            }
            else Exception(EX.x1000001, LOCAL_FIELD_COUNT, Field.Length);
        }
        SystemInformation("end configurate.", ConsoleColor.Green);
    }

    void Destruction()
    {
        _isRunning = false;

        SystemInformation("destruction.", ConsoleColor.Green);
    }

    void Stop()
    {
        if (StateInformation.IsCallStart)
            i_addOrRemoveInServer.To(GetKey(), false);

        try
        {
            _listener.Stop();
        }
        catch { /* ... */ }

        SystemInformation("stop.", ConsoleColor.Green);
    }

    void Destroying()
    {
        SystemInformation("destroying.", ConsoleColor.Green);
    }

    private struct EX
    {
        public const string x1000001 = @"Передано невеное количево локальных данных, ожидалось {0}, но получено {1}.";
    }
}
