namespace Acklann.TSBuild
{
    public readonly struct CompilerResult
    {
        public CompilerResult(bool success, CompilerError[] errors, string[] srcFiles, string[] generatedFiles, System.TimeSpan elapse)
        {
            Success = success;
            SourceFiles = srcFiles;
            Elapse = elapse;
            Errors = errors;
            GeneratedFiles = generatedFiles;
        }

        public bool Success { get; }

        public string[] SourceFiles { get; }

        public System.TimeSpan Elapse { get; }

        public CompilerError[] Errors { get; }

        public string[] GeneratedFiles { get; }

        public string OutputFile
        {
            get
            {
                if (GeneratedFiles?.Length > 0)
                    return GeneratedFiles[GeneratedFiles.Length - 1];
                else
                    return null;
            }
        }

        public bool HasErrors
        {
            get
            {
                for (int i = 0; i < Errors.Length; i++)
                    if (Errors[i].Severity == ErrorSeverity.Error)
                        return true;

                return false;
            }
        }
    }
}