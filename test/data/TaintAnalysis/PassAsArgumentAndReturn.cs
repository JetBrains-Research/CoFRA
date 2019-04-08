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
		
		private void PreSink(int a)
		{
			Sink(a);
		}
		
		private int PostSource()
		{
			var b = A;
			return b;
		}
		
		private int Brace(int a)
		{
			var b = a;
			return b;
		}

        static void Main(string[] args)
        {
            Program a = new Program(); 
			
            var b = a.A;
			a.PreSink(b);
			
			var c = a.PostSource();
			a.Sink(c);
			
			var d = a.Brace(b);
			a.Sink(d)
			
			var e = Filter(b);
			var f = Brace(e);
			a.Sink(f);
        }
    }
}