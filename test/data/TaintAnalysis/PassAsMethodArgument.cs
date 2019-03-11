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
		
		void Method(int argument1, int argument2, int argument3)
		{
			B = argument1;
			C = argument2;
			D = argument3;
		}

        static void Main(string[] args)
        {
            Program a = new Program(); 
			var c = a.A;
			var d = 10;
            a.Method(a.A, c, d);
        }
    }
}