using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: Acklann.Diffa.ApprovedFolder("approved-result")]
[assembly: Acklann.Diffa.Reporters.Reporter(typeof(Acklann.Diffa.Reporters.DiffReporter))]

namespace Tees.MSTest
{
    [TestClass]
    public class Startup
    {

    }
}
