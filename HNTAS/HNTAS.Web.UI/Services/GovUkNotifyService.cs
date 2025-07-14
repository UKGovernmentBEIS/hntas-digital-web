using Notify.Client;
using Notify.Models.Responses;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HNTAS.Web.UI.Services
{
    public class GovUkNotifyService
    {
        private readonly NotificationClient _notificationClient;
        private readonly string _apiKey;

        /// <summary>
        /// Initializes a new instance of the GovUkNotifyService.
        /// </summary>
        /// <param name="configuration">The application's configuration.</param>
        public GovUkNotifyService(IConfiguration configuration)
        {
            // Retrieve API key from configuration
            _apiKey = Environment.GetEnvironmentVariable("GOV_NOTIFY_API_KEY") ?? throw new ArgumentNullException(
                "GOV.UK Notify API key 'GovUkNotify:ApiKey' is not configured.");

            // Initialize the official GOV.UK Notify client
            _notificationClient = new NotificationClient(_apiKey);
        }

        /// <summary>
        /// Sends an email using the GOV.UK Notify API.
        /// </summary>
        /// <param name="emailAddress">The recipient's email address.</param>
        /// <param name="templateId">The ID of the email template to use.</param>
        /// <param name="personalisation">A dictionary of personalization fields for the template.</param>
        /// <param name="reference">An optional unique reference for this notification.</param>
        /// <returns>True if the email was sent successfully, false otherwise.</returns>
        public async Task<bool> SendEmailAsync(
            string emailAddress,
            string templateId,
            Dictionary<string, dynamic>? personalisation = null,
            string? reference = null)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ArgumentException("Email address cannot be null or empty.", nameof(emailAddress));
            }
            if (string.IsNullOrEmpty(templateId))
            {
                throw new ArgumentException("Template ID cannot be null or empty.", nameof(templateId));
            }

            try
            {
                // Use the official client to send the email
                EmailNotificationResponse response = await _notificationClient.SendEmailAsync(
                    emailAddress: emailAddress,
                    templateId: templateId,
                    personalisation: personalisation
                );

                Console.WriteLine($"Email sent successfully to {emailAddress}. Notification ID: {response.id}");
                return true;
            }
            catch (Notify.Exceptions.NotifyClientException ex)
            {
                Console.Error.WriteLine($"GOV.UK Notify client error sending email to {emailAddress}: {ex.Message}");
                // You might want to log ex.InnerException or ex.StackTrace for more details
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred while sending email to {emailAddress}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Example usage of the GovUkNotifyService.
        /// Note: This static method is for demonstration and would typically not be used
        /// when injecting the service via DI in a real ASP.NET Core app.
        /// </summary>
        public static async Task ExampleUsage()
        {
            Console.WriteLine("ExampleUsage is not directly runnable without IConfiguration. Please use dependency injection.");
            // For a test, you might do something like this (not for production):
            // var inMemorySettings = new Dictionary<string, string> {
            //     {"GovUkNotify:ApiKey", "your-api-key-here"}
            // };
            // IConfiguration config = new ConfigurationBuilder()
            //     .AddInMemoryCollection(inMemorySettings)
            //     .Build();
            // var notifyService = new GovUkNotifyService(config);
            // await notifyService.SendEmailAsync(...);
        }
    }
}

