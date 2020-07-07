using Newtonsoft.Json;
using System.Net;

namespace PinIndex.India.Responses {
	public abstract class Response {
		[JsonIgnore]
		public HttpStatusCode RequestStatusCode { get; set; }

		[JsonIgnore]
		public bool IsSuccessResponse => !string.IsNullOrEmpty(Status) &&
			Status.Equals("Success", System.StringComparison.OrdinalIgnoreCase) && RequestStatusCode == HttpStatusCode.OK;

		[JsonProperty]
		public string Message { get; set; }

		[JsonProperty]
		public string Status { get; set; }

		[JsonProperty]
		public PostOffice[] PostOffice { get; set; }
	}
}
