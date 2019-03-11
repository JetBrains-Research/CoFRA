namespace TaintTrackingTests
{
    class TaintedAttribute : Attribute
    {
    }

    class Program
    {
        [Tainted] 
        public int A;
		
		public int C;

        public int B;  

        static void Main(string[] args)
        {
            Program a = new Program(); 
            a.C = a.A;
            a.B = a.C;
        }
    }
}