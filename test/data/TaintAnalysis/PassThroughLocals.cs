namespace TaintTrackingTests
{
    class TaintedAttribute : Attribute
    {
    }

    class Program
    {
        [Tainted] 
        public int A;

        public int B;  

        static void Main(string[] args)
        {
            Program a = new Program(); 
            var b = a.A;
            a.B = b;
        }
    }
}