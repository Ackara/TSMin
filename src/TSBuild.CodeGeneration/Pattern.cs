using System.Text.RegularExpressions;

namespace Acklann.TSBuild.CodeGeneration
{
	internal class Pattern
	{
		public static readonly Regex EnumerableType = new Regex(@"(List|Collection)(`\d)?<(?<type>[^><]+)>", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
	}
}
