﻿using System;
using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class NegativeSign : Expression
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            this.Root = this.Root.PastelResolve(parser);
            return this;
        }

        public override bool CanAssignTo { get { return false; } }

        public Expression Root { get; private set; }

        public NegativeSign(Token sign, Expression root, TopLevelConstruct owner)
            : base(sign, owner)
        {
            this.Root = root;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            this.Root = this.Root.Resolve(parser);
            if (this.Root is IntegerConstant)
            {
                return new IntegerConstant(this.FirstToken, ((IntegerConstant)this.Root).Value * -1, this.Owner);
            }

            if (this.Root is FloatConstant)
            {
                return new FloatConstant(this.FirstToken, ((FloatConstant)this.Root).Value * -1, this.Owner);
            }

            return this;
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase)
        {
            this.Root.PerformLocalIdAllocation(parser, varIds, phase);
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            this.Root = this.Root.ResolveNames(parser);
            return this;
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars)
        {
            this.Root.GetAllVariablesReferenced(vars);
        }
    }
}
