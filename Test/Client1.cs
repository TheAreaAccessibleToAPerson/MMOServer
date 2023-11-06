using Butterfly;

public class Client1 : Controller.Board
{
    void Start()
    {
        SystemInformation("start");

        gameClient.Client client = new gameClient.Client();

        client.Start("login", "password", 
            Header1.ADDRESS, Header1.SSL_PORT, Header1.TCP_PORT, Header1.UDP_PORT);
    }
}