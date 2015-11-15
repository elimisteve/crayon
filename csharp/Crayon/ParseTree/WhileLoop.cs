﻿using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class WhileLoop : Executable
	{
		public Expression Condition { get; private set; }
		public Executable[] Code { get; private set; }

		public WhileLoop(Token whileToken, Expression condition, IList<Executable> code)
			: base(whileToken)
		{
			this.Condition = condition;
			this.Code = code.ToArray();
		}

		public override IList<Executable> Resolve(Parser parser)
		{
			this.Condition = this.Condition.Resolve(parser);
			this.Code = Resolve(parser, this.Code).ToArray();
			return Listify(this);
		}

		public override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			foreach (Executable line in this.Code)
			{
				line.GetAllVariableNames(lookup);
			}
		}

		public override void AssignVariablesToIds(VariableIdAllocator varIds)
		{
			foreach (Executable ex in this.Code)
			{
				ex.AssignVariablesToIds(varIds);
			}
		}

		public override void VariableUsagePass(Parser parser)
		{
			this.Condition.VariableUsagePass(parser);
			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableUsagePass(parser);
			}
		}

		public override void VariableIdAssignmentPass(Parser parser)
		{
			this.Condition.VariableIdAssignmentPass(parser);
			for (int i = 0; i < this.Code.Length; ++i)
			{
				this.Code[i].VariableIdAssignmentPass(parser);
			}
		}
	}
}
