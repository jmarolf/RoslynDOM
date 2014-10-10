using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using RoslynDom.Common;
 using System.ComponentModel.DataAnnotations;
 using System.ComponentModel.DataAnnotations;namespace RoslynDom
{
    public class RDomWhileStatement : RDomBaseLoop<IWhileStatement>, IWhileStatement
    {
        public RDomWhileStatement(SyntaxNode rawItem, IDom parent, SemanticModel model)
           : base(rawItem, parent, model)
        {
            this.TestAtEnd = true;
            Initialize();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
          "CA1811:AvoidUncalledPrivateCode", Justification = "Called via Reflection")]
        internal RDomWhileStatement(RDomWhileStatement oldRDom)
            : base(oldRDom)
        { }
    }
}
