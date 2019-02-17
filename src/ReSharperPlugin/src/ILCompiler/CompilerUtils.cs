using System.Collections.Generic;
using System.Linq;
using Cofra.AbstractIL.Common.Types;
using Cofra.AbstractIL.Common.Types.AnalysisSpecific;
using Cofra.AbstractIL.Common.Types.Ids;
using Cofra.AbstractIL.Common.Types.InvocationTargets;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace Cofra.ReSharperPlugin.ILCompiler
{
    
    public static class MethodIdExtension  
    {  
        public static string GetNameWithHash(this IDeclaredElement declaredElement)
        {
            //var containerName = method.GetContainingType()?.GetClrName();
            var name = declaredElement.ShortName;
            //var finalName = $"{containerName}.{name}";
            switch (declaredElement)
            {
                case IParametersOwner method:
                    var parameters = new List<string>();
                    //parameters.Add(method.ReturnType.ToString());
                    parameters.AddRange(method.Parameters.Select(x => x.Type.ToString()));
                    return $"({method.ReturnType}){name}({string.Join(",", parameters)})";
                case ITypeMember typeMember:
                    var hash = typeMember.CalcHash()?.Value ?? 0;
                    return $"{name}{{{hash}}}";
                default:
                    return $"{name}";
            }
        }
    }

    public static class CompilerUtils
    {
        //private static readonly CSharpCacheProvider myLanguageCacheProvider = CSharpLanguage.Instance.CacheProvider() as CSharpCacheProvider;

        public static MethodId GetMethodId(IDeclaredElement method)
        {
            return new MethodId(method.GetNameWithHash());
        }

        public static bool IsInvocation(IMethod method)
        {
            return false;
        }

        public static ClassReference GetClassReference(this ITypeElement typeElement)
        {
            return new ClassReference(new ClassId(typeElement.GetClrName().FullName));
        }

        public static bool NeedToSkipMethod(IMethod method)
        {
            return false;
        }

        public static bool ReferenceIsAllowedToInvoke(Reference reference)
        {
            return reference is ClassFieldReference ||
                   reference is ClassMethodReference ||
                   reference is ClassPropertyReference ||
                   reference is LocalFunctionReference ||
                   reference is LocalVariableReference;
        }

        public static bool IsAddToCollectionMethod(ClassMethodTarget classMethod)
        {
//            if (classMethod.TargetMethod != "Add") return false;
//            
//            if (classMethod.TargetClass)
            
            return true;
        }
}
}