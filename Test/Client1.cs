using Butterfly;

public class Client : Controller.Board
{
    void Start()
    {
        SystemInformation("start");

        gameClient.Client client = new gameClient.Client();

        client.Start("login", "password", 
            Header.ADDRESS, Header.SSL_PORT, Header.TCP_PORT, Header.UDP_PORT);
    }
}

public class ClientTest
{
    // Записываем пакеты.
    private byte[][] bytes = new byte[1024][];

    public void SendUpDownLeftMove()
    {
    }
}

public class ServerTest
{
    public void Receive()
    {
    }
}