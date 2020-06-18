using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.TSBuild.CodeGeneration
{
	public class CSharpAdapter : CSharpSyntaxWalker
	{
		public CSharpAdapter() : this(new TypeDeclaration())
		{
		}

		public CSharpAdapter(TypeDeclaration definition)
		{
			_definition = definition;
		}

		public TypeDeclaration Definition
		{
			get => _definition;
		}

		public static IEnumerable<TypeDeclaration> ReadFile(string filePath)
		{
			if (!File.Exists(filePath)) throw new FileNotFoundException($"Could not find file at '{filePath}'.");

			SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), path: filePath);
			if (tree.TryGetRoot(out SyntaxNode node))
			{
				var declarations = from t in node.DescendantNodes()
								   where t.IsKind(SyntaxKind.ClassDeclaration) || t.IsKind(SyntaxKind.EnumDeclaration) || t.IsKind(SyntaxKind.StructDeclaration)
								   select t;

				foreach (SyntaxNode item in declarations)
				{
					var adapter = new CSharpAdapter(new TypeDeclaration());
					adapter.Visit(node);
					yield return adapter._definition;
				}
			}
		}

		public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
		{
			_definition.Namespace = node.Name.ToFullString().Trim();
			base.VisitNamespaceDeclaration(node);
		}

		public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
		{
			_definition.Traits |= (Helper.GetTraits(node.Modifiers) | Trait.Enum);
			_definition.Name = node.Identifier.ValueText;

			base.VisitEnumDeclaration(node);
		}

		public override void VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			_definition.Traits |= node.Modifiers.GetTraits();
			_definition.Name = node.Identifier.ValueText;
			SetValues(_definition, node.TypeParameterList);
			SetValues(_definition, node.BaseList);

			base.VisitClassDeclaration(node);
		}

		public override void VisitStructDeclaration(StructDeclarationSyntax node)
		{
			_definition.Traits |= (node.Modifiers.GetTraits() | Trait.Class);
			_definition.Name = node.Identifier.ValueText;
			GetBaseTypes(node);

			base.VisitStructDeclaration(node);
		}

		public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
		{
			_definition.Traits |= (node.Modifiers.GetTraits() | Trait.Interface);
			_definition.Name = node.Identifier.ValueText;
			GetBaseTypes(node);

			base.VisitInterfaceDeclaration(node);
		}

		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
		{
			var property = new MemberDeclaration(node.Identifier.ValueText.Trim(), new TypeDeclaration());
			property.Traits |= node.Modifiers.GetTraits();
			if (node.ChildNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.ExplicitInterfaceSpecifier)) != null) property.Traits |= Trait.Interface;

			var unwantedNodes = from x in node.ChildNodes()
								where x.IsKind(SyntaxKind.AccessorList) || x.IsKind(SyntaxKind.AccessorList)
								select x;
			SetValues(property.Type, node.RemoveNodes(unwantedNodes, SyntaxRemoveOptions.KeepNoTrivia));
			_definition.Add(property);
		}

		public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
		{
			var type = new TypeDeclaration();

			var unwantedNodes = from x in node.ChildNodes()
								where x.IsKind(SyntaxKind.AttributeList)
								select x;
			SetValues(type, node.Declaration);

			var variables = from x in node.DescendantNodes()
							where x.IsKind(SyntaxKind.VariableDeclarator)
							select (VariableDeclaratorSyntax)x;
			Trait modifiers = node.Modifiers.GetTraits();

			foreach (var item in variables)
				_definition.Add(new MemberDeclaration(item.Identifier.ValueText.Trim(), type) { Traits = modifiers });
		}

		public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
		{
			var member = new MemberDeclaration(node.Identifier.ValueText, new TypeDeclaration() { Namespace = nameof(System), Name = nameof(Int32), Traits = Trait.Primitive | Trait.Enum });
			object variable = node.EqualsValue?.Value.ToFullString().Trim();
			bool isInt = int.TryParse((string)variable, out int constant);

			member.Traits = (Trait.Public | Trait.Enum);
			member.DefaultValue = (isInt ? constant : variable);

			_definition.Add(member);

			base.VisitEnumMemberDeclaration(node);
		}

		private static void SetValues(TypeDeclaration type, CSharpSyntaxNode node)
		{
			SyntaxNode temp = node.ChildNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.QualifiedName));
			if (temp == null) temp = node;
			if (temp is QualifiedNameSyntax q)
			{
				type.Namespace = q.Left.ToFullString().Trim();
				SetValues(type, q.Right);
			}

			temp = node.ChildNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.IdentifierName));
			if (temp == null) temp = node;
			if (temp is IdentifierNameSyntax i)
			{
				type.Name = i.ToFullString().Trim();
			}

			temp = node.ChildNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.ArrayType));
			if (temp == null) temp = node;
			if (temp is ArrayTypeSyntax array)
			{
				type.Traits |= Trait.Array;
				type.ArrayRankSpecifiers = array.RankSpecifiers.Count;
				SetValues(type, array.ElementType);
			}

			temp = node.ChildNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.GenericName));
			if (temp == null) temp = node;
			if (temp is GenericNameSyntax generic)
			{
				type.Name = generic.Identifier.ValueText.Trim();
				if (Pattern.EnumerableType.IsMatch(generic.ToFullString())) type.Traits |= Trait.Enumerable;
				foreach (CSharpSyntaxNode item in generic.TypeArgumentList.ChildNodes())
				{
					var arg = new TypeDeclaration();
					type.ParameterList.Add(arg);
					SetValues(arg, item);
				}
				return;
			}

			temp = node.ChildNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.PredefinedType));
			if (temp == null) temp = node;
			if (temp is PredefinedTypeSyntax premitive)
			{
				type.Name = premitive.ToFullString().Trim();
				if (!string.Equals(type.Name, nameof(Object), StringComparison.OrdinalIgnoreCase)) type.Traits |= Trait.Primitive;
			}
		}

		private static void SetValues(TypeDeclaration type, BaseListSyntax baseList)
		{
			if (baseList != null)
				foreach (SimpleBaseTypeSyntax item in baseList.ChildNodes().Where(x => x.IsKind(SyntaxKind.SimpleBaseType)))
				{
					var baseType = new TypeDeclaration();
					SetValues(baseType, item.Type);
					type.BaseList.Add(baseType);
				}
		}

		private static void SetValues(TypeDeclaration type, TypeParameterListSyntax args)
		{
			if (args != null)
				foreach (TypeParameterSyntax item in args.ChildNodes())
				{
					type.ParameterList.Add(new TypeDeclaration(item.Identifier.ValueText.Trim()));
				}
		}

		private void GetBaseTypes(CSharpSyntaxNode node)
		{
			string shortName(string x)
			{
				int index = x.LastIndexOf('.');
				return (index >= 0 ? x.Substring(index + 1) : x).Trim();
			}

			var baseList = from x in node.ChildNodes()
						   where x.IsKind(SyntaxKind.BaseList)
						   let bls = (BaseListSyntax)x
						   from y in bls.Types
						   select y;

			foreach (var item in baseList)
			{
				_definition.BaseList.Add(new TypeDeclaration
				{
					Name = shortName(item.ToFullString())
				});
			}
		}

		#region Private Members

		private TypeDeclaration _definition;

		#endregion Private Members
	}
}
