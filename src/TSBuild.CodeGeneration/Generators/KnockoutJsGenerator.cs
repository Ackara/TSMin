using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.TSBuild.CodeGeneration.Generators
{
	public class KnockoutJsGenerator
	{
		public static byte[] Emit(params string[] sourceFiles)
		{
			return Emit(default, Adapter.ParseFiles(sourceFiles).ToArray());
		}

		public static byte[] Emit(params TypeDefinition[] definitions)
		{
			return Emit(default, definitions);
		}

		public static byte[] Emit(TypescriptGeneratorSettings settings, params string[] sourceFiles)
		{
			return Emit(settings, Adapter.ParseFiles(sourceFiles).ToArray());
		}

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

		internal static void EmitClassDeclaration(CodeWriter writer, TypeDefinition definition, TypescriptGeneratorSettings settings)
		{
			writer.WriteIndent("export ");
			if (settings.UseAbstract && definition.IsClass) writer.Write("abstract ");
			writer.Write("class ");
			writer.WriteTypeSignature(definition);
			writer.WriteTypeBaseList(definition);
			writer.WriteLine(" {");
			writer.PushIndent();

			// --- Constructor --- //

			writer.WriteIndent($"constructor(model?: any) {{");
			writer.WriteLine();
			writer.PushIndent();

			if (definition.HasInScopeBaseType)
			{
				writer.WriteIndent("super(model);");
				writer.WriteLine();
			}

			foreach (MemberDefinition member in definition.GetPublicFieldsAndProperties())
			{
				string name = member.Name.ToCamel();
				if (member.IsCollection)
					writer.WriteIndent($"this.{name} = ko.observableArray((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
				else if (member.Type.IsObject)
					writer.WriteIndent($"this.{name} = new {CodeWriter.NormalizeName(member.Type, settings)}((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
				else
					writer.WriteIndent($"this.{name} = ko.observable((model && model.hasOwnProperty('{name}'))? model.{name} : null);");

				writer.WriteLine();
			}
			writer.CloseBrace();
			writer.WriteLine();

			// --- Copy Method --- //

			writer.WriteIndent($"public copy(model: any) {{");
			writer.WriteLine();
			writer.PushIndent();

			foreach (MemberDefinition member in definition.GetPublicFieldsAndProperties())
			{
				string name = member.Name.ToCamel();
				writer.WriteIndent();

				if (member.IsArray)
					writer.WriteLine($"this.{name}((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
				else if (member.Type.IsObject)
					writer.WriteLine($"this.{name}.copy((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
				else
					writer.WriteLine($"this.{name}((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
			}
			writer.CloseBrace();
			writer.WriteLine();

			// --- Properties --- //

			foreach (MemberDefinition member in definition.GetPublicFieldsAndProperties())
			{
				writer.WriteProperty(member, knockout: true);
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

			foreach (MemberDefinition member in definition.GetPublicFieldsAndProperties())
			{
				writer.WriteProperty(member, optional: true, knockout: true);
			}

			writer.PopIndent();
			writer.WriteIndent("}");
			writer.WriteLine();
		}
	}
}
