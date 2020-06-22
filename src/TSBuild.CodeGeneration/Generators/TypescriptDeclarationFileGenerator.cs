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

		public static byte[] EmitDeclarationFile(params TypeDefinition[] definitions) => EmitDeclarationFile(default, definitions);

		public static byte[] EmitDeclarationFile(TypescriptGeneratorSettings settings, params TypeDefinition[] definitions)
		{
			using (var stream = new MemoryStream())
			using (var writer = new CodeWriter(stream, Encoding.UTF8, settings))
			{
				writer.WriteReferencePaths();
				if (settings.HasNamespace) writer.Write("declare ");
				writer.WriteNamespaceStart();

				TypeDefinition definition;
				int n = definitions.Length;
				for (int i = 0; i < n; i++)
				{
					definition = definitions[i];

					if (definition.IsEnum)
						GenerateEnumDeclaration(writer, definition, settings);
					else
						EmitObjectDeclaration(writer, definition, settings);

					if (i < (n - 1)) writer.WriteLine();
				}

				if (settings.HasNamespace) writer.WriteLine("}");

				writer.Flush();
				return stream.ToArray();
			}
		}

		// ==================== BACKING MEMBERS ==================== //

		private static void GenerateEnumDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			string declare = (settings.HasNamespace ? string.Empty : "declare ");
			writer.EmitLine($"{declare}enum {definition.Name.ToPascal()} {{");
			writer.PushIndent();

			MemberDeclaration member;
			int n = definition.Members.Count;
			for (int i = 0; i < n; i++)
			{
				member = definition.Members[i];

				writer.EmitLine(string.Concat(
					member.Name,
					(member.DefaultValue == default ? string.Empty : $" = {member.DefaultValue}"),
					(i < (n - 1) ? "," : string.Empty)));
			}

			writer.PopIndent();
			writer.EmitLine("}");
		}

		private static void EmitObjectDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.WriteIndent("interface ");
			writer.WriteTypeSignature(definition);

			bool onFirstItem = true;
			foreach (TypeDefinition def in definition.BaseList)
			{
				if (onFirstItem)
				{
					writer.Write(" extends ");
					onFirstItem = false;
				}
				else writer.Write(", ");
				writer.WriteTypeSignature(def);
			}

			writer.WriteLine(" {");
			writer.PushIndent();

			foreach (MemberDeclaration member in definition.Members)
			{
				writer.WriteProperty(member, optional: true);
			}

			writer.CloseBrace();
		}
	}
}
