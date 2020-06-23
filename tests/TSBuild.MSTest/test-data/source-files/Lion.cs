using System;

namespace Acklann.TSBuild.testdata
{
	public interface IAnimal
	{
		string Name { get; set; }

		int Legs { get; set; }

		decimal MarketPrice { get; set; }

		bool Extinct { get; set; }

		DateTime DiscoveryDate { get; set; }
	}

	public class Lion : IAnimal
	{
		public string Name { get; set; }
		public int Legs { get; set; }
		public decimal MarketPrice { get; set; }
		public bool Extinct { get; set; }
		public DateTime DiscoveryDate { get; set; }
	}
}
