using HNTAS.Web.UI.Models.Address;
using System.Text.Json;

namespace HNTAS.Web.UI.Services
{
    public class AddressLookupService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public AddressLookupService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = Environment.GetEnvironmentVariable("OS_API_KEY");
        }

        public async Task<AddressLookUpModel?> PostcodeLookupAsync(string postcode)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.os.uk/search/places/v1/postcode?postcode={postcode}&key={_apiKey}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("results", out var results))
            {
                return null;
            }

            var addressesInResults = doc.RootElement.GetProperty("results").EnumerateArray();

            // Simplify collection initialization
            var addresses = new List<string>();

            foreach (var addressResult in addressesInResults)
            {
                // Get the DPA property which contains the address details
                if (addressResult.TryGetProperty("DPA", out var dpa) && dpa.TryGetProperty("ADDRESS", out var addressElement))
                {
                    // Get the address as a string and add to the list
                    var addressString = addressElement.GetString();
                    if (!string.IsNullOrEmpty(addressString))
                    {
                        addresses.Add(addressString);
                    }
                }
            }

            string[] addressesArray = addresses.ToArray() ?? [];

            return new AddressLookUpModel { Postcode = postcode, Addresses = addressesArray };
        }
    }
}
