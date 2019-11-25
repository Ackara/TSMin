namespace Acklann.TSMin
{
    public class CompilerResult
    {
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
            get => Errors?.Length > 0;
        }
    }
}