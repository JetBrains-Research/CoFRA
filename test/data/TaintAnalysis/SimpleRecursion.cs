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
        private void Sink(int a)
        {
        }
		
        private int Recursive(int c, int d, int e)
        {
            if (c == 0)
            {
                return d;
            }
			
            c--;
            var r = Recursive(c, e, 0);
            return r;
        }

        static void Main(string[] args)
        {
            Program a = new Program(); 
			
            var b = a.A;
            var c = a.Filter(b);
			
            var d = a.Recursive(10, c, b);
            var e = a.Recursive(10, 0, c);
			
            a.Sink(d);
            a.Sink(e);
        }
    }
}