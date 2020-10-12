using Acklann.Daterpillar.Attributes;
using Acklann.Daterpillar.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using Tecari.LLC.Gateways;

namespace Tecari.LLC
{
	public class Keeper
	{
		public string Name { get; set; }

		public int Age { get; set; }
	}

	public class Zoo
	{
		public string Name { get; set; }

		public string Address { get; set; }

		public Keeper[] Workers { get; set; }

        //public List<Keeper> ActiveWorkers { get; set; }
    }
}
