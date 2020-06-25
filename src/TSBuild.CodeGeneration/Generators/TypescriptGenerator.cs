using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.TSBuild.CodeGeneration.Generators
{
	public class TypescriptGenerator
	{
		public static byte[] Emit(params string[] sourceFiles)
		{
			return Emit(default, Adapter.ParseFiles(sourceFiles).ToArray());
		}

		public static byte[] Emit(TypescriptGeneratorSettings settings, params string[] sourceFiles)
		{
			return Emit(settings, Adapter.ParseFiles(sourceFiles).ToArray());
		}

		public static byte[] Emit(params TypeDefinition[] definitions) => Emit(default, definitions);

		public static byte[] Emit(TypescriptGeneratorSettings settings, params TypeDefinition[] definitions)
		{
			using (var stream = new MemoryStream())
			using (var writer = new CodeWriter(stream, Encoding.UTF8, settings))
			{
				writer.WriteReferencePaths();
				writer.WriteNamespaceStart();

				TypeDefinition definition;
				int n = definitions.Length;
				for (int i = 0; i < n; i++)
				{
					definition = definitions[i];

					if (definition.IsEnum)
						EmitEnumDeclaration(writer, definition, settings);
					else if (definition.IsInterface)
						EmitInterfaceDeclaration(writer, definition, settings);
					else
						EmitClassDeclaration(writer, definition, settings);

					if (i < (n - 1)) writer.WriteLine();
				}

				writer.WriteNamespaceEnd();

				writer.Flush();
				return stream.ToArray();
			}
		}

		// ==================== BACKING MEMBERS ==================== //

		internal static void EmitEnumDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.WriteIndent($"export enum {definition.Name.ToPascal()} {{");
			writer.WriteLine();
			writer.PushIndent();

			MemberDefinition member;
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

		internal static void EmitClassDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.WriteIndent("export ");
			if (settings.UseAbstract && definition.IsClass) writer.Write("abstract ");
			writer.Write("class ");
			writer.WriteTypeSignature(definition);
			writer.WriteTypeBaseList(definition);

			writer.WriteLine(" {");
			writer.PushIndent();

			foreach (MemberDefinition member in definition.Members)
			{
				writer.WriteProperty(member);
			}

			writer.PopIndent();
			writer.WriteIndent("}");
			writer.WriteLine();
		}

		internal static void EmitInterfaceDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.WriteIndent("export interface ");
			writer.WriteTypeSignature(definition);

			if (definition?.BaseList?.Count > 0)
			{
				var onFirstItem = true;
				writer.Write(" extends ");
				foreach (TypeDefinition def in definition.BaseList)
				{
					if (onFirstItem) onFirstItem = false;
					else writer.Write(", ");

					writer.WriteTypeSignature(def);
				}
			}
			writer.WriteLine(" {");
			writer.PushIndent();

			foreach (MemberDefinition member in definition.Members)
			{
				writer.WriteProperty(member, optional: true);
			}

			writer.PopIndent();
			writer.WriteIndent("}");
			writer.WriteLine();
		}
	}
}
