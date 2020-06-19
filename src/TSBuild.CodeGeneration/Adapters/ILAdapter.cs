using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Acklann.TSBuild.CodeGeneration
{
	public class ILAdapter
	{
		public static IEnumerable<TypeDefinition> Parse(string assemblyFile)
		{
			if (!File.Exists(assemblyFile)) throw new FileNotFoundException($"Could not find file at '{assemblyFile}'.");

			var module = ModuleDefinition.ReadModule(assemblyFile);
			foreach (var item in module.Types)
				if (item.IsPublic)
				{
					yield return AsTypeDeclaration(item);
				}
		}

		internal static TypeDefinition AsTypeDeclaration(Mono.Cecil.TypeDefinition definition)
		{
			var declaration = new TypeDefinition
			{
				Name = Regex.Replace(definition.Name, @"`\d+", string.Empty),
				BaseList = new List<TypeDefinition>(),
				Namespace = definition.Namespace
			};

			if (definition.HasGenericParameters)
				foreach (GenericParameter item in definition.GenericParameters)
				{
					declaration.ParameterList.Add(new TypeDefinition { Namespace = item.Namespace, Name = item.Name });
				}

			if (definition?.BaseType != null && definition.BaseType.Name != nameof(Object) && definition.BaseType.Name != nameof(Enum))
			{
				declaration.BaseList.Add(new TypeDefinition()
				{
					Namespace = definition.BaseType.Namespace,
					Name = definition.BaseType.Name
				});
			}

			if (definition.HasInterfaces)
				foreach (InterfaceImplementation item in definition.Interfaces)
				{
					declaration.BaseList.Add(new TypeDefinition
					{
						Namespace = item.InterfaceType.Namespace,
						Name = item.InterfaceType.Name
					});
				}

			if (definition.IsEnum) declaration.Traits |= Trait.Enum;
			if (definition.IsPublic) declaration.Traits |= Trait.Public;
			if (definition.IsInterface) declaration.Traits |= Trait.Interface;
			if (definition.IsAbstract && !definition.IsInterface) declaration.Traits |= Trait.Abstract;

			if (definition.HasFields)
				foreach (FieldDefinition item in definition.Fields.Where(x => x.IsPublic && !x.IsSpecialName))
					declaration.Add(AsMemberDeclaration(item));

			if (definition.HasProperties)
				foreach (PropertyDefinition item in definition.Properties.Where(x => !x.IsSpecialName))
					declaration.Add(AsMemberDeclaration(item));

			return declaration;
		}

		internal static MemberDeclaration AsMemberDeclaration(FieldDefinition definition)
		{
			var declaration = new MemberDeclaration(definition.Name, new TypeDefinition());
			declaration.Type.Namespace = definition.FieldType.Namespace;
			declaration.Type.Name = definition.FieldType.FullName.Replace(definition.FieldType.Namespace, string.Empty).Trim('.', ' ');
			declaration.DefaultValue = definition.Constant;

			if (definition.IsPublic) declaration.Traits |= Trait.Public;
			if (definition.HasConstant) declaration.Traits |= Trait.Constant;
			if (definition.FieldType.IsPrimitive) declaration.Type.Traits |= Trait.Primitive;
			if (definition.FieldType.IsArray || Pattern.EnumerableType.IsMatch(definition.FieldType.FullName)) declaration.Type.Traits |= Trait.Array;

			return declaration;
		}

		internal static MemberDeclaration AsMemberDeclaration(PropertyDefinition definition)
		{
			var declaration = new MemberDeclaration(definition.Name, new TypeDefinition());
			declaration.Type.Namespace = definition.PropertyType.Namespace;
			declaration.Type.Name = definition.PropertyType.FullName;
			if (!string.IsNullOrEmpty(declaration.Type.Namespace)) declaration.Type.Name = definition.PropertyType.Name.Replace(definition.PropertyType.Namespace, string.Empty).Trim('.', ' ');

			declaration.DefaultValue = definition.Constant;
			declaration.Traits |= Trait.Public;

			if (definition.PropertyType.IsPrimitive) declaration.Type.Traits |= Trait.Primitive;
			if (definition.PropertyType.IsArray || Pattern.EnumerableType.IsMatch(definition.PropertyType.FullName)) declaration.Type.Traits |= Trait.Array;

			if (definition.DeclaringType.HasInterfaces)
				foreach (InterfaceImplementation item in definition.DeclaringType.Interfaces)
				{
					if (definition.Name.Contains(item.InterfaceType.Name + '.'))
					{
						declaration.Traits |= Trait.Interface;
					}
				}

			return declaration;
		}
	}
}
