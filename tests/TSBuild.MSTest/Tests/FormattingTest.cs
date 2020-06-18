using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Acklann.TSBuild.Tests
{
	[TestClass]
	public class FormattingTest
	{
		[DataTestMethod]
		[DataRow("", "")]
		[DataRow("a", "a")]
		[DataRow("A", "a")]
		[DataRow(null, null)]
		[DataRow("UPPERCASE", "uppercase")]
		[DataRow("lowercase", "lowercase")]
		[DataRow("Title Case", "titleCase")]
		[DataRow("PascalCase", "pascalCase")]
		public void Can_convert_text_to_camel_case(string text, string expected)
		{
			var result = CodeGeneration.FormatExtensions.ToCamel(text);
			ShouldBeStringTestExtensions.ShouldBe(result, expected);
		}

		[DataTestMethod]
		[DataRow("", "")]
		[DataRow("a", "A")]
		[DataRow("A", "A")]
		[DataRow(null, null)]
		[DataRow("UPPERCASE", "UPPERCASE")]
		[DataRow("lowercase", "Lowercase")]
		[DataRow("Title Case", "TitleCase")]
		[DataRow("PascalCase", "PascalCase")]
		public void Can_convert_text_to_pascal_case(string text, string expected)
		{
			var result = CodeGeneration.FormatExtensions.ToPascal(text);
			ShouldBeStringTestExtensions.ShouldBe(result, expected);
		}
	}
}
