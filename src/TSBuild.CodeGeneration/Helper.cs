using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acklann.TSBuild.CodeGeneration
{
	internal static class Helper
	{
		public static Trait GetTraits(this SyntaxTokenList modifiers)
		{
			Trait m = 0;
			foreach (var modifier in modifiers)
				switch (modifier.Kind())
				{
					case SyntaxKind.PublicKeyword:
						m |= Trait.Public;
						break;

					case SyntaxKind.AbstractKeyword:
						m |= Trait.Abstract;
						break;

					case SyntaxKind.InterfaceKeyword:
						m |= Trait.Interface;
						break;

					case SyntaxKind.EnumKeyword:
						m |= Trait.Enum;
						break;

					case SyntaxKind.ClassKeyword:
						m |= Trait.Class;
						break;

					case SyntaxKind.StaticKeyword:
						m |= Trait.Static;
						break;

					case SyntaxKind.ConstKeyword:
						m |= Trait.Constant;
						break;
				}

			return m;
		}
	}
}
