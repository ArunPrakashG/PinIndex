using Newtonsoft.Json;

namespace PinIndex.India.Responses {
	public class PostOffice {
		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public string Description { get; set; }

		[JsonProperty]
		public string BranchType { get; set; }

		[JsonProperty]
		public string DeliveryStatus { get; set; }

		[JsonProperty]
		public string Taluk { get; set; }

		[JsonProperty]
		public string Circle { get; set; }

		[JsonProperty]
		public string District { get; set; }

		[JsonProperty]
		public string Division { get; set; }

		[JsonProperty]
		public string Region { get; set; }

		[JsonProperty]
		public string State { get; set; }

		[JsonProperty]
		public string Country { get; set; }
	}
}
