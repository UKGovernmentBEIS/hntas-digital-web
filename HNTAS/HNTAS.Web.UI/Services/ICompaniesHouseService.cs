using HNTAS.Web.UI.Models.CompaniesHouse;

namespace HNTAS.Web.UI.Services
{
    public interface ICompaniesHouseService
    {
        /// <summary>
        /// Retrieves company details from Companies House API by company number.
        /// </summary>
        /// <param name="companyNumber">The 8-character company number.</param>
        /// <returns>A CompanyDetailsModel object if found, otherwise null.</returns>
        Task<CompanyDetailsModel?> GetCompanyByNumberAsync(string companyNumber);
    }
}
