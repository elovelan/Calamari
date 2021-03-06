﻿using System;
using System.Text;
using Calamari.Deployment;
using Calamari.Integration.FileSystem;
using Octostache;

namespace Calamari.Integration.Substitutions
{
    public class FileSubstituter : IFileSubstituter
    {
        readonly ICalamariFileSystem fileSystem;

        public FileSubstituter(ICalamariFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void PerformSubstitution(string sourceFile, VariableDictionary variables)
        {
            PerformSubstitution(sourceFile, variables, sourceFile);
        }

        public void PerformSubstitution(string sourceFile, VariableDictionary variables, string targetFile)
        {
            Encoding sourceFileEncoding;
            var source = fileSystem.ReadFile(sourceFile, out sourceFileEncoding);
            var encoding = GetEncoding(variables, sourceFileEncoding);

            string error;
            var result = variables.Evaluate(source, out error);

            if (!string.IsNullOrEmpty(error))
                Log.VerboseFormat("Parsing file '{0}' with Octostache returned the following error: `{1}`", sourceFile, error);

            fileSystem.OverwriteFile(targetFile, result, encoding);
        }

        private Encoding GetEncoding(VariableDictionary variables, Encoding fileEncoding)
        {
            var requestedEncoding = variables.Get(SpecialVariables.Package.SubstituteInFilesOutputEncoding);
            if (requestedEncoding == null)
            {
                return fileEncoding;
            }

            try
            {
                return Encoding.GetEncoding(requestedEncoding);
            }
            catch (ArgumentException)
            {
                return fileEncoding;
            }
            
        }
    }
}
