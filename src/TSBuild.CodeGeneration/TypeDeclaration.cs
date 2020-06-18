using System.Collections.Generic;

namespace Acklann.TSBuild.CodeGeneration
{
	[System.Diagnostics.DebuggerDisplay("{" + nameof(FullName) + "(),nq}")]
	public partial class TypeDeclaration : DeclarationBase
	{
		public TypeDeclaration() : this(null)
		{
		}

		

		public TypeDeclaration(string name, Trait traits = Trait.None)
		{
			Name = name;
			Traits = traits;
			ArrayRankSpecifiers = 0;
			BaseList = new List<TypeDeclaration>();
			Members = new List<MemberDeclaration>();
			ParameterList = new List<TypeDeclaration>();
		}

		public string Namespace { get; set; }

		public int ArrayRankSpecifiers { get; set; }

		public string FullName
		{
			get => string.Concat(
				Namespace,
				(string.IsNullOrEmpty(Namespace) ? '\0' : '.'),
				Name).Trim();
		}

		public bool HasBaseType
		{
			get => BaseType != null;
		}

		public TypeDeclaration BaseType
		{
			get
			{
				if (BaseList?.Count > 0 && !BaseList[0].IsInterface)
					return BaseList[0];

				return null;
			}
		}

		public List<TypeDeclaration> BaseList { get; set; }

		public List<TypeDeclaration> ParameterList { get; set; }

		public List<MemberDeclaration> Members { get; set; }

		public TypeDeclaration Add(MemberDeclaration member)
		{
			member.Owner = this;
			Members.Add(member);
			if (IsEnum) member.Traits |= Trait.Enum | Trait.Public;

			return this;
		}

		public IEnumerable<TypeDeclaration> EnumerateBaseList()
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

		public static IEnumerable<TypeDeclaration> ResolveDependencies(TypeDeclaration[] declarations)
		{
			foreach (TypeDeclaration item in declarations)
			{
				item.InScope = true;
				FindReferences(item, declarations);
			}

			return new Enumerable(declarations);
		}

		private static void FindReferences(TypeDeclaration type, TypeDeclaration[] declarations)
		{
			TypeDeclaration item;
			foreach (TypeDeclaration reference in declarations)
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
