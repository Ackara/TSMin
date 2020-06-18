using System;
using System.Text;

namespace Acklann.TSBuild.CodeGeneration.Generators
{
	public static class FormatExtensions
	{
		public static string ToCamel(this string text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			else if (text.Length == 1) return text.ToLowerInvariant();
			else
			{
				bool allCaps = true;
				var result = new StringBuilder();
				ReadOnlySpan<char> span = text.AsSpan();

				for (int i = 0; i < span.Length; i++)
				{
					if (allCaps && !char.IsUpper(span[i])) allCaps = false;

					if (span[i] == ' ' || span[i] == '_')
						continue;
					else if (i == 0)
						result.Append(char.ToLowerInvariant(span[i]));
					else if (span[i - 1] == ' ' || span[i - 1] == '_')
						result.Append(char.ToUpperInvariant(span[i]));
					else
						result.Append(span[i]);
				}

				return (allCaps ? text.ToLower() : result.ToString());
			}
		}

		public static string ToPascal(this string text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			else if (text.Length == 1) return text.ToUpperInvariant();
			else
			{
				var result = new StringBuilder();
				ReadOnlySpan<char> span = text.AsSpan();

				for (int i = 0; i < span.Length; i++)
				{
					if (span[i] == ' ' || span[i] == '_')
						continue;
					else if (i == 0)
						result.Append(char.ToUpperInvariant(span[i]));
					else if (span[i - 1] == ' ' || span[i - 1] == '_')
						result.Append(char.ToUpperInvariant(span[i]));
					else
						result.Append(span[i]);
				}

				return result.ToString();
			}
		}

		public static string ToShortName(this string text)
		{
			int index = text.LastIndexOf('.');
			return (index >= 0 ? text.Substring(index + 1) : text).Trim();
		}

		internal static string ToDataType(TypeDeclaration definition, TypescriptGeneratorSettings settings)
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
					name = (definition.InScope ? definition.GetName(settings) : "any");
					isPrimitive = false;
					break;
			}

			if (settings.UseKnockoutJs)
				if (isPrimitive || definition.IsEnum)
					return (definition.IsCollection ? $"KnockoutObservableArray<{name}>" : $"KnockoutObservable<{name}>");
				else
					return (definition.IsCollection ? $"KnockoutObservableArray<{name}>" : name);
			else
				return (definition.IsCollection ? $"Array<{name}>" : name);
		}

		internal static string GetName(this TypeDeclaration definition, TypescriptGeneratorSettings settings)
		{
			string prefix = (!definition.IsEnum && !string.IsNullOrEmpty(settings.Prefix) ? settings.Prefix : string.Empty);
			string suffix = (!definition.IsEnum && !string.IsNullOrEmpty(settings.Suffix) ? settings.Suffix : string.Empty);

			return string.Concat(prefix, definition.Name, suffix).Trim();
		}
	}
}
