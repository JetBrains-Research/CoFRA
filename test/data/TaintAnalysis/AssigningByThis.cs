using System;
using System.Runtime.InteropServices;

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

        public void Store(int a)
        {
            B = a;
        }

        public void Store2(int a)
        {
            this.B = a;
        } 
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
            c.Store(a);
            return c;
        }

        private Container Store2(int a)
        {
            var c = new Container();
            c.Store2(a);
            return c;
        }

        static void Main(string[] args)
        {
            Program a = new Program(); 
			
            var b = a.A;
            var c = a.Filter(b);
			
            var d = a.Store(b);
            var e = a.Store(c);

            var f = a.Store2(b);
            var g = a.Store2(c);

            a.Sink(d);
            a.Sink(e);
            a.Sink(f);
            a.Sink(g);
        }
    }
}