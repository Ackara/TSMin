namespace Acklann.TSBuild.CodeGeneration
{
	[System.Diagnostics.DebuggerDisplay("{" + nameof(Name) + "}")]
	public class MemberDefinition : DefinitionBase
	{
		public MemberDefinition() : base()
		{
		}

		public MemberDefinition(string name, TypeDefinition type, Trait trait = default, object defaultValue = default) : base()
		{
			Name = name;
			Type = type;
			Traits = trait;
			DefaultValue = defaultValue;

			if (Type != null && Type.IsArray) Type.IsArray = true;
		}

		public TypeDefinition Owner;

		public TypeDefinition Type { get; set; }

		public override bool IsCollection
		{
			get => base.IsCollection || Type.IsCollection;
		}

		public object DefaultValue { get; set; }

		public bool HasValue
		{
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
			get => DefaultValue != null && DefaultValue != string.Empty;
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
		}
	}
}
