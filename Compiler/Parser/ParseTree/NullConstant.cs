﻿using System.Collections.Generic;

namespace Parser.ParseTree
{
    public class NullConstant : Expression, IConstantValue
    {
        internal override Expression PastelResolve(ParserContext parser)
        {
            return this;
        }

        public override bool IsInlineCandidate { get { return true; } }

        public override bool CanAssignTo { get { return false; } }

        public NullConstant(Token token, TopLevelConstruct owner)
            : base(token, owner)
        { }

        public override bool IsLiteral { get { return true; } }

        internal override Expression Resolve(ParserContext parser)
        {
            return this;
        }

        internal override Expression ResolveNames(ParserContext parser)
        {
            return this;
        }

        public Expression CloneValue(Token token, TopLevelConstruct owner)
        {
            return new NullConstant(token, owner);
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(ParserContext parser, VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
