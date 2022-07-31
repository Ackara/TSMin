using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.TSBuild.CodeGeneration
{
	public class Adapter
	{
		public static IEnumerable<TypeDefinition> ParseFiles(params string[] sourceFiles)
		{
			TypeDefinition[] declarations = ReadAll(sourceFiles).ToArray();
			return TypeDefinition.ResolveDependencies(declarations);
		}

		private static IEnumerable<TypeDefinition> ReadAll(string[] sourceFiles)
		{
			foreach (string sourceFile in sourceFiles)
				if (File.Exists(sourceFile))
				{
					if (sourceFile.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
					{
						foreach (TypeDefinition item in CSharpAdapter.ReadFile(sourceFile))
							if (!string.IsNullOrEmpty(item.Name)) yield return item;
					}
					else if (sourceFile.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
					{
						foreach (Mono.Cecil.TypeDefinition item in ModuleDefinition.ReadModule(sourceFile).Types.Where(x => x.IsPublic))
							if (!string.IsNullOrEmpty(item.Name)) yield return ILAdapter.AsTypeDeclaration(item);
					}
					else throw new NotSupportedException($"'{Path.GetExtension(sourceFile)}' files are not supported yet.");
				}
		}
	}
}
