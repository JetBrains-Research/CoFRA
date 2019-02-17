using System.Collections.Generic;
using System.Linq;

namespace PDASimulator.DataStructures.GSS
{
    public class GssNode<TStackSymbol, TGssData>
        where TGssData : new()
    {
        private readonly HashSet<GssNode<TStackSymbol, TGssData>> myNext;

        public readonly TStackSymbol Symbol;
        public TGssData UserData;

        public GssNode(TStackSymbol symbol)
        {
            Symbol = symbol;
            UserData = new TGssData();
            myNext = new HashSet<GssNode<TStackSymbol, TGssData>>();
        }

        public GssNode(TStackSymbol data, GssNode<TStackSymbol, TGssData> parent) : this(data)
        {
            AddParent(parent);
        }

        public IEnumerable<GssNode<TStackSymbol, TGssData>> Pop()
        {
            return myNext;
        }

        public GssNode<TStackSymbol, TGssData> Push(TStackSymbol symbol)
        {
            return new GssNode<TStackSymbol, TGssData>(symbol, this);
        }

        public void AddParent(GssNode<TStackSymbol, TGssData> parent)
        {
            myNext.Add(parent);
        }
    }
}