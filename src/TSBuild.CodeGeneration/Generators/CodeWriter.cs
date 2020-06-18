using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.TSBuild.CodeGeneration.Generators
{
	public class CodeWriter : StreamWriter
	{
		public CodeWriter(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public void EmitLine(string content)
		{
			base.WriteLine(string.Concat(Indent, content));
		}

		public string Indent
		{
			get => string.Concat(Enumerable.Repeat('\t', _depth)) ?? string.Empty;
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
