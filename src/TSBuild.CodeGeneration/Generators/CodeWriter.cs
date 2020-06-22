using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.TSBuild.CodeGeneration.Generators
{
	public class CodeWriter : StreamWriter
	{
		public CodeWriter(Stream stream, Encoding encoding, TypescriptGeneratorSettings settings) : base(stream, encoding)
		{
			_settings = settings;
		}

		public void WriteReferencePaths()
		{
			foreach (string refPath in _settings.References)
				if (!string.IsNullOrEmpty(refPath))
				{
					WriteLine($"/// <reference path=\"{refPath}\" />");
				}
		}

		public void WriteNamespaceStart(bool forDeclarationFile = false)
		{
			if (_settings.HasNamespace)
			{
				if (forDeclarationFile) WriteLine($"declare namespace {_settings.Namespace} {{");
				else WriteLine($"namespace {_settings.Namespace} {{");
				PushIndent();
			}
		}

		public void WriteNamespaceEnd()
		{
			if (_settings.HasNamespace)
			{
				WriteLine('}');
				PopIndent();
			}
		}

		public void WriteClassStart(TypeDefinition definition, bool forDeclarationFile = false)
		{
			Write(GetIndent());
			if (forDeclarationFile) Write("export ");
			if (_settings.UseAbstract && definition.IsClass) Write("abstract ");
			Write(definition.IsInterface ? "interface " : "class ");
			WriteTypeSignature(definition);
			WriteTypeBaseList(definition, forDeclarationFile);
			WriteLine(" {");

			PushIndent();
		}

		public void WriteProperty(MemberDeclaration member, bool forDeclarationFile = false)
		{
			Write(GetIndent());
			Write(member.Name.ToCamel());
			if (forDeclarationFile) Write('?');
			Write(": ");
			WriteDataType(member.Type);
			WriteLine(';');
		}

		public void WriteKnockoutPropertyInitialization(MemberDeclaration member)
		{
			string name = member.Name.ToCamel();

			Write(GetIndent());
			WriteLine($"this.{name} = ko.observable((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
		}

		public void WriteKnockoutPropertyAssignment(MemberDeclaration member)
		{
			string name = member.Name.ToCamel();

			Write(GetIndent());
			WriteLine($"this.{name}((model && model.hasOwnProperty('{name}'))? model.{name} : null);");
		}

		public void WriteEnumValue(MemberDeclaration member)
		{
			Write(GetIndent());
			Write(member.Name);
			if (member.DefaultValue != null) Write($" = {member.DefaultValue}");
		}

		public void WriteDataType(TypeDefinition definition)
		{
			bool isPrimitive = true;

			string name = definition.Name;
			if (definition.IsCollection && definition.ParameterList?.Count == 1) name = definition.ParameterList[0].Name;

			switch (name)
			{
				case "char":
				case "string":
				case nameof(Char):
				case nameof(String):
					name = "string";
					break;

				case "int":
				case "long":
				case "byte":
				case "float":
				case "sbyte":
				case "double":
				case "single":
				case "numeric":
				case "decimal":
				case nameof(Int16):
				case nameof(Int64):
				case nameof(Int32):
				case nameof(Double):
				case nameof(Single):
				case nameof(SByte):
					name = "number";
					break;

				case "bool":
				case nameof(Boolean):
					name = "boolean";
					break;

				default:
					name = (definition.InScope ? NormalizeName(definition) : "any");
					isPrimitive = false;
					break;
			}

			if (_settings.UseKnockoutJs)
			{
				if (isPrimitive || definition.IsEnum)
					Write(definition.IsCollection ? $"KnockoutObservableArray<{name}>" : $"KnockoutObservable<{name}>");
				else
					Write(definition.IsCollection ? $"KnockoutObservableArray<{name}>" : name);
			}
			else Write(definition.IsCollection ? $"Array<{name}>" : name);
		}

		public void WriteTypeSignature(TypeDefinition definition)
		{
			Write(NormalizeName(definition));
			if (definition.HasParameters)
			{
				Write('<');

				TypeDefinition type;
				int n = definition.ParameterList.Count;
				for (int i = 0; i < n; i++)
				{
					type = definition.ParameterList[i];

					if (type.InScope) Write(NormalizeName(type));
					else Write("any");

					if (i < (n - 1)) Write(", ");
				}
				Write('>');
			}
		}

		public void WriteTypeBaseList(TypeDefinition definition, bool asDeclarationFile = false)
		{
			if (definition?.BaseList?.Count > 0)
			{
				int i = 0, n = definition.BaseList.Count;
				if (definition.HasBaseType && asDeclarationFile == false)
				{
					Write(" extends ");
					WriteTypeSignature(definition.BaseType);
					i++;
				}

				Write(asDeclarationFile ? " extends " : " implements ");
				for (; i < n; i++)
				{
					WriteTypeSignature(definition.BaseList[i]);
					if (i < (n - 1)) Write(", ");
				}
			}
		}

		public void WriteIndent(string content = default)
		{
			Write(string.Concat(GetIndent(), content));
		}

		public void CloseBrace()
		{
			PopIndent();
			WriteLine(string.Concat(GetIndent(), '}'));
		}

		public void Emit(string content)
		{
			base.Write(string.Concat(GetIndent(), content));
		}

		public void EmitLine(string content)
		{
			base.WriteLine(string.Concat(GetIndent(), content));
		}

		public void PushIndent()
		{
			_depth++;
		}

		public void PopIndent()
		{
			_depth--;
		}

		#region Backing Members

		private int _depth;
		private readonly TypescriptGeneratorSettings _settings;

		private string NormalizeName(TypeDefinition definition)
		{
			string prefix = (!definition.IsEnum && !string.IsNullOrEmpty(_settings.Prefix) ? _settings.Prefix : string.Empty);
			string suffix = (!definition.IsEnum && !string.IsNullOrEmpty(_settings.Suffix) ? _settings.Suffix : string.Empty);

			return string.Concat(prefix, definition.Name.ToPascal(), suffix).Trim();
		}

		private string GetIndent()
		{
			return string.Concat(Enumerable.Repeat('\t', _depth)) ?? string.Empty;
		}

		#endregion Backing Members
	}
}
