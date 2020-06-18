using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Acklann.Diffa.ApprovedFolder("approved-result")]
[assembly: Acklann.Diffa.Reporters.Reporter(typeof(Acklann.Diffa.Reporters.DiffReporter))]

namespace Tees.MSTest
{
	[TestClass]
	public class Startup
	{
		//[AssemblyInitialize]
		public static void Setup(TestContext _)
		{
		}
	}
}
