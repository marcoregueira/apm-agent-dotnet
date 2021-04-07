using Elastic.Apm.Config;
using Newtonsoft.Json;

namespace Elastic.Apm.Report.Serialization
{
	internal class PayloadItemSerializer2
	{
		private readonly JsonSerializerSettings _settings;

		internal PayloadItemSerializer2(IConfigurationReader configurationReader) =>
			_settings = new JsonSerializerSettings
			{
				ContractResolver = new ElasticApmContractResolver(configurationReader),
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.None,
			};

		internal string SerializeObject(object item) =>
			JsonConvert.SerializeObject(item, _settings);
	}
}
