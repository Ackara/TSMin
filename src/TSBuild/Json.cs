using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Acklann.TSBuild
{
	public class Json
	{
		public static void CopyJsonProperty(string sourceFile, string destinationFile, string sourceJPath, string destinationJPath = default)
		{
			if (!File.Exists(sourceFile)) throw new FileNotFoundException($"Could not find file at '{sourceFile}'.");
			if (string.IsNullOrEmpty(destinationFile)) throw new ArgumentNullException(nameof(destinationFile));
			if (string.IsNullOrEmpty(sourceJPath)) return;

			JProperty sourceProperty;
			using (var stream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var reader = new JsonTextReader(new StreamReader(stream)))
			{
				JObject src = JObject.Load(reader);
				sourceProperty = (JProperty)src.SelectToken(sourceJPath).Parent;
			}

			if (!File.Exists(destinationFile))
			{
				string folder = Path.GetDirectoryName(destinationFile);
				if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
				File.WriteAllText(destinationFile, "{}", System.Text.Encoding.UTF8);
			}

			JObject destination;
			using (var stream = new FileStream(destinationFile, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var reader = new JsonTextReader(new StreamReader(stream)))
			{
				destination = JObject.Load(reader);
				JProperty prop = (destinationJPath == default ?
					destination.Property(sourceProperty.Name)
					:
					(JProperty)destination.SelectToken(destinationJPath)?.Parent);

				switch (sourceProperty.Value.Type)
				{
					default:
					case JTokenType.Array:
					case JTokenType.String:
						if (prop == null)
							destination.Add(sourceProperty);
						else
							prop.Value = sourceProperty.Value;
						break;

					case JTokenType.Object:
						if (prop == null)
							destination.Merge(sourceProperty.Value);
						else
							prop.Merge(sourceProperty.Value);
						break;
				}
			}

			using (var stream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.Read))
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(destination.ToString(Formatting.Indented));
				writer.Flush();
			}
		}
	}
}
