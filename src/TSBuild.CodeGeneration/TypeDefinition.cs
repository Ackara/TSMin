using System.Collections.Generic;
using System.Linq;

namespace Acklann.TSBuild.CodeGeneration
{
	[System.Diagnostics.DebuggerDisplay("{" + nameof(FullName) + ",nq}")]
	public partial class TypeDefinition : DefinitionBase
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
			Members = new List<MemberDefinition>();
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

		public bool HasInScopeBaseType
		{
			get => BaseType != null && BaseType.InScope;
		}

		public bool InheritsInScopeDefinition
		{
			get => (BaseList?.Count(x => x.InScope) ?? 0) > 0;
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

		public List<MemberDefinition> Members { get; set; }

		public TypeDefinition Add(MemberDefinition member)
		{
			member.Owner = this;
			Members.Add(member);
			if (IsEnum) member.Traits |= Trait.Enum | Trait.Public;
			if (IsInterface) member.Traits |= Trait.Public;

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

		public IEnumerable<MemberDefinition> GetPublicFieldsAndProperties()
		{
			return from x in Members
				   where x.IsPublic && !x.IsConstant && !x.IsStatic
				   select x;
		}

		public IEnumerable<TypeDefinition> EnumerateInScopeBaseTypes()
		{
			if (BaseList == null || BaseList.Count < 1) yield break;

			foreach (TypeDefinition def in BaseList)
				if (!def.IsInterface && def.InScope)
				{
					yield return def;
				}

			foreach (TypeDefinition def in BaseList)
				if (def.IsInterface && def.InScope)
				{
					yield return def;
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
					foreach (MemberDefinition member in type.Members)
					{
						if (member.Type.FullName == reference.FullName || member.Type.Name == reference.Name)
							member.Type = reference;
					}
			}
		}
	}
}
