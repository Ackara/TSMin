using Acklann.Diffa;
using Acklann.TSBuild.CodeGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
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
		[DynamicData(nameof(GetUnorderedTypes), DynamicDataSourceType.Method)]
		public void Can_resolve_type_dependencies(TypeDeclaration[] declarations, string expected)
		{
			// Arrange + Act
			var types = TypeDeclaration.ResolveDependencies(declarations).Select(x => x.Name);
			var results = string.Join(" ", types);

			// Assert
			results.ShouldBe(expected);
		}

		#region Backing Members

		private static IEnumerable<object[]> GetUnorderedTypes()
		{
			var a = new TypeDeclaration("a", (Trait.Public | Trait.InScope));
			var b = new TypeDeclaration("b", (Trait.Public | Trait.InScope));
			var c = new TypeDeclaration("c", (Trait.Public | Trait.InScope));
			var d = new TypeDeclaration("d", (Trait.Public | Trait.InScope));
			var e = new TypeDeclaration("e", (Trait.Public | Trait.InScope));
			var f = new TypeDeclaration("f", (Trait.Public | Trait.InScope));

			void clear(TypeDeclaration t) { t.BaseList.Clear(); t.Members.Clear(); }
			void reset() { clear(a); clear(b); clear(c); clear(d); clear(e); clear(f); }
			MemberDeclaration field(TypeDeclaration t) => new MemberDeclaration(t.Name, new TypeDeclaration(t.Name) { Traits = Trait.Public });

			// =================================== //

			yield return new object[] { new TypeDeclaration[] { a, b, c }, "a b c" };

			reset();
			a.BaseList.Add(b);
			b.BaseList.Add(c);
			yield return new object[] { new TypeDeclaration[] { a, b, c }, "c b a" };

			reset();
			a.Members.Add(field(c));
			b.Members.Add(field(c));
			yield return new object[] { new TypeDeclaration[] { a, b, c }, "c a b" };

			reset();
			a.BaseList.Add(b);
			a.Members.Add(field(c));
			yield return new object[] { new TypeDeclaration[] { a, b, c }, "b c a" };
		}

		private static IEnumerable<object[]> GetSourceFiles()
		{
			//yield return new object[] { new string[] { Sample.GetTranseiSampleDLL().FullName } };
			//yield return new object[] { new string[] { Sample.GetPaymentmethodCS().FullName, Sample.GetClientCS().FullName } };

			yield break;
		}

		private static string Serialize(TypeDeclaration type)
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
