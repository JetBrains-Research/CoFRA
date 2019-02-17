using System;
using System.Linq;
using Cofra.ReSharperPlugin.ILCompiler.CompilationResults;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;

namespace Cofra.ReSharperPlugin.ILCompiler.ElementCompilers
{
    internal class PropertyDeclarationCompiler : ElementCompiler
    {
        private IPropertyDeclaration myPropertyDeclaration;
    
        public PropertyDeclarationCompiler(IPropertyDeclaration propertyDeclaration, AbstractILCompilerParams myParams) : base(myParams)
        {
            myPropertyDeclaration = propertyDeclaration;
        }

        public override ICompilationResult GetResult()
        {
            if (myPropertyDeclaration.DeclaredElement == null)
                throw MyParams.CreateException($"declaredElement of property is null");
            
//            var accesors = MyResults.Where(result => result is AccessorDeclarationCompilationResult).Cast<AccessorDeclarationCompilationResult>().ToList();
//            if (!accesors.IsEmpty())
//            {
//                if (!accesors.Exists(x => x.KindOfAccesor == AccessorKind.GETTER))
//                {
//                    MyParams.CreateFakeProperty($"get_{myPropertyDeclaration.DeclaredElement.ShortName}");
//                }
//                if (!accesors.Exists(x => x.KindOfAccesor == AccessorKind.SETTER))
//                {
//                    MyParams.CreateFakeProperty($"set_{myPropertyDeclaration.DeclaredElement.ShortName}");
//                }
//            }
            return new ElementCompilationResult();
        }
    }
}