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
		
		public int C;
		
		public int D;
		
		public int E;
		
		int Method(int argument)
		{
			var a = argument;
			return a;
		}
		
		int Method2()
		{
			return A;
		}

        static void Main(string[] args)
        {
            Program a = new Program(); 
			B = a.Method(a.A);
			D = a.Method(a.C);
			E = a.Method2();
        }
    }
}