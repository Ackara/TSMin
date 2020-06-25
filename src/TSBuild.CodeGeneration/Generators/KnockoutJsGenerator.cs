using System.IO;
using System.Text;

namespace Acklann.TSBuild.CodeGeneration.Generators
{
	public class KnockoutJsGenerator
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
					{
						TypescriptGenerator.EmitEnumDeclaration(writer, definition);
					}
					else if (definition.IsInterface)
					{
						TypescriptGenerator.EmitInterfaceDeclaration(writer, definition, settings);
					}
					else
					{
						EmitClassDeclaration(writer, definition, settings);
					}

					if (i < (n - 1)) writer.WriteLine();
				}

				writer.WriteNamespaceEnd();

				writer.Flush();
				return stream.ToArray();
			}
		}

		// ==================== BACKING MEMBERS ==================== //

		private static void EmitClassDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.WriteIndent("export ");
			if (settings.UseAbstract && definition.IsClass) writer.Write("abstract ");
			writer.Write("class ");
			writer.WriteTypeSignature(definition);
			writer.WriteTypeBaseList(definition);
			writer.WriteLine(" {");
			writer.PushIndent();

			// --- Contructor --- //

			writer.WriteIndent($"constructor(model?: any) {{");
			writer.WriteLine();
			writer.PushIndent();

			foreach (MemberDefinition member in definition.Members)
			{
				string name = member.Name.ToCamel();
				writer.WriteIndent($"this.{name} = ko.observable((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
				writer.WriteLine();
			}
			writer.CloseBrace();
			writer.WriteLine();

			// --- Copy Function --- //

			writer.WriteIndent($"public copy(model: any) {{");
			writer.WriteLine();
			writer.PushIndent();

			foreach (MemberDefinition member in definition.Members)
			{
				string name = member.Name.ToCamel();
				writer.WriteIndent();
				writer.WriteLine($"this.{name}((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
			}
			writer.CloseBrace();
			writer.WriteLine();

			// --- Properties --- //

			foreach (MemberDefinition member in definition.Members)
			{
				//writer.WriteIndent($"{member.Name.ToCamel()}: {member.Type.ToTypeName(settings)};");
				writer.WriteProperty(member, knockout: true);
			}

			writer.PopIndent();
			writer.WriteIndent("}");
			writer.WriteLine();
		}
	}
}
