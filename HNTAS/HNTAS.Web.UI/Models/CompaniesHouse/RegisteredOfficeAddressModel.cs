namespace HNTAS.Web.UI.Models.CompaniesHouse
{
    public class RegisteredOfficeAddressModel
    {
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Country { get; set; }
        public string? Locality { get; set; } // City/Town
        public string? PostalCode { get; set; }
    }
}
