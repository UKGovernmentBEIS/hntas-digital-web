using HNTAS.Web.UI.Models.CompaniesHouse;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration; // Ensure this is included
using System;

namespace HNTAS.Web.UI.Services
{
    public class CompaniesHouseService : ICompaniesHouseService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public CompaniesHouseService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = Environment.GetEnvironmentVariable("OS_API_KEY"); // Corrected indexing issue  

            // Set base address for HttpClient  
            _httpClient.BaseAddress = new Uri("https://api.company-information.service.gov.uk/");
        }

        public async Task<CompanyDetailsModel?> GetCompanyByNumberAsync(string companyNumber)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Companies House API key is not configured.");
            }

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"company/{companyNumber}"); // Use relative path since BaseAddress is set

            // Add Basic Authentication header
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_apiKey}:"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            try
            {
                var response = await _httpClient.SendAsync(request);

                // Handle 404 Not Found specifically, return null if company not found
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode(); // Throws for other HTTP error codes

                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Helper to safely get string values from JsonElement
                string? getPropertyString(JsonElement element, string propertyName)
                {
                    return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
                }

                var companyDetails = new CompanyDetailsModel
                {
                    Title = root.GetProperty("company_name").GetString(),
                };

                // Extract Registered Office Address
                if (root.TryGetProperty("registered_office_address", out var addressElement) && addressElement.ValueKind == JsonValueKind.Object)
                {
                    companyDetails.RegisteredOfficeAddress = new RegisteredOfficeAddressModel
                    {
                        AddressLine1 = getPropertyString(addressElement, "address_line_1"),
                        AddressLine2 = getPropertyString(addressElement, "address_line_2"),
                        Country = getPropertyString(addressElement, "country"),
                        Locality = getPropertyString(addressElement, "locality"),
                        PostalCode = getPropertyString(addressElement, "postal_code")
                    };
                }

                return companyDetails;
            }
            catch (HttpRequestException ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error calling Companies House API: {ex.Message}");
                throw; // Re-throw the exception for higher-level error handling
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing Companies House API response: {ex.Message}");
                throw; // Re-throw the exception
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                throw; // Re-throw the exception
            }
        }
    }
}

