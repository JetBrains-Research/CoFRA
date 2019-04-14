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
        public string B;

        public Container(string b)
        {
            B = b;
        }
    }

    class Program
    {
        [Tainted] 
        public string A;
		
        [Filter]
        private string Filter(string a)
        {
            return a;
        }
		
        [Sink]
        private void Sink(string c)
        {
        }
		
        [Sink]
        private void Sink2(Container c)
        {
        }
		
        private Container Store(string a)
        {
            var c = new Container(a);
            return c;
        }

        static void Main(string[] args)
        {
            Program a = new Program(); 
			
            var b = a.A;
            var c = a.Filter(b);

            var d = b + "a";
            var e = c + "a";

            a.Sink(d);  
            a.Sink(e); 

            var f = a.Store(d);
            var g = a.Store(e);

            a.Sink2(f);
            a.Sink2(g);
        }
    }
}