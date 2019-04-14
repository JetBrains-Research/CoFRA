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
		
        private int Recursive1(int c, int d)
        {
            var r = Recursive2(c - 1, d);
            return r;
        }
		
        private int Recursive2(int c, int d)
        {
            if (c == 0)
            {
                return d;
            }
			
            var r = Recursive1(c, d);
            return r;
        }

        static void Main(string[] args)
        {
            Program a = new Program(); 
			
            var b = a.A;
            var c = a.Filter(b);
			
            var d = a.Recursive1(10, b);
            var e = a.Recursive1(10, c);
			
            a.Sink(d);
            a.Sink(e);
        }
    }
}