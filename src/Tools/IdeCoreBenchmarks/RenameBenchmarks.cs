// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;

namespace IdeCoreBenchmarks
{
    [MemoryDiagnoser]
    public class RenameBenchmarks
    {

        //private Solution _solution;
        //private ISymbol _symbol;
        //private readonly OptionSet _options;
        private string _fileText;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var roslynRoot = Environment.GetEnvironmentVariable(Program.RoslynRootPathEnvVariableName);
            var csFilePath = Path.Combine(roslynRoot, @"src\Compilers\CSharp\Portable\Generated\BoundNodes.xml.Generated.cs");

            if (!File.Exists(csFilePath))
            {
                throw new ArgumentException();
            }

            _fileText = File.ReadAllText(csFilePath);
        }

        [Benchmark]
        public void RenameNodes()
        {
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);

            var solution = new AdhocWorkspace().CurrentSolution
                .AddProject(projectId, "ProjectName", "AssemblyName", LanguageNames.CSharp)
                .AddDocument(documentId, "DocumentName", _fileText);

            var project = solution.Projects.First();
            var compilation = project.GetCompilationAsync().Result;
            var symbol = compilation.GetTypeByMetadataName("Microsoft.CodeAnalysis.CSharp.BoundKind");
            _ = Microsoft.CodeAnalysis.Rename.Renamer.RenameSymbolAsync(solution, symbol, "NewName", null);
        }

        /*[IterationCleanup]
        public void Cleanup()
        {
            _solution = null;
            _symbol = null;
        }*/
    }
}
