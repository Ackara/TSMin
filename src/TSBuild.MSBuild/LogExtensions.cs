using Microsoft.Build.Framework;

namespace Acklann.TSBuild.MSBuild
{
	public static class LogExtensions
	{
		public static void Debug(this IBuildEngine engine, string message, string sender = default)
		{
			engine.LogMessageEvent(new BuildMessageEventArgs(message, null, (sender ?? nameof(TSBuild)), MessageImportance.Low));
		}

		public static void Info(this IBuildEngine engine, string message, string sender = default)
		{
			engine.LogMessageEvent(new BuildMessageEventArgs(message, null, (sender ?? nameof(TSBuild)), MessageImportance.Normal));
		}

		public static void Warn(this IBuildEngine engine, string message, string sender = default)
		{
			engine.LogWarningEvent(new BuildWarningEventArgs(message, null, null, 0, 0, 0, 0, message, null, sender));
		}
	}
}
