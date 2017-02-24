﻿using System;
using System.Collections.Generic;

namespace Pastel.Nodes
{
    class BreakStatement : Executable
    {
        public BreakStatement(Token breakToken) : base(breakToken)
        { }

        public override IList<Executable> ResolveNamesAndCullUnusedCode(PastelCompiler compiler)
        {
            return this.Listify(this);
        }

        internal override void ResolveTypes(VariableScope varScope, PastelCompiler compiler)
        {
            // nothing to do
        }

        internal override Executable ResolveWithTypeContext(PastelCompiler compiler)
        {
            return this;
        }
    }
}
