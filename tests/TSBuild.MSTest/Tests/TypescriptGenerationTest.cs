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
		public void Can_emit_enum_definition_as_as_dts_file()
		{
			// Arrange
			var declaration = new TypeDefinition("Foo", (Trait.Public | Trait.Enum))
				.Add(new MemberDefinition("A", new TypeDefinition("int", (Trait.Enum | Trait.Public))))
				.Add(new MemberDefinition("B", new TypeDefinition("int", (Trait.Enum | Trait.Public))));

			// Act + Assert
			string ts = UTF8(DeclarationFileGenerator.EmitDeclarationFile(declaration));
			ts.ShouldMatch(@"declare enum Foo\s+{\s+A,\s+B\s+}");

			var config = new TypescriptGeneratorSettings(ns: "Foo");
			ts = UTF8(DeclarationFileGenerator.EmitDeclarationFile(config, declaration));
			ts.ShouldMatch(@"declare namespace Foo\s+{\s+enum Foo\s+{\s+A,\s+B\s+}\s+}");
		}

		[DataTestMethod]
		[DynamicData(nameof(GetDefinitions), DynamicDataSourceType.Method)]
		public void Can_emit_class_definitions_as_dts_file(string label, TypeDefinition[] args)
		{
			string result = UTF8(DeclarationFileGenerator.EmitDeclarationFile(args));
			Diff.Approve(result, Encoding.UTF8, "d.ts", label);
		}

		[DataTestMethod]
		[DynamicData(nameof(GetDefinitions), DynamicDataSourceType.Method)]
		public void Can_emit_class_definitions_as_typescript_models(string label, TypeDefinition[] args)
		{
			var config = new TypescriptGeneratorSettings("Foo", suffix: "Base");
			string result = UTF8(TypescriptGenerator.Emit(config, args));
			Diff.Approve(result, Encoding.UTF8, "ts", label);
		}

		[DataTestMethod]
		[DynamicData(nameof(GetDefinitions), DynamicDataSourceType.Method)]
		public void Can_emit_definition_as_knockout_js_models(string label, TypeDefinition[] args)
		{
			var config = new TypescriptGeneratorSettings("App", useAbstract: true, koJs: true, references: new string[] { "../../../node_modules/@types/knockout/index.d.ts" });
			string result = UTF8(KnockoutJsGenerator.Emit(config, args));
			Diff.Approve(result, Encoding.UTF8, "ts", label);
		}

		#region Backing Members

		private static IEnumerable<object[]> GetDefinitions()
		{
			var status = new TypeDefinition("Status", (Trait.Public | Trait.Enum))
				.Add(new MemberDefinition("Striving", new TypeDefinition("int"), 5))
				.Add(new MemberDefinition("Endangered", new TypeDefinition("int")))
				.Add(new MemberDefinition("Instinct", new TypeDefinition("int")));

			var animal = new TypeDefinition("Animal", (Trait.Public | Trait.Interface))
				.Add(new MemberDefinition("Name", new TypeDefinition("string")))
				.Add(new MemberDefinition("Legs", new TypeDefinition("int")))
				.Add(new MemberDefinition("Status", status));

			yield return new object[]
			{
				"simple",
				new TypeDefinition[] { animal }
			};

			// -----

			var feline = new TypeDefinition("Feline", (Trait.Public | Trait.Interface))
				.Inherit(animal)
				.Add(new MemberDefinition("Whiskers", new TypeDefinition("int")));

			var pathera = new TypeDefinition("Pathera", (Trait.Public | Trait.Interface))
				.Inherit(animal)
				.Add(new MemberDefinition("Stealth", new TypeDefinition("int")));

			var lion = new TypeDefinition("Lion", (Trait.Public | Trait.Class))
				.Inherit(feline)
				.Inherit(pathera)
				.Add(new MemberDefinition("Name", new TypeDefinition("string")))
				.Add(new MemberDefinition("Legs", new TypeDefinition("int")))
				.Add(new MemberDefinition("Status", status))
				.Add(new MemberDefinition("Kills", new TypeDefinition("int")))
				.Add(new MemberDefinition("Whiskers", new TypeDefinition("int")))
				.Add(new MemberDefinition("Stealth", new TypeDefinition("int")))
				;

			yield return new object[]
			{
				"inheritance",
				TypeDefinition.ResolveDependencies(new TypeDefinition[] { status, animal, feline, pathera, lion }).ToArray()
			};

			// -----

			var skill = new TypeDefinition("Skill", (Trait.Public | Trait.Interface))
				.Add(new TypeDefinition("T"));

			var predator = new TypeDefinition("Predator", (Trait.Public | Trait.Class))
				.Add(new MemberDefinition("Name", new TypeDefinition("string")));

			var africanLion = new TypeDefinition("AfricanLion", (Trait.Public | Trait.Class))
				.Inherit(predator)
				.Inherit(skill)
				.Add(new MemberDefinition("Region", new TypeDefinition("string")));

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
