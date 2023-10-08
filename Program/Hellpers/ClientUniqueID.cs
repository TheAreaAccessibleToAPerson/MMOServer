public static class ClientUniqueID 
{
    private static uint Value = 0;

    public static bool Get(out uint value)
    {
        if (Value == 0)
        {
            Random rand = new Random();
            Value = (uint)rand.Next(100000, 5000000);
        }

        value = Value++;

        if (Value >= uint.MaxValue) 
            return false;

        return true;
    }
}
