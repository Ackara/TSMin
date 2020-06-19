using System.Collections;
using System.Collections.Generic;

namespace Acklann.TSBuild.CodeGeneration
{
	partial class TypeDefinition
	{
		internal class Enumerator : IEnumerator<TypeDefinition>
		{
			public Enumerator(TypeDefinition[] definitions)
			{
				n = definitions.Length;
				_visited = new string[n];
				_definitions = definitions;
			}

			private readonly TypeDefinition[] _definitions;
			private string[] _visited;
			private int v = 0, n = 0, a = 0;
			private TypeDefinition _current, _anchor;

			object IEnumerator.Current => _current;
			public TypeDefinition Current => _current;

			public bool MoveNext()
			{
				if (v < n && a < n)
				{
				top:
					_anchor = _definitions[a];
					_current = GetDependency(_anchor);

					if (_current == _anchor)
					{
						while ((v < n && a < n) && WasVisited(_current.FullName))
						{
							a++;
							_current = _definitions[a];
						}
						if (_current != _anchor) goto top;
					}

					_visited[v++] = _current.FullName;
					return true;
				}

				return false;
			}

			public void Reset()
			{
				a = v = 0;
				n = _definitions.Length;
				_visited = new string[n];
			}

			public void Dispose()
			{
			}

			private TypeDefinition GetDependency(TypeDefinition type, TypeDefinition caller = null)
			{
				foreach (var item in type.BaseList)
				{
					if (item.InScope && !WasVisited(item.FullName))
					{
						if (item == caller) return caller;// protection against circular reference.
						else if (type != item) return GetDependency(item, type);
					}
				}

				foreach (var item in type.Members)
				{
					if (item.Type.InScope && !WasVisited(item.Type.FullName))
					{
						if (item.Type == caller) return caller;// protection against circular reference.
						else if (type != item.Type) return GetDependency(item.Type, type);
					}
				}

				return type;
			}

			private bool WasVisited(string name)
			{
				for (int i = 0; i < _visited.Length; i++)
					if (_visited[i] == name)
					{
						return true;
					}

				return false;
			}
		}

		internal class Enumerable : IEnumerable<TypeDefinition>
		{
			public Enumerable(TypeDefinition[] declarations)
			{
				_instance = new Enumerator(declarations);
			}

			private readonly Enumerator _instance;

			public IEnumerator<TypeDefinition> GetEnumerator() => _instance;

			IEnumerator IEnumerable.GetEnumerator() => _instance;
		}
	}
}
