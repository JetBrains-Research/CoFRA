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
    
    delegate int Brace(int a);

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
		
        private int Brace1(int a)
        {
            return a;
        }

        private int Brace2(int a)
        {
            return Filter(a);
        }

        private int Call(Brace brace, int v)
        {
            return brace(v);
        }

        static void Main(string[] args)
        {
            Program a = new Program(); 
			
            var b = a.A;
            var c = a.Filter(b);

            var d = a.Call(a.Brace1, b);
            var e = a.Call(a.Brace2, b);

            var f = a.Call(a.Brace1, c);
            var g = a.Call(a.Brace2, c);
			
            a.Sink(d);
            a.Sink(e); // False positive!!!
            a.Sink(f);
            a.Sink(g);

            Brace anonBrace = i => i;

            var h = a.Call(anonBrace, b);
            var j = a.Call(anonBrace, c);

            a.Sink(h);
            a.Sink(j);
        }
    }
}