﻿using Parser.ParseTree;
using System.Collections.Generic;

namespace Parser
{
    public class AnnotationParser
    {
        private ParserContext parser;
        public AnnotationParser(ParserContext parser)
        {
            this.parser = parser;
        }

        internal AnnotationCollection ParseAnnotations(TokenStream tokens)
        {
            AnnotationCollection annotationCollection = new AnnotationCollection(this.parser);
            while (tokens.IsNext("@"))
            {
                annotationCollection.Add(this.ParseAnnotation(tokens));
            }

            annotationCollection.Validate();

            return annotationCollection;
        }

        private Annotation ParseAnnotation(TokenStream tokens)
        {
            Token annotationToken = tokens.PopExpected("@");
            Token typeToken = tokens.Pop();

            // TODO: refactor this. All built-in annotations should be exempt from the VerifyIdentifier check in an extensible way.
            if (typeToken.Value != this.parser.Keywords.PRIVATE)
            {
                parser.VerifyIdentifier(typeToken);
            }

            List<Expression> args = new List<Expression>();
            if (tokens.PopIfPresent("("))
            {
                while (!tokens.PopIfPresent(")"))
                {
                    if (args.Count > 0)
                    {
                        tokens.PopExpected(",");
                    }

                    args.Add(this.parser.ExpressionParser.Parse(tokens, null));
                }
            }
            return new Annotation(annotationToken, typeToken, args);
        }
    }
}
