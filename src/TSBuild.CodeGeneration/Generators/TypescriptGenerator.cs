using System.IO;
using System.Text;

namespace Acklann.TSBuild.CodeGeneration.Generators
{
	public class TypescriptGenerator
	{
		public static byte[] Emit(params string[] sourceFiles) => Emit(default, sourceFiles);

		public static byte[] Emit(TypescriptGeneratorSettings settings, params string[] sourceFiles)
		{
			throw new System.NotImplementedException();
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
					else if (settings.UseKnockoutJs)
						EmitKnockoutJsModel(writer, definition, settings);
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

		private static void EmitEnumDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.EmitLine($"export enum {definition.Name.ToPascal()} {{");
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

		private static void EmitClassDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.WriteClassStart(definition);
			writer.WriteLine(" {");
			writer.PushIndent();

			MemberDeclaration member;
			int n = definition.Members.Count;
			for (int i = 0; i < n; i++)
			{
				member = definition.Members[i];
				writer.EmitLine($"{member.Name.ToCamel()}?: {member.Type.ToTypeName(settings)};");
			}

			writer.PopIndent();
			writer.EmitLine("}");
		}

		private static void EmitInterfaceDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.WriteClassStart(definition);
			writer.WriteLine(" {");
			writer.PushIndent();

			MemberDeclaration member;
			int n = definition.Members.Count;
			for (int i = 0; i < n; i++)
			{
				member = definition.Members[i];
				writer.EmitLine($"{member.Name.ToCamel()}?: {member.Type.ToTypeName(settings)};");
			}

			writer.PopIndent();
			writer.EmitLine("}");
		}

		private static void EmitKnockoutJsModel(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.WriteIndent();
			writer.Write("export ");
			if (settings.UseAbstract) writer.Write("abstract ");
			writer.Write("class ");
			writer.WriteTypeSignature(definition);
			writer.WriteTypeBaseList(definition);
			writer.WriteLine(" {");
			writer.PushIndent();
			writer.WriteIndent();

			// Contructor
			writer.WriteLine($"constructor(model?: any) {{");
			writer.PushIndent();

			int n = definition.Members.Count;
			for (int i = 0; i < n; i++)
			{
				string name = definition.Members[i].Name.ToCamel();
				writer.WriteIndent();
				writer.WriteLine($"this.{name} = ko.observable((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
			}
			writer.CloseBrace();
			writer.WriteLine();
			writer.WriteIndent();

			// Copy Function
			writer.WriteLine($"public copy(model: any) {{");
			writer.PushIndent();

			foreach (MemberDeclaration member in definition.Members)
			{
				string name = member.Name.ToCamel();
				writer.WriteIndent();
				writer.WriteLine($"this.{name}((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
			}
			writer.CloseBrace();
			writer.WriteLine();

			// Members
			foreach (MemberDeclaration member in definition.Members)
			{
				writer.WriteProperty(member);
			}

			writer.CloseBrace();
		}
	}
}
