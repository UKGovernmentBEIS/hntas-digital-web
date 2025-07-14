using HNTAS.Web.UI.Models.CompaniesHouse;
using System.Net;

namespace HNTAS.Web.UI.Helpers
{
    public static class StringFormatter
    {

        public static string ToTitleCaseSingleWord(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            // Convert the first character to uppercase and the rest to lowercase.
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        public static string FormatAddress(RegisteredOfficeAddressModel? address)
        {
            if (address == null)
            {
                return "";
            }

            var parts = new List<string?>();
            parts.Add(address.AddressLine1);
            parts.Add(address.AddressLine2);
            parts.Add(address.Locality);
            parts.Add(address.Country);
            parts.Add(address.PostalCode);

            return string.Join(", ", parts.Where(p => !string.IsNullOrEmpty(p)));
        }
    }
}
