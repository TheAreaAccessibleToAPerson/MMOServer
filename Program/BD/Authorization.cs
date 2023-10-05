using Butterfly;

public interface ISelect 
{
    string Select(string str);
}

public class Authorization : Controller, ISelect
{
    public string Select(string str)
    {
        return "";
    }
}