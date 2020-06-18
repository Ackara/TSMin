using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.TSBuild.CodeGeneration
{
	public class Adapter
	{
		public static IEnumerable<TypeDeclaration> ParseFiles(params string[] sourceFiles)
		{
			TypeDeclaration[] declarations = ReadAll(sourceFiles).ToArray();
			return TypeDeclaration.ResolveDependencies(declarations);
		}

		private static IEnumerable<TypeDeclaration> ReadAll(string[] sourceFiles)
		{
			foreach (string sourceFile in sourceFiles)
				if (File.Exists(sourceFile))
				{
					if (sourceFile.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
					{
						foreach (TypeDeclaration item in CSharpAdapter.ReadFile(sourceFile))
							yield return item;
					}
					else if (sourceFile.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
					{
						foreach (TypeDefinition item in ModuleDefinition.ReadModule(sourceFile).Types.Where(x => x.IsPublic))
							yield return ILAdapter.AsTypeDeclaration(item);
					}
					else throw new NotSupportedException($"'{Path.GetExtension(sourceFile)}' files are not supported yet.");
				}
		}
	}
}
