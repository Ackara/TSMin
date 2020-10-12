using Acklann.Diffa;
using Acklann.TSBuild.CodeGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.TSBuild.Tests
{
	[TestClass]
	public class AdapterTest
	{
		[DataTestMethod]
		[DynamicData(nameof(GetSourceFiles), DynamicDataSourceType.Method)]
		public void Can_parse_source_files(string[] sourceFiles)
		{
			// Arrange
			var results = new StringBuilder();
			var label = string.Join('_', sourceFiles.Select(x => Path.GetFileNameWithoutExtension(x)));

			// Act
			foreach (var type in Adapter.ParseFiles(sourceFiles))
				results.AppendLine(Serialize(type));

			// Assert
			results.ToString().ShouldNotBeNullOrEmpty();
			Diff.Approve(results, ".txt", label);
		}

		[DataTestMethod]
		[DynamicData(nameof(GetUnResolvedTypes), DynamicDataSourceType.Method)]
		public void Can_resolve_dependencies(TypeDefinition[] declarations, Action<TypeDefinition[]> assert)
		{
			// Arrange + Act
			var results = TypeDefinition.ResolveDependencies(declarations).ToArray();

			// Assert
			assert(results);
		}

		[DataTestMethod]
		[DynamicData(nameof(GetUnorderedTypes), DynamicDataSourceType.Method)]
		public void Can_sort_dependencies(TypeDefinition[] declarations, string expected)
		{
			// Arrange + Act
			var types = TypeDefinition.ResolveDependencies(declarations).Select(x => x.Name);
			var results = string.Join(" ", types);

			// Assert
			results.ShouldBe(expected);
		}

		#region Backing Members

		private static IEnumerable<object[]> GetUnResolvedTypes()
		{
			var a = new TypeDefinition("a", (Trait.Public | Trait.InScope));
			var b = new TypeDefinition("b", (Trait.Public | Trait.InScope));
			var c = new TypeDefinition("c", (Trait.Public | Trait.InScope));
			var d = new TypeDefinition("d", (Trait.Public | Trait.InScope));
			var e = new TypeDefinition("e", (Trait.Public | Trait.InScope));
			var f = new TypeDefinition("f", (Trait.Public | Trait.InScope));

			MemberDefinition field(TypeDefinition t) => new MemberDefinition(t.Name, new TypeDefinition(t.Name) { Traits = Trait.Public });
			void clear(TypeDefinition t) { t.BaseList.Clear(); t.Members.Clear(); t.ParameterList.Clear(); }
			void reset() { clear(a); clear(b); clear(c); clear(d); clear(e); clear(f); }

			// =================================== //

			yield return new object[] { new TypeDefinition[] { a, b, c }, new Action<TypeDefinition[]>((x) =>
			{
				foreach (var item in x) item.InScope.ShouldBeTrue();
			})};

			reset();
			a.Members.Add(new MemberDefinition("id", new TypeDefinition("int", Trait.Public | Trait.Primitive)));
			a.Members.Add(new MemberDefinition("name", new TypeDefinition("string", Trait.Public | Trait.Primitive)));
			b.Members.Add(field(a));
			b.Members.Add(new MemberDefinition("Items",
				new TypeDefinition("List", Trait.Public)
				{
					ParameterList = new List<TypeDefinition>() { new TypeDefinition("a", (Trait.Public | Trait.InScope)) }
				}));

			yield return new object[] { new TypeDefinition[] { a, b }, new Action<TypeDefinition[]>((results)=>
			{
				foreach (var item in results) item.InScope.ShouldBeTrue();

				var target = results.First(x=> x.Name == b.Name).Members.First(x=> x.Name == "Items");
				target.Type.ParameterList[0].InScope.ShouldBeTrue("Member not in scope");
			})};
		}

		private static IEnumerable<object[]> GetUnorderedTypes()
		{
			var a = new TypeDefinition("a", (Trait.Public | Trait.InScope));
			var b = new TypeDefinition("b", (Trait.Public | Trait.InScope));
			var c = new TypeDefinition("c", (Trait.Public | Trait.InScope));
			var d = new TypeDefinition("d", (Trait.Public | Trait.InScope));
			var e = new TypeDefinition("e", (Trait.Public | Trait.InScope));
			var f = new TypeDefinition("f", (Trait.Public | Trait.InScope));

			void clear(TypeDefinition t) { t.BaseList.Clear(); t.Members.Clear(); }
			void reset() { clear(a); clear(b); clear(c); clear(d); clear(e); clear(f); }
			MemberDefinition field(TypeDefinition t) => new MemberDefinition(t.Name, new TypeDefinition(t.Name) { Traits = Trait.Public });

			// =================================== //

			yield return new object[] { new TypeDefinition[] { a, b, c }, "a b c" };

			reset();
			a.BaseList.Add(b);
			b.BaseList.Add(c);
			yield return new object[] { new TypeDefinition[] { a, b, c }, "c b a" };

			reset();
			a.Members.Add(field(c));
			b.Members.Add(field(c));
			yield return new object[] { new TypeDefinition[] { a, b, c }, "c a b" };

			reset();
			a.BaseList.Add(b);
			a.Members.Add(field(c));
			yield return new object[] { new TypeDefinition[] { a, b, c }, "b c a" };

			reset();
			a.Members.Add(new MemberDefinition("id", new TypeDefinition("int", Trait.Public | Trait.Primitive)));
			a.Members.Add(new MemberDefinition("name", new TypeDefinition("string", Trait.Public | Trait.Primitive)));
			b.Members.Add(field(a));
			b.Members.Add(new MemberDefinition("Items",
				new TypeDefinition("List", Trait.Public)
				{
					ParameterList = new List<TypeDefinition>() { a }
				}));
		}

		private static IEnumerable<object[]> GetSourceFiles()
		{
#if DEBUG
			//yield return new object[] { new string[]
			//{
			//
			//}};
#endif
			string folder = Path.Combine(Sample.DirectoryName, "source-files");
			if (!Directory.Exists(folder)) throw new DirectoryNotFoundException($"Could not find directory at '{folder}'.");

			foreach (string item in Directory.GetFiles(folder))
			{
				yield return new object[] { new string[] { item } };
			}
		}

		private static string Serialize(TypeDefinition type)
		{
			var builder = new StringBuilder();
			builder.AppendLine($"{type.Name}");

			builder.Append($"\tBaseList: ");
			builder.AppendJoin(", ", type.BaseList.Select(x => x.Name));

			builder.AppendLine();
			builder.AppendLine();

			builder.Append("\tMembers: ");
			builder.AppendJoin(", ", type.Members.Select(x => x.Name));

			builder.AppendLine();
			builder.AppendLine();

			return builder.ToString();
		}

		#endregion Backing Members
	}
}
