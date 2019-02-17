using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;

namespace Cofra.Core.Analyzes
{
    public class AnalyzesResultsCache
    {
        private readonly Dictionary<AnalysisType, object> myResults;

        public AnalyzesResultsCache()
        {
            myResults = new Dictionary<AnalysisType, object>();
        }

        public void StoreResult<TResult>(AnalysisType type, TResult result)
        {
            lock (myResults)
            {
                myResults[type] = result;
            }
        }

        public TResult GetResult<TResult>(AnalysisType type)
        {
            lock (myResults)
            {
                myResults.TryGetValue(type, out var result);
                return (TResult) result;
            }
        }
    }
}
