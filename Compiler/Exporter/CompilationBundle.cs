﻿using Build;
using Common;
using Parser;
using System.Collections.Generic;

namespace Exporter
{
    public class CompilationBundle
    {
        public ByteBuffer ByteCode { get; set; }
        public string ProjectID { get; set; }
        public string GuidSeed { get; set; }
        public ICollection<LibraryCompilationScope> LibraryScopesUsed { get; set; }
        public UserCodeCompilationScope UserCodeScope { get; set; }
        public string IconPath { get; set; }
        public string LaunchScreenPath { get; set; }
        public string DefaultTitle { get; set; }
        public string JsFilePrefix { get; set; }
        public bool JsFullPage { get; set; }
        public string IosBundlePrefix { get; set; }
        public string JavaPackage { get; set; }
        public int? WindowWidth { get; set; }
        public int? WindowHeight { get; set; }
        public string Orientations { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public static CompilationBundle Compile(BuildContext buildContext)
        {
            using (new PerformanceSection("CompilationBundle.Compile"))
            {
                ParserContext parserContext = new ParserContext(buildContext);
                Parser.ParseTree.TopLevelConstruct[] resolvedParseTree = parserContext.ParseAllTheThings();

                ByteCodeCompiler bcc = new ByteCodeCompiler();
                ByteBuffer buffer = bcc.GenerateByteCode(parserContext, resolvedParseTree);

                string jsFilePrefix = buildContext.JsFilePrefix == null
                    ? ""
                    : ("/" + buildContext.JsFilePrefix.Trim('/') + "/");

                return new CompilationBundle()
                {
                    ByteCode = buffer,
                    UserCodeScope = parserContext.UserCodeCompilationScope,
                    LibraryScopesUsed = parserContext.LibraryManager.ImportedLibraries.ToArray(),
                    ProjectID = buildContext.ProjectID,
                    Version = buildContext.Version,
                    Description = buildContext.Description,
                    GuidSeed = buildContext.GuidSeed,
                    DefaultTitle = buildContext.DefaultTitle,
                    JsFilePrefix = jsFilePrefix,
                    JsFullPage = buildContext.JsFullPage,
                    IosBundlePrefix = buildContext.IosBundlePrefix,
                    JavaPackage = buildContext.JavaPackage,
                    IconPath = buildContext.IconFilePath,
                    LaunchScreenPath = buildContext.LaunchScreenPath,
                    WindowWidth = buildContext.WindowWidth,
                    WindowHeight = buildContext.WindowHeight,
                    Orientations = buildContext.Orientation,
                };
            }
        }
    }
}
