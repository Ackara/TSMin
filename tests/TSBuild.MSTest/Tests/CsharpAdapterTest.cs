using Acklann.TSBuild.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;

namespace Acklann.TSBuild.Tests
{
	[TestClass]
	public class CsharpAdapterTest
	{
		[TestMethod]
		public void Can_parse_csharp_enum()
		{
			TypeDefinition result;

			result = CreateTypeFromSnippet("[ATTR]enum Foo { [Col]A, [Col]B }");
			result.Name.ShouldBe("Foo");
			result.Members[0].Name.ShouldBe("A");

			result = CreateTypeFromSnippet("enum Foo { A, B }");
			result.Name.ShouldBe("Foo");
			result.IsEnum.ShouldBeTrue();
			result.Members.Count.ShouldBe(2);
			result.Members[0].Name.ShouldBe("A");

			result = CreateTypeFromSnippet("enum Foo { A = 1, B = 4 }");
			result.Members[0].DefaultValue.ShouldBe(1);
			result.Members[1].DefaultValue.ShouldBe(4);
		}

		[TestMethod]
		public void Can_parse_csharp_type()
		{
			TypeDefinition result;

			result = CreateTypeFromSnippet("class Foo<T> {}");
			result.ParameterList[0].Name.ShouldBe("T");

			result = CreateTypeFromSnippet("class Foo: List<string> { }");
			result.BaseList[0].Name.ShouldBe("List");
			result.BaseList[0].ParameterList[0].Name.ShouldBe("string");

			result = CreateTypeFromSnippet("class Foo : Base1, Base2 { }");
			result.BaseList[0].Name.ShouldBe("Base1");
			result.BaseList.Count.ShouldBe(2);

			result = CreateTypeFromSnippet("[Table()]struct Foo { }");
			result.Name.ShouldBe("Foo");

			result = CreateTypeFromSnippet("class Foo { }");
			result.Name.ShouldBe("Foo");
		}

		[TestMethod]
		public void Can_parse_csharp_property()
		{
			MemberDefinition result;

			result = CreateMemberFromSnippet("List<object[]> Data { get; set; }");
			result.Name.ShouldBe("Data");
			result.Type.Name.ShouldBe("List");
			result.Type.ParameterList[0].Name.ShouldBe("object");
			result.Type.ParameterList[0].IsArray.ShouldBeTrue();

			result = CreateMemberFromSnippet("public System.DateTime Date { get; set; }");
			result.Name.ShouldBe("Date");
			result.Type.Name.ShouldBe("DateTime");
			result.Type.Namespace.ShouldBe("System");

			result = CreateMemberFromSnippet("public Money Amount { get; set; }");
			result.Name.ShouldBe("Amount");
			result.Type.Name.ShouldBe("Money");

			result = CreateMemberFromSnippet("public List<int> Collection { get; set; }");
			result.Name.ShouldBe("Collection");
			result.Type.Name.ShouldBe("List");
			result.Type.ParameterList[0].Name.ShouldBe("int");
			result.Type.IsCollection.ShouldBeTrue();

			result = CreateMemberFromSnippet("public string Name { get; set; }");
			result.Type.Name.ShouldBe("string");
			result.Name.ShouldBe("Name");

			result = CreateMemberFromSnippet("public int[] Collection { get; set; }");
			result.Name.ShouldBe("Collection");
			result.Type.Name.ShouldBe("int");
			result.Type.IsArray.ShouldBeTrue();
			result.Type.ArrayRankSpecifiers.ShouldBe(1);

			result = CreateMemberFromSnippet("public Foo[] Items { get; set; }");
			result.Name.ShouldBe("Items");
			result.Type.IsArray.ShouldBeTrue();
		}

		[TestMethod]
		public void Can_parse_csharp_field()
		{
			MemberDefinition result;

			result = CreateMemberFromSnippet("[Foo]string Name;");
			result.Name.ShouldBe("Name");
			result.Type.Name.ShouldBe("string");

			result = CreateMemberFromSnippet("string Id, Name;");
			result.Name.ShouldBe("Id");
			result.Type.Name.ShouldBe("string");
			result.Owner.Members.Count.ShouldBe(2);

			result = CreateMemberFromSnippet("int[] List;");
			result.Name.ShouldBe("List");
			result.Type.Name.ShouldBe("int");
			result.Type.IsArray.ShouldBeTrue();
			result.Type.ArrayRankSpecifiers.ShouldBe(1);

			result = CreateMemberFromSnippet("string Name;");
			result.Name.ShouldBe("Name");
			result.Type.Name.ShouldBe("string");

			result = CreateMemberFromSnippet("const int MAX = 10;");
			result.IsConstant.ShouldBeTrue();
			result.Name.ShouldBe("MAX");
		}

		#region Backing Members

		private static MemberDefinition CreateMemberFromSnippet(string snippet)
		{
			if (string.IsNullOrEmpty(snippet)) throw new ArgumentNullException(nameof(snippet));

			var sut = new CSharpAdapter();
			var tree = CSharpSyntaxTree.ParseText(snippet);

			if (tree.TryGetRoot(out SyntaxNode node)) sut.Visit(node);

			return sut.Definition.Members[0];
		}

		private static TypeDefinition CreateTypeFromSnippet(string snippet)
		{
			if (string.IsNullOrEmpty(snippet)) throw new ArgumentNullException(nameof(snippet));

			var sut = new CSharpAdapter();
			var tree = CSharpSyntaxTree.ParseText(snippet);

			if (tree.TryGetRoot(out SyntaxNode node)) sut.Visit(node);

			return sut.Definition;
		}

		#endregion Backing Members
	}
}
