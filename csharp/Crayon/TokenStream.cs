﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
	internal class TokenStream
	{
		private int index;
		private int length;
		private Token[] tokens;

		public TokenStream(IList<Token> tokens)
		{
			this.index = 0;
			this.tokens = tokens.ToArray();
			this.length = tokens.Count;
		}

		public bool IsNext(string token)
		{
			return this.PeekValue() == token;
		}

		public Token Peek()
		{
			if (this.index < this.length)
			{
				return this.tokens[this.index];
			}

			throw new EofException();
		}

		public Token Pop()
		{
			if (this.index < this.length)
			{
				return this.tokens[this.index++];
			}

			throw new EofException();
		}

		public string PeekValue()
		{
			return this.Peek().Value;
		}

		public string PeekValue(int skipAhead)
		{
			int index = this.index + skipAhead;
			if (index < this.length)
			{
				return this.tokens[index].Value;
			}
			return null;
		}

		public string PopValue()
		{
			return this.Pop().Value;
		}

		public bool PopIfPresent(string value)
		{
			if (this.index < this.length && this.tokens[this.index].Value == value)
			{
				this.index += 1;
				return true;
			}
			return false;
		}

		public Token PopExpected(string value)
		{
			Token token = this.Pop();
			if (token.Value != value)
			{
				throw new Exception("Line: " + (token.Line + 1) + ", Col: " + (token.Col + 1) + ", Unexpected token. Expected: '" + value + "' but found '" + token.Value + "'");
			}
			return token;
		}

		public bool HasMore
		{
			get { return this.index < this.length; }
		}

		public bool NextHasNoWhitespacePrefix
		{
			get
			{
				if (this.index < this.length)
				{
					Token token = this.tokens[this.index];
					return token.HasWhitespacePrefix;
				}
				return false;
			}
		}
	}
}