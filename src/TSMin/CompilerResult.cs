namespace Acklann.TSMin
{
    public class CompilerResult
    {
        public CompilerResult()
        {
        }

        public CompilerResult(bool success, System.TimeSpan elapse, CompilerError[] errors, string[] srcFiles, string[] generatedFiles)
        {
            Success = success;
            SourceFiles = srcFiles;
            Elapse = elapse;
            Errors = errors;
            GeneratedFiles = generatedFiles;
        }

        public bool Success { get; set; }

        public string[] SourceFiles { get; set; }

        public System.TimeSpan Elapse { get; set; }

        public CompilerError[] Errors { get; set; }

        public string[] GeneratedFiles { get; set; }

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