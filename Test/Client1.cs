using Butterfly;

public class Client1 : Controller.Board
{
    void Start()
    {
        SystemInformation("start");

        gameClient.Client client = new gameClient.Client();

        client.Start("login", "password", 
            Header.ADDRESS, Header.SSL_PORT, Header.TCP_PORT, Header.UDP_PORT);
    }
}