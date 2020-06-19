namespace Acklann.TSBuild.CodeGeneration
{
	public readonly struct TypescriptGeneratorSettings
	{
		public TypescriptGeneratorSettings(string ns, string prefix = default, string suffix = default, bool useAbstract = default, bool koJs = default, params string[] references)
		{
			Namespace = ns;
			Prefix = prefix;
			Suffix = suffix;
			UseKnockoutJs = koJs;
			UseAbstract = useAbstract;
			References = references;
		}

		public readonly string Namespace;

		public readonly string Prefix;

		public readonly string Suffix;

		public readonly bool UseKnockoutJs;

		public readonly bool UseAbstract;

		public readonly string[] References;

		public bool HasNamespace
		{
			get => !string.IsNullOrEmpty(Namespace);
		}
	}
}
