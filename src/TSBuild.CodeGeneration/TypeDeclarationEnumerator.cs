using System.Collections;
using System.Collections.Generic;

namespace Acklann.TSBuild.CodeGeneration
{
	partial class TypeDeclaration
	{
		internal class Enumerator : IEnumerator<TypeDeclaration>
		{
			public Enumerator(TypeDeclaration[] declarations)
			{
				n = declarations.Length;
				_visited = new string[n];
				_declarations = declarations;
			}

			private readonly TypeDeclaration[] _declarations;
			private string[] _visited;
			private int v = 0, n = 0, a = 0;
			private TypeDeclaration _current, _anchor;

			object IEnumerator.Current => _current;
			public TypeDeclaration Current => _current;

			public bool MoveNext()
			{
				if (v < n && a < n)
				{
				top:
					_anchor = _declarations[a];
					_current = GetDependency(_anchor);

					if (_current == _anchor)
					{
						while ((v < n && a < n) && WasVisited(_current.FullName))
						{
							a++;
							_current = _declarations[a];
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
				n = _declarations.Length;
				_visited = new string[n];
			}

			public void Dispose()
			{
			}

			private TypeDeclaration GetDependency(TypeDeclaration type, TypeDeclaration caller = null)
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

		internal class Enumerable : IEnumerable<TypeDeclaration>
		{
			public Enumerable(TypeDeclaration[] declarations)
			{
				_instance = new Enumerator(declarations);
			}

			private readonly Enumerator _instance;

			public IEnumerator<TypeDeclaration> GetEnumerator() => _instance;

			IEnumerator IEnumerable.GetEnumerator() => _instance;
		}
	}
}
