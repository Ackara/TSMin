using Acklann.Diffa;
using Acklann.TSBuild.CodeGeneration;
using Acklann.TSBuild.CodeGeneration.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.Text;

namespace Acklann.TSBuild.Tests
{
	[TestClass]
	public class TypescriptGenerationTest
	{
		[TestMethod]
		public void Can_emit_typescript_dts_file_enum()
		{
			// Arrange
			var declaration = new TypeDeclaration("Foo", (Trait.Public | Trait.Enum))
				.Add(new MemberDeclaration("A", new TypeDeclaration("int", (Trait.Enum | Trait.Public))))
				.Add(new MemberDeclaration("B", new TypeDeclaration("int", (Trait.Enum | Trait.Public))));

			// Act + Assert
			string ts = UTF(DeclarationFileGenerator.EmitDeclarationFile(declaration));
			ts.ShouldMatch(@"declare enum Foo\s+{\s+A,\s+B\s+}");

			var config = new TypescriptGeneratorSettings(ns: "Foo");
			ts = UTF(DeclarationFileGenerator.EmitDeclarationFile(config, declaration));
			ts.ShouldMatch(@"declare namespace Foo\s+{\s+enum Foo\s+{\s+A,\s+B\s+}\s+}");
		}

		[TestMethod]
		[DynamicData(nameof(GetClassDefinitions), DynamicDataSourceType.Method)]
		public void Can_emit_typescript_dts_file_class(string label, TypeDeclaration[] args)
		{
			string result = UTF(DeclarationFileGenerator.EmitDeclarationFile(args));
			Diff.Approve(result, "d.ts", label);
		}

		#region Backing Members

		private static IEnumerable<object[]> GetClassDefinitions()
		{
			var status = new TypeDeclaration("Status", (Trait.Public | Trait.Enum))
				.Add(new MemberDeclaration("Striving", new TypeDeclaration("int")))
				.Add(new MemberDeclaration("Endangered", new TypeDeclaration("int")))
				.Add(new MemberDeclaration("Instinct", new TypeDeclaration("int")));

			var animal = new TypeDeclaration("Animal", Trait.Public)
				.Add(new MemberDeclaration("status", status))
				.Add(new MemberDeclaration("Name", new TypeDeclaration("string")))
				.Add(new MemberDeclaration("Legs", new TypeDeclaration("int")));

			var feline = new TypeDeclaration("Feline", Trait.Public)
				.Add(new MemberDeclaration("Whiskers", new TypeDeclaration("int")));

			yield return new object[]
			{
				"simple",
				new TypeDeclaration[] { animal }
			};
		}

		private static string UTF(byte[] x) => Encoding.UTF8.GetString(x);

		#endregion Backing Members
	}
}
