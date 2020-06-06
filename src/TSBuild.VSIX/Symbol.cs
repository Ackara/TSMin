using System;

namespace Acklann.TSBuild
{
	public static class Symbol
	{
		public const string Name = "TSBuild";

		public const string Version = "0.0.49";
		
		public struct Package
		{
			public const string GuidString = "648cd2ff-1416-4be6-bcb1-9e2f729a008d";
			public static readonly Guid Guid = new Guid("648cd2ff-1416-4be6-bcb1-9e2f729a008d");
		}
		public struct CmdSet
		{
			public const string GuidString = "979e0666-7021-4ff9-8026-8fda4bb84e0f";
			public static readonly Guid Guid = new Guid("979e0666-7021-4ff9-8026-8fda4bb84e0f");
			public const int FileCommandGroup = 0x0101;
			public const int MiscellaneousGroup = 0x0102;
			public const int MainMenu = 0x0200;
			public const int ConfigureCompileOnBuildCommandId = 0x0509;
			public const int GotoConfigurationPageCommandId = 0x0510;
		}
	}
}
