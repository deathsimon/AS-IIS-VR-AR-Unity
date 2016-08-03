public class ZEDException : System.Exception
{

    public ZEDException() : base() { }
    public ZEDException(string message)
        : base(message)
    {

    }
    public ZEDException(string message, System.Exception inner) : base(message, inner) { }



}