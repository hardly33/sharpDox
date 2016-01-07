﻿using System;
using System.Collections.Generic;
using SharpDox.Build.Roslyn.Parser;
using SharpDox.Model;
using SharpDox.Model.Repository;
using SharpDox.Sdk.Config;

namespace SharpDox.Build.Roslyn
{
    public class RoslynParser : ICodeParser
    {
        public event Action<string> OnDocLanguageFound;
        public event Action<string> OnStepMessage;
        public event Action<int> OnStepProgress;

        private readonly ParserStrings _parserStrings;
        private readonly RoslynLoader _roslynLoader;

        public RoslynParser(ParserStrings parserStrings)
        {
            _parserStrings = parserStrings;
            _roslynLoader = new RoslynLoader();
        }

        public SDSolution GetFullParsedSolution(string solutionFile, ICoreConfigSection sharpDoxConfig, Dictionary<string, string> tokens)
        {
            var solution = _roslynLoader.LoadSolutionFile(solutionFile);
            foreach (var project in solution.Projects)
            {
                ExecuteOnStepMessage(string.Format(_parserStrings.Compiling, project.Name));
                var projectCompilation = project.GetCompilationAsync().Result;

                var nparser = new NamespaceParser(sharpDoxConfig, solutionFile, tokens);
                var sdRepository = nparser.ParseProjectNamespaces(projectCompilation, new SDRepository());
            }
            return new SDSolution(solutionFile);
        }

        public SDSolution GetStructureParsedSolution(string solutionFile)
        {
            _roslynLoader.LoadSolutionFile(solutionFile);
            return null;
            //http://www.codeproject.com/Articles/861548/Roslyn-Code-Analysis-in-Easy-Samples-Part
            //https://joshvarty.wordpress.com/2014/10/30/learn-roslyn-now-part-7-introducing-the-semantic-model/
        }

        private void ExecuteOnDocLanguageFound(string lang)
        {
            var handlers = OnDocLanguageFound;
            if (handlers != null)
            {
                handlers(lang);
            }
        }

        private void ExecuteOnStepMessage(string message)
        {
            var handlers = OnStepMessage;
            if (handlers != null)
            {
                handlers(message);
            }
        }

        private void ExecuteOnStepProgress(int progress)
        {
            var handlers = OnStepProgress;
            if (handlers != null)
            {
                handlers(progress);
            }
        }
    }
}