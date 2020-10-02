using System;
using System.Collections.Generic;
using System.Linq;

namespace Acklann.TSBuild.Fake
{
	public interface IJobInfo : ICloneable
	{
		string Id { get; }
		string Name { get; }
	}
}
