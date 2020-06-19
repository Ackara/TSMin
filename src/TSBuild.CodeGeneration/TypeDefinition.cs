using System.Collections.Generic;

namespace Acklann.TSBuild.CodeGeneration
{
	[System.Diagnostics.DebuggerDisplay("{" + nameof(FullName) + ",nq}")]
	public partial class TypeDefinition : DeclarationBase
	{
		public TypeDefinition() : this(null)
		{
		}

		public TypeDefinition(string name, Trait traits = Trait.None)
		{
			Name = name;
			Traits = traits;
			ArrayRankSpecifiers = 0;
			BaseList = new List<TypeDefinition>();
			Members = new List<MemberDeclaration>();
			ParameterList = new List<TypeDefinition>();
		}

		public string Namespace { get; set; }

		public int ArrayRankSpecifiers { get; set; }

		public string FullName
		{
			get => string.Concat(
				Namespace,
				'.',
				Name).Trim('.', ' ', '\r', '\n');
		}

		public bool HasBaseType
		{
			get => BaseType != null;
		}

		public bool HasParameters
		{
			get => (ParameterList != null && ParameterList.Count > 0);
		}

		public TypeDefinition BaseType
		{
			get
			{
				if (BaseList?.Count > 0 && !BaseList[0].IsInterface)
					return BaseList[0];

				return null;
			}
		}

		public List<TypeDefinition> BaseList { get; set; }

		public List<TypeDefinition> ParameterList { get; set; }

		public List<MemberDeclaration> Members { get; set; }

		public TypeDefinition Add(MemberDeclaration member)
		{
			member.Owner = this;
			Members.Add(member);
			if (IsEnum) member.Traits |= Trait.Enum | Trait.Public;

			return this;
		}

		public TypeDefinition Add(TypeDefinition definition)
		{
			ParameterList.Add(definition);
			return this;
		}

		public TypeDefinition Inherit(TypeDefinition definition)
		{
			BaseList.Add(definition);
			return this;
		}

		public IEnumerable<TypeDefinition> EnumerateBaseList()
		{
			if (BaseList == null || BaseList.Count < 1) yield break;

			for (int i = 0; i < BaseList.Count; i++)
				if (!BaseList[i].IsInterface)
				{
					yield return BaseList[i];
				}

			for (int i = 0; i < BaseList.Count; i++)
				if (BaseList[i].IsInterface)
				{
					yield return BaseList[i];
				}
		}

		public static IEnumerable<TypeDefinition> ResolveDependencies(TypeDefinition[] definitions)
		{
			foreach (TypeDefinition def in definitions)
			{
				def.InScope = true;
				FindReferences(def, definitions);
			}

			return new Enumerable(definitions);
		}

		private static void FindReferences(TypeDefinition type, TypeDefinition[] definitions)
		{
			TypeDefinition item;
			foreach (TypeDefinition reference in definitions)
			{
				if (type.BaseList?.Count > 0)
					for (int i = 0; i < type.BaseList.Count; i++)
					{
						item = type.BaseList[i];
						if (item.FullName == reference.FullName || item.Name == reference.Name)
							type.BaseList[i] = reference;
					}

				if (type.ParameterList?.Count > 0)
					for (int i = 0; i < type.BaseList.Count; i++)
					{
						item = type.ParameterList[i];
						if (item.FullName == reference.FullName || item.Name == reference.Name)
							type.ParameterList[i] = reference;
					}

				if (!type.IsEnum)
					foreach (MemberDeclaration member in type.Members)
					{
						if (member.Type.FullName == reference.FullName || member.Type.Name == reference.Name)
							member.Type = reference;
					}
			}
		}
	}
}
