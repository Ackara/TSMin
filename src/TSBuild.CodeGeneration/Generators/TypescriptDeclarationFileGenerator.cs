using System;
using System.IO;
using System.Text;

namespace Acklann.TSBuild.CodeGeneration.Generators
{
	public class DeclarationFileGenerator
	{
		public static byte[] EmitDeclarationFile(params string[] sourceFiles) => EmitDeclarationFile(default, sourceFiles);

		public static byte[] EmitDeclarationFile(TypescriptGeneratorSettings settings, params string[] sourceFiles)
		{
			throw new System.NotImplementedException();
		}

		public static byte[] EmitDeclarationFile(params TypeDeclaration[] definitions) => EmitDeclarationFile(default, definitions);

		public static byte[] EmitDeclarationFile(TypescriptGeneratorSettings settings, params TypeDeclaration[] definitions)
		{
			using (var stream = new MemoryStream())
			using (var writer = new CodeWriter(stream, Encoding.UTF8))
			{
				if (settings.HasNamespace)
				{
					writer.WriteLine($"declare namespace {settings.Namespace}");
					writer.WriteLine("{");
				}

				foreach (TypeDeclaration definition in definitions)
				{
					if (definition.IsEnum)
						GenerateEnumDeclaration(definition, settings, writer);
					else
						GenerateClassDeclaration(definition, writer);
				}

				if (settings.HasNamespace) writer.WriteLine("}");

				writer.Flush();
				return stream.ToArray();
			}
		}

		// ==================== BACKING MEMBERS ==================== //

		private static void GenerateEnumDeclaration(TypeDeclaration definition, TypescriptGeneratorSettings settings, CodeWriter writer)
		{
			string declare = (settings.HasNamespace ? string.Empty : "declare ");
			writer.EmitLine($"{declare}enum {definition.Name.ToPascal()}");
			writer.EmitLine("{");
			writer.PushIndent();

			int n = definition.Members.Count;
			for (int i = 0; i < n; i++)
			{
				writer.EmitLine(string.Concat($"{definition.Members[i].Name}", (i < (n - 1) ? "," : string.Empty)));
			}

			writer.PopIndent();
			writer.EmitLine("}");
		}

		private static void GenerateClassDeclaration(TypeDeclaration definition, CodeWriter writer)
		{
			string type = (definition.IsInterface ? "interface" : "class");

			writer.EmitLine($"interface {definition.Name.ToPascal()}");
			writer.EmitLine("{");
			writer.PushIndent();

			MemberDeclaration member;
			int n = definition.Members.Count;
			for (int i = 0; i < n; i++)
			{
				member = definition.Members[i];
				writer.EmitLine($"{member.Name.ToCamel()}?: ");
			}

			writer.PopIndent();
			writer.EmitLine("}");
		}
	}
}
