﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon.ParseTree
{
	internal class SwitchStatement : Executable
	{
		private Chunk[] chunks;

		public Chunk[] Chunks { get { return this.chunks; } }

		public bool UsesStrings { get; private set; }
		public bool UsesIntegers { get { return !this.UsesStrings; } }
		public bool ContainsDefault { get; private set; }
		public bool IsContinuous { get; private set; }
		public bool IsSafe { get; private set; }

		public class Chunk
		{
			public Chunk(int id, IList<Expression> cases, IList<Executable> code)
			{
				this.Cases = cases.ToArray();
				this.Code = code.ToArray();
				this.ID = id;
				this.ContainsFallthrough = this.Code.Length == 0 || !this.Code[this.Code.Length - 1].IsTerminator;
			}

			public Expression[] Cases { get; set; }
			public Executable[] Code { get; set; }
			public bool ContainsFallthrough { get; private set; }
			public int ID { get; private set; }
		}

		public Expression Condition { get; private set; }
		// for platforms that don't have switch statements and require creative use of if/else statements to mapped ID's, you can explicitly declare the maximum integer value that will
		// ever occur as input in the condition. This allows you to create a list large enough to accomodate all values. 
		// This is specifically used for the binary operator switch statement, which is a sparse lookup table defined as a list where most entries point to the default case. 
		// The explicit max fills this list large enough to accomodate the maximum value beyond the largest valid lookup ID and maps them to default.
		private Expression explicitMax;
		private Token explicitMaxToken;

		public SwitchStatement(Token switchToken, Expression condition, List<List<Expression>> cases, List<List<Executable>> code, Expression explicitMax, Token explicitMaxToken)
			: base(switchToken)
		{
			if (cases.Count == 0) throw new ParserException(switchToken, "Switch statement needs cases.");
			if (code.Count == 0) throw new ParserException(switchToken, "Switch statement needs code.");
			if (cases[0] == null) throw new ParserException(switchToken, "Switch statement must start with a case.");
			if (cases[cases.Count - 1] != null) throw new ParserException(switchToken, "Last case in switch statement is empty.");

			this.Condition = condition;
			this.explicitMax = explicitMax;
			this.explicitMaxToken = explicitMaxToken;

			List<Chunk> chunks = new List<Chunk>();
			int counter = 0;
			for (int i = 0; i < cases.Count; i += 2)
			{
				if (cases[i] == null) throw new Exception("This should not happen.");
				if (code[i + 1] == null) throw new Exception("This should not happen.");
				chunks.Add(new Chunk(counter++, cases[i], code[i + 1]));
			}
			this.chunks = chunks.ToArray();
		}

		// TODO: in the resolve be sure to make fallthroughs an error. 

		public override IList<Executable> Resolve(Parser parser)
		{
			if (parser.IsByteCodeMode && this.explicitMax != null)
			{
				throw new ParserException(this.explicitMaxToken, "Unexpected token: '{'");
			}

			bool useExplicitMax = this.explicitMax != null;
			int explicitMax = 0;
			if (useExplicitMax)
			{
				this.explicitMax = this.explicitMax.Resolve(parser);
				if (!(this.explicitMax is IntegerConstant)) throw new ParserException(this.explicitMax.FirstToken, "Explicit max must be an integer.");
				explicitMax = ((IntegerConstant)this.explicitMax).Value;
			}

			this.Condition = this.Condition.Resolve(parser);
			int integers = 0;
			int strings = 0;
			int largestSpan = 0;

			HashSet<int> intCases = new HashSet<int>();
			HashSet<string> stringCases = new HashSet<string>();

			foreach (Chunk chunk in this.chunks)
			{
				if (chunk.Cases.Length > largestSpan) largestSpan = chunk.Cases.Length;

				for (int i = 0; i < chunk.Cases.Length; ++i)
				{
					if (chunk.Cases[i] != null)
					{
						Expression caseExpr = chunk.Cases[i].Resolve(parser);
						chunk.Cases[i] = caseExpr;

						if (caseExpr is IntegerConstant)
						{
							int intValue = ((IntegerConstant)caseExpr).Value;
							if (intCases.Contains(intValue))
							{
								throw new ParserException(caseExpr.FirstToken, "Duplicate case value in same switch: " + intValue);
							}
							intCases.Add(intValue);
						}
						else if (caseExpr is StringConstant)
						{
							string strValue = ((StringConstant)caseExpr).Value;
							if (stringCases.Contains(strValue))
							{
								throw new ParserException(caseExpr.FirstToken, "Duplicate case value in same switch: " + Util.ConvertStringValueToCode(strValue));
							}
							stringCases.Add(strValue);
						}

						if (chunk.Cases[i] is IntegerConstant) integers++;
						else if (chunk.Cases[i] is StringConstant) strings++;
						else throw new ParserException(chunk.Cases[i].FirstToken, "Only strings and integers can be used in a switch statement.");
					}
				}

				chunk.Code = Resolve(parser, chunk.Code).ToArray();
			}

			if (integers != 0 && strings != 0)
			{
				throw new ParserException(this.FirstToken, "Cannot mix and match strings and integers in a switch statement.");
			}

			if (integers == 0 && strings == 0)
			{
				if (this.chunks.Length == 0) throw new ParserException(this.FirstToken, "Cannot have a blank switch statement.");
				if (this.chunks.Length > 1) throw new Exception("only had default but had multiple chunks. This should have been prevented at parse-time.");
				return this.chunks[0].Code;
			}

			Chunk lastChunk = this.chunks[this.chunks.Length - 1];
			Expression lastCase = lastChunk.Cases[lastChunk.Cases.Length - 1];

			this.ContainsDefault = lastCase == null;
			this.UsesStrings = strings != 0;
			this.IsSafe = !this.ContainsDefault;
			this.IsContinuous = integers != 0 && largestSpan == 1 && this.IsSafe && this.DetermineContinuousness();

			if (this.IsSafe & this.IsContinuous)
			{
				return new SwitchStatementContinuousSafe(this).Resolve(parser);
			}

			return new SwitchStatementUnsafeBlotchy(this, useExplicitMax, explicitMax).Resolve(parser);
		}

		private bool DetermineContinuousness()
		{
			HashSet<int> integersUsed = new HashSet<int>();
			bool first = true;
			int min = 0;
			int max = 0;
			foreach (Chunk chunk in this.chunks)
			{
				if (chunk.Cases.Length != 1) throw new Exception("This should have been filtered out by the largestSpane check earlier.");
				IntegerConstant c = chunk.Cases[0] as IntegerConstant;
				if (c == null) throw new Exception("How did this happen?");
				int num = c.Value;
				integersUsed.Add(num);
				if (first)
				{
					first = false;
					min = num;
					max = num;
				}
				else
				{
					if (num < min) min = num;
					if (num > max) max = num;
				}
			}

			int expectedTotal = max - min + 1;
			return expectedTotal == integersUsed.Count;
		}

		public override void GetAllVariableNames(Dictionary<string, bool> lookup)
		{
			foreach (Chunk chunk in this.chunks)
			{
				foreach (Executable line in chunk.Code)
				{
					line.GetAllVariableNames(lookup);
				}
			}
		}
	}
}