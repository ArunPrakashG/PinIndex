using Newtonsoft.Json;
using PinIndex.India.Responses;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PinIndex.India {
	public sealed class PinIndexIndiaClient : IDisposable {
		private const int MAX_RETRY_COUNT = 3;
		private const string BASE_URL = "https://api.postalpincode.in/";
		private const string SEARCH_BY_BRANCH_NAME = "postoffice/";
		private const string SEARCH_BY_PIN_CODE = "pincode/";

		private readonly HttpClientHandler ClientHandler;
		private readonly HttpClient Client;
		private readonly SemaphoreSlim RequestSync = new SemaphoreSlim(1, 1);

		public PinIndexIndiaClient(HttpClientHandler clientHandler) {
			ClientHandler = clientHandler ?? throw new ArgumentNullException(nameof(clientHandler) + " cannot be null!");
			Client = new HttpClient(ClientHandler, true);
		}

		public PinIndexIndiaClient() {
			ClientHandler = new HttpClientHandler();
			Client = new HttpClient(ClientHandler, true);
		}

		public Response RequestByBranchName(string branchName, int retryCount = MAX_RETRY_COUNT) => RequestByBranchNameAsync(branchName, default, retryCount).Result;

		public Response RequestByPinCode(int pinCode, int retryCount = MAX_RETRY_COUNT) => RequestByPinCodeAsync(pinCode, default, retryCount).Result;

		public async Task<Response> RequestByBranchNameAsync(string branchName, CancellationToken cancellationToken, int retryCount = MAX_RETRY_COUNT) {
			if (string.IsNullOrEmpty(branchName)) {
				throw new ArgumentException(nameof(branchName) + " is invalid.");
			}
			
			Uri requestUri = new Uri(BASE_URL + SEARCH_BY_BRANCH_NAME + branchName);
			return await RequestAsync(requestUri, cancellationToken, retryCount).ConfigureAwait(false);
		}

		public async Task<Response> RequestByPinCodeAsync(int pinCode, CancellationToken cancellationToken, int retryCount = MAX_RETRY_COUNT) {
			if(pinCode <= 0) {
				throw new ArgumentException(nameof(pinCode) + " is invalid.");
			}
						
			Uri requestUri = new Uri(BASE_URL + SEARCH_BY_PIN_CODE + pinCode);
			return await RequestAsync(requestUri, cancellationToken, retryCount).ConfigureAwait(false);
		}

		private async Task<Response> RequestAsync(Uri requestUri, CancellationToken cancellationToken, int retryCount) {
			await RequestSync.WaitAsync().ConfigureAwait(false);

			try {
				async Task<Response> requestAction() {
					for (int i = 0; i < retryCount; i++) {
						using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri)) {
							using (HttpResponseMessage response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false)) {
								if (response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.RequestTimeout) {
									continue;
								}

								string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

								if (string.IsNullOrEmpty(responseString)) {
									continue;
								}

								Response reqResponse = JsonConvert.DeserializeObject<Response>(responseString);
								reqResponse.RequestStatusCode = response.StatusCode;
								return reqResponse;
							}
						}
					}

					return default;
				}

				return await Task.Run(requestAction, cancellationToken).ConfigureAwait(false);
			}
			finally {
				RequestSync.Release();
			}
		}

		public void Dispose() {
			Client?.Dispose();
			RequestSync?.Dispose();
		}
	}
}
