using HNTAS.Web.UI.Models.CompaniesHouse;
using System.Net;

namespace HNTAS.Web.UI.Helpers
{
    public static class AddressFormatter
    {
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
