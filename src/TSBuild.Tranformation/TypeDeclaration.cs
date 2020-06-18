namespace Acklann.TSBuild.Tranformation
{
	public class TypeDeclaration
	{
		public string Namespace { get; set; }

		public string Name { get; set; }

		public string FullName
		{
			get => string.Concat(
				Namespace,
				(string.IsNullOrEmpty(Namespace) ? '\0' : '.'),
				Name).Trim();
		}

		public bool IsPublic { get; set; }
	}
}
