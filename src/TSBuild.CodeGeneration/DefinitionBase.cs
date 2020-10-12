namespace Acklann.TSBuild.CodeGeneration
{
	public class DefinitionBase
	{
		public string Name { get; set; }

		public Trait Traits { get; set; }

		public bool IsClass
		{
			get => Traits.HasFlag(Trait.Class);
			set
			{
				if (value)
					Traits |= Trait.Class;
				else
					Traits &= ~Trait.Class;
			}
		}

		public bool IsArray
		{
			get => Traits.HasFlag(Trait.Array);
			set
			{
				if (value)
					Traits |= Trait.Array;
				else
					Traits &= ~Trait.Array;
			}
		}

		public bool IsInterface
		{
			get => Traits.HasFlag(Trait.Interface);
			set
			{
				if (value)
					Traits |= Trait.Interface;
				else
					Traits &= ~Trait.Interface;
			}
		}

		public bool IsCollection
		{
			get => (Traits.HasFlag(Trait.Array) || Traits.HasFlag(Trait.Enumerable));
		}

		public bool IsPublic
		{
			get => Traits.HasFlag(Trait.Public);
			set
			{
				if (value)
					Traits |= Trait.Public;
				else
					Traits &= ~Trait.Public;
			}
		}

		public bool IsEnum
		{
			get => Traits.HasFlag(Trait.Enum);
			set
			{
				if (value)
					Traits |= Trait.Enum;
				else
					Traits &= ~Trait.Enum;
			}
		}

		public bool IsAbstract
		{
			get => Traits.HasFlag(Trait.Abstract);
			set
			{
				if (value)
					Traits |= Trait.Abstract;
				else
					Traits &= ~Trait.Abstract;
			}
		}

		public bool IsStruct
		{
			get => Traits.HasFlag(Trait.Struct);
			set
			{
				if (value)
					Traits |= Trait.Struct;
				else
					Traits &= ~Trait.Struct;
			}
		}

		public bool InScope
		{
			get => Traits.HasFlag(Trait.InScope);
			set
			{
				if (value)
					Traits |= Trait.InScope;
				else
					Traits &= ~Trait.InScope;
			}
		}

		public bool IsPrimitive
		{
			get => Traits.HasFlag(Trait.Primitive);
			set
			{
				if (value)
					Traits |= Trait.Primitive;
				else
					Traits &= ~Trait.Primitive;
			}
		}

		public bool IsConstant
		{
			get => Traits.HasFlag(Trait.Constant);
			set
			{
				if (value)
					Traits |= Trait.Constant;
				else
					Traits &= ~Trait.Constant;
			}
		}

		public bool IsStatic
		{
			get => Traits.HasFlag(Trait.Static);
			set
			{
				if (value)
					Traits |= Trait.Static;
				else
					Traits &= ~Trait.Static;
			}
		}

		public bool IsObject
		{
			get => IsClass || IsStruct;
		}
	}
}
