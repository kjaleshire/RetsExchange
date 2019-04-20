namespace RetsExchange
{
    [System.AttributeUsage(System.AttributeTargets.Class |
                           System.AttributeTargets.Struct)
    ]
    public class RetsResource : System.Attribute
    {
        public string Class;
        public string SearchType;
    }
}
