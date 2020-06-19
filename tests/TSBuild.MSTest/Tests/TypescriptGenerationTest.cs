using Acklann.Diffa;
using Acklann.TSBuild.CodeGeneration;
using Acklann.TSBuild.CodeGeneration.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
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
			var declaration = new TypeDefinition("Foo", (Trait.Public | Trait.Enum))
				.Add(new MemberDeclaration("A", new TypeDefinition("int", (Trait.Enum | Trait.Public))))
				.Add(new MemberDeclaration("B", new TypeDefinition("int", (Trait.Enum | Trait.Public))));

			// Act + Assert
			string ts = UTF8(DeclarationFileGenerator.EmitDeclarationFile(declaration));
			ts.ShouldMatch(@"declare enum Foo\s+{\s+A,\s+B\s+}");

			var config = new TypescriptGeneratorSettings(ns: "Foo");
			ts = UTF8(DeclarationFileGenerator.EmitDeclarationFile(config, declaration));
			ts.ShouldMatch(@"declare namespace Foo\s+{\s+enum Foo\s+{\s+A,\s+B\s+}\s+}");
		}

		[DataTestMethod]
		[DynamicData(nameof(GetClassDefinitions), DynamicDataSourceType.Method)]
		public void Can_emit_typescript_dts_file_class(string label, TypeDefinition[] args)
		{
			string result = UTF8(DeclarationFileGenerator.EmitDeclarationFile(args));
			Diff.Approve(result, Encoding.UTF8, "d.ts", label);
		}

		[DataTestMethod]
		[DynamicData(nameof(GetClassDefinitions), DynamicDataSourceType.Method)]
		public void Can_emit_typescript_file_classes(string label, TypeDefinition[] args)
		{
			var config = new TypescriptGeneratorSettings("Foo", suffix: "Base");
			string result = UTF8(TypescriptGenerator.Emit(config, args));
			Diff.Approve(result, Encoding.UTF8, "ts", label);
		}

		[DataTestMethod]
		[DynamicData(nameof(GetClassDefinitions), DynamicDataSourceType.Method)]
		public void Can_emit_knockout_js_models(string label, TypeDefinition[] args)
		{
			var config = new TypescriptGeneratorSettings("App", useAbstract: true, koJs: true, references: new string[] { "../../../node_modules/@types/knockout/index.d.ts" });
			string result = UTF8(TypescriptGenerator.Emit(config, args));
			Diff.Approve(result, Encoding.UTF8, "ts", label);
		}

		#region Backing Members

		private static IEnumerable<object[]> GetClassDefinitions()
		{
			var status = new TypeDefinition("Status", (Trait.Public | Trait.Enum))
				.Add(new MemberDeclaration("Striving", new TypeDefinition("int"), 5))
				.Add(new MemberDeclaration("Endangered", new TypeDefinition("int")))
				.Add(new MemberDeclaration("Instinct", new TypeDefinition("int")));

			var animal = new TypeDefinition("Animal", (Trait.Public | Trait.Interface))
				.Add(new MemberDeclaration("Name", new TypeDefinition("string")))
				.Add(new MemberDeclaration("Legs", new TypeDefinition("int")))
				.Add(new MemberDeclaration("Status", status));

			yield return new object[]
			{
				"simple",
				new TypeDefinition[] { animal }
			};

			// -----

			var feline = new TypeDefinition("Feline", (Trait.Public | Trait.Interface))
				.Inherit(animal)
				.Add(new MemberDeclaration("Whiskers", new TypeDefinition("int")));

			var pathera = new TypeDefinition("Pathera", (Trait.Public | Trait.Interface))
				.Inherit(animal)
				.Add(new MemberDeclaration("Stealth", new TypeDefinition("int")));

			var lion = new TypeDefinition("Lion", Trait.Public)
				.Inherit(feline)
				.Inherit(pathera)
				.Add(new MemberDeclaration("Kills", new TypeDefinition("int")));

			yield return new object[]
			{
				"inheritance",
				TypeDefinition.ResolveDependencies(new TypeDefinition[] { status, animal, feline, pathera, lion }).ToArray()
			};

			// -----

			var skill = new TypeDefinition("Skill", (Trait.Public | Trait.Interface))
				.Add(new TypeDefinition("T"));

			var predator = new TypeDefinition("Predator", Trait.Public)
				.Add(new MemberDeclaration("Name", new TypeDefinition("string")));

			var africanLion = new TypeDefinition("AfricanLion", Trait.Public)
				.Inherit(predator)
				.Inherit(skill)
				.Add(new MemberDeclaration("Region", new TypeDefinition("string")));

			yield return new object[]
			{
				"generic-type",
				TypeDefinition.ResolveDependencies(new TypeDefinition[]{ africanLion, skill, predator }).ToArray()
			};
		}

		private static string UTF8(byte[] x) => Encoding.UTF8.GetString(x);

		#endregion Backing Members
	}
}
