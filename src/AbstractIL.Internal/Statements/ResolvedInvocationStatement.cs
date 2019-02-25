using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cofra.AbstractIL.Common.Statements;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Internal.Types;
using Cofra.AbstractIL.Internal.Types.Primaries;
using Cofra.AbstractIL.Internal.Types.Secondaries;

namespace Cofra.AbstractIL.Internal.Statements
{
    [DataContract(Name = "ResolvedInvocationStatement")]
    [KnownType(typeof(ResolvedInvocationStatement<int>))]
    public sealed class ResolvedInvocationStatement<TNode> : InternalStatement
    {
        //TODO: initialize this field properly
        [DataMember] 
        public readonly Entity TargetEntity;

        [DataMember] 
        public readonly ResolvedMethodId TargetMethodId;

        [DataMember]
        public readonly Dictionary<SecondaryEntity, ParameterIndex> PassedParameters;

        [DataMember]
        public readonly Dictionary<ParameterIndex, SecondaryEntity> ReturnedValues;

        public ResolvedInvocationStatement(
            InvocationStatement source,
            Dictionary<SecondaryEntity, ParameterIndex> passedParameters,
            Dictionary<ParameterIndex, SecondaryEntity> returnedValues, 
            Entity resolvedEntity, 
            ResolvedMethodId resolvedMethodId) 
            : base(source.Location)
        {
            TargetEntity = resolvedEntity;
            TargetMethodId = resolvedMethodId;

            PassedParameters = passedParameters;
            ReturnedValues = returnedValues;
        }

        public ResolvedInvocationStatement(
            ResolvedInvocationStatement<TNode> source, 
            Entity resolvedEntity, 
            ResolvedMethodId resolvedMethodId,
            Dictionary<SecondaryEntity, ParameterIndex> passedParameters,
            Dictionary<ParameterIndex, SecondaryEntity> returnedValues)
            : base(source.Location)
        {
            TargetEntity = resolvedEntity;
            TargetMethodId = resolvedMethodId;

            PassedParameters = passedParameters;
            ReturnedValues = returnedValues;
        }

        public override InternalStatementType InternalType => InternalStatementType.ResolvedInvocation;

        public override string ToString()
        {
            return $"invoke ?"; //{Target.EntryPoint}";
        }

        public override bool Equals(object obj)
        {
            return obj is ResolvedInvocationStatement<TNode> statement &&
                   base.Equals(obj) &&
                   TargetMethodId.Equals(statement.TargetMethodId) &&
                   TargetEntity.Equals(statement.TargetEntity);
        }

        public override int GetHashCode()
        {
            var hashCode = 1207886411;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + TargetMethodId.GetHashCode();
            hashCode = hashCode * -1521134295 + TargetEntity.GetHashCode();
            return hashCode;
        }
    }
}
