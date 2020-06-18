namespace Acklann.TSBuild.CodeGeneration
{
	[System.Diagnostics.DebuggerDisplay("{" + nameof(Name) + "}")]
	public class MemberDeclaration : DeclarationBase
	{
		public MemberDeclaration() : base()
		{
		}

		public MemberDeclaration(string name, TypeDeclaration type) : base()
		{
			Name = name;
			Type = type;
		}

		public TypeDeclaration Owner;

		public TypeDeclaration Type { get; set; }

		public object DefaultValue { get; set; }

		public bool HasValue
		{
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
			get => DefaultValue != null && DefaultValue != string.Empty;
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
		}
	}
}
