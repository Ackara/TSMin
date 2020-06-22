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
	}
}
