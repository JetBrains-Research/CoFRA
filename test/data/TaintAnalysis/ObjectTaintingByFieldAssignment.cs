using System;

namespace TaintTrackingTests
{
    class TaintedAttribute : Attribute
    {
    }
	
    class FilterAttribute : Attribute
    {
    }
	
    class SinkAttribute : Attribute
    {
    }
	
	class Container
	{
		public int B;
	}

    class Program
    {
        [Tainted] 
        public int A;
		
        [Filter]
        private int Filter(int a)
        {
            return a;
        }
		
        [Sink]
        private void Sink(Container c)
        {
        }
		
		private Container Store(int a)
		{
			var c = new Container();
			c.B = a;
			return c;
		}

        static void Main(string[] args)
        {
            Program a = new Program(); 
			
            var b = a.A;
			var c = a.Filter(b);
			
			var d = a.Store(b);
			var e = a.Store(c);
			
			a.Sink(d);
			a.Sink(e);
        }
    }
}