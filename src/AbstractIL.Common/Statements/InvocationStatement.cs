using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Common.Types.InvocationTargets;

namespace Cofra.AbstractIL.Common.Statements
{
    [DataContract]
    public sealed class InvocationStatement : Statement
    {
        [DataMember] public readonly InvocationTarget InvocationTarget;
        [DataMember] public readonly Dictionary<ParameterIndex, Reference> PassedParameters;
        /// <summary>
        /// [-1] parameter is always for returned value,
        /// out parameters has same number as in the parameters list 
        /// </summary>
        [DataMember] public readonly Dictionary<ParameterIndex, Reference> ReturnedValues;

        [DataMember] public readonly bool IsConstructor;

        public InvocationStatement(
            Location location,
            InvocationTarget invocationTarget,
            IDictionary<ParameterIndex, Reference> passedParameters,
            IDictionary<ParameterIndex, Reference> returnedValues,
            bool isConstructor = false)
            : base(location)
        {
            InvocationTarget = invocationTarget;
            PassedParameters = new Dictionary<ParameterIndex, Reference>(passedParameters);
            ReturnedValues = new Dictionary<ParameterIndex, Reference>(returnedValues);
            IsConstructor = isConstructor;
        }

        public override StatementType Type => StatementType.Invocation;

        public override string ToString()
        {
            var paramsStr = string.Join(";", PassedParameters.Select(kvp => $"{kvp.Key} <- {kvp.Value}").ToArray());
            var returnedStr = string.Join(";", ReturnedValues.Select(kvp => $"{kvp.Key} <- {kvp.Value}").ToArray()); 
            return $"{base.ToString()}: InvTarget:{InvocationTarget}, PassedParams: {paramsStr}, ReturnedVals:{returnedStr}";
        }
    }
}