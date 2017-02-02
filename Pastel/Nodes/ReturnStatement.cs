﻿using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class ReturnStatement : Executable
    {
        public Expression Expression { get; set; }

        public ReturnStatement(Token returnToken, Expression expression) : base(returnToken)
        {
            this.Expression = expression;
        }

        public override IList<Executable> ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            if (this.Expression != null)
            {
                this.Expression = this.Expression.ResolveNamesAndCullUnusedCode(compiler);
            }
            return Listify(this);
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            if (this.Expression != null)
            {
                this.Expression.ResolveType(varScope, compiler);
                if (!PType.CheckReturnType(varScope.RootFunctionDefinition.ReturnType, this.Expression.ResolvedType))
                {
                    throw new ParserException(this.Expression.FirstToken, "This expression is not the expected return type of this function.");
                }
            }
            else
            {
                if (!this.Expression.ResolvedType.IsIdentical(PType.VOID))
                {
                    throw new ParserException(this.FirstToken, "Must return a value in this function.");
                }
            }
        }
    }
}
