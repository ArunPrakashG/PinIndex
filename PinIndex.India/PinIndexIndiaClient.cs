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

		/// <summary>
		///  Gets the total count of requests send from this instance of <see cref="PinIndexIndiaClient"/>.
		/// </summary>
		public int RequestsCount { get; private set; } = 0;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="clientHandler">The <see cref="HttpClientHandler"/></param>
		public PinIndexIndiaClient(HttpClientHandler clientHandler) {
			ClientHandler = clientHandler ?? throw new ArgumentNullException(nameof(clientHandler) + " cannot be null!");
			Client = new HttpClient(ClientHandler, true);
		}

		/// <summary>
		/// Constructor with default values.
		/// </summary>
		public PinIndexIndiaClient() {
			ClientHandler = new HttpClientHandler();
			Client = new HttpClient(ClientHandler, true);
		}

		/// <summary>
		/// sync method to send a request by branch name.
		/// </summary>
		/// <param name="branchName">the branch name.</param>
		/// <param name="retryCount">the maximum retry count.</param>
		/// <returns>The response.</returns>
		public Response RequestByBranchName(string branchName, int retryCount = MAX_RETRY_COUNT) => RequestByBranchNameAsync(branchName, default, retryCount).Result;

		/// <summary>
		/// sync method to send a request by pin code.
		/// </summary>
		/// <param name="pinCode">the pin code.</param>
		/// <param name="retryCount">the maximum retry count.</param>
		/// <returns>The response.</returns>
		public Response RequestByPinCode(int pinCode, int retryCount = MAX_RETRY_COUNT) => RequestByPinCodeAsync(pinCode, default, retryCount).Result;

		/// <summary>
		/// async method to send a request by branch name.
		/// </summary>
		/// <param name="branchName">the branch name.</param>
		/// <param name="cancellationToken">the cancellation token.</param>
		/// <param name="retryCount">the maximum retry count.</param>
		/// <returns>The response.</returns>
		public async Task<Response> RequestByBranchNameAsync(string branchName, CancellationToken cancellationToken, int retryCount = MAX_RETRY_COUNT) {
			if (string.IsNullOrEmpty(branchName)) {
				throw new ArgumentException(nameof(branchName) + " is invalid.");
			}
			
			Uri requestUri = new Uri(BASE_URL + SEARCH_BY_BRANCH_NAME + branchName);
			return await RequestAsync(requestUri, cancellationToken, retryCount).ConfigureAwait(false);
		}

		/// <summary>
		/// async method to send a request by pin code.
		/// </summary>
		/// <param name="pinCode">the pin code.</param>
		/// <param name="cancellationToken">the cancellation token.</param>
		/// <param name="retryCount">the maximum retry count.</param>
		/// <returns>The response.</returns>
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
				RequestsCount++;
			}
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		public void Dispose() {
			Client?.Dispose();
			RequestSync?.Dispose();
		}
	}
}
