using System;

namespace Acklann.TSBuild.CodeGeneration
{
	[Flags]
	public enum Trait
	{
		None = 0,
		Public = 1,
		Private = 2,
		Protected = 4,
		Abstract = 8,
		Static = 16,
		Array = 32,
		Enumerable = 64,
		Interface = 128,
		Enum = 256,
		Class = 512,
		Struct = 1024,
		InScope = 2048,
		Primitive = 4096,
		Constant = 8192
	}
}
