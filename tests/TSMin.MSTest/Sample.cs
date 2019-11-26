using System;
using System.IO;
using System.Linq;

namespace Acklann.TSMin
{
	internal static partial class Sample
	{
		public const string FOLDER_NAME = "samples";

		public static string DirectoryName => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME);
		
		public static FileInfo GetFile(string fileName, string directory = null)
        {
            fileName = Path.GetFileName(fileName);
            string searchPattern = $"*{Path.GetExtension(fileName)}";

            string targetDirectory = directory?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME);
            return new DirectoryInfo(targetDirectory).EnumerateFiles(searchPattern, SearchOption.AllDirectories)
                .First(x => x.Name.Equals(fileName, StringComparison.CurrentCultureIgnoreCase));
        }

		public static FileInfo GetError5TS() => GetFile(@"error-5.ts");
		public static FileInfo GetIndexHTML() => GetFile(@"index.html");
		public static FileInfo GetProjXML() => GetFile(@"proj.xml");
		public static FileInfo GetServerTS() => GetFile(@"domain\Server.ts");
		public static FileInfo GetToastTS() => GetFile(@"domain\Toast.ts");
		public static FileInfo GetCarTS() => GetFile(@"Modles\Car.ts");
		public static FileInfo GetObservableTS() => GetFile(@"Modles\Observable.ts");

		public struct File
		{
			public const string Error5TS = @"error-5.ts";
			public const string IndexHTML = @"index.html";
			public const string ProjXML = @"proj.xml";
			public const string ServerTS = @"domain\Server.ts";
			public const string ToastTS = @"domain\Toast.ts";
			public const string CarTS = @"Modles\Car.ts";
			public const string ObservableTS = @"Modles\Observable.ts";
		}
	}	
}
