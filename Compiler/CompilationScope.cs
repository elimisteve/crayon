﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crayon.ParseTree;

namespace Crayon
{
    public class CompilationScope
    {
        public Library Library { get; private set; }

        public Locale Locale
        {
            get { return this.Library == null ? this.buildContext.CompilerLocale : this.Library.Metadata.InternalLocale; }
        }

        private List<Executable> executables = new List<Executable>();

        public List<Executable> GetExecutables_HACK()
        {
            return this.executables;
        }

        public void AddExecutable(Executable executable, string[] importsNamespaceSearch)
        {
            executable.NamespacePrefixSearch = importsNamespaceSearch;

            if (executable is Namespace)
            {
                ((Namespace)executable).GetFlattenedCode(this.executables, importsNamespaceSearch);
            }
            else
            {
                this.executables.Add(executable);
            }
        }

        private BuildContext buildContext;

        public CompilationScope(BuildContext buildContext, Library library)
        {
            this.buildContext = buildContext;
            this.Library = library;
        }
    }
}
