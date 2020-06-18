using System.Linq;

namespace Acklann.TSBuild.CodeGeneration.Generators
{
	internal class WriterContext
	{
		public string Indent
		{
			get => string.Concat(Enumerable.Repeat('\t', _depth));
		}

		public void PushIndent()
		{
			_depth++;
		}

		public void PopIndent()
		{
			_depth--;
		}

		private int _depth;
	}
}
