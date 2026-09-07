using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace HNTAS.Web.UI.Controllers
{
    public class KpiDashboardController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IArmsDashboardService _armsDashboardService;

        public KpiDashboardController(ISessionHelper sessionHelper, IArmsDashboardService armsDashboardService)
        {
            _sessionHelper = sessionHelper;
            _armsDashboardService = armsDashboardService;
        }

        public async Task<IActionResult> Index(string? searchTerm, int? month, int? year, int page = 1)
        {
            var isSuperUser = _sessionHelper.GetFromSession<bool?>(HttpContext, SessionKeys.IsSuperUserKey);

            ViewBag.isSuperUser = isSuperUser ?? false;

            ModelState.Clear();

            // 1. Handle Defaults
            int filterYear = year ?? DateTime.Now.Year;

            // Get the current logged-in User ID (assuming available via User context)
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

            // 2. Call the API Service
            // We pass the filter criteria directly to our new service wrapper
            var dashboardData = await _armsDashboardService.GetKpiNetworksByRpUser(
                userId,
                month,
                filterYear,
                page);

            if (dashboardData == null)
            {
                // Handle empty state or error
                return View(new NetworkListViewModel { SelectedYear = filterYear, SelectedMonth = month });
            }

            // 3. Local Search/Filter (if the API doesn't handle searchTerm server-side yet)
            var filteredItems = dashboardData.Items;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredItems = filteredItems.Where(n =>
                    n.NetworkName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    n.HnId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // 4. Map to ViewModel
            var viewModel = new NetworkListViewModel
            {
                SearchTerm = searchTerm,
                SelectedMonth = month, // Maintains the "null" state for the dropdown if preferred
                SelectedYear = filterYear,

                // Map API response rows to your existing ViewModels
                Networks = filteredItems.Select(n => new HeatNetworkRowViewModel
                {
                    Hnid = n.HnId,
                    Name = n.NetworkName,
                    Provider = n.Provider,
                    DataPeriod = n.DataPeriod, // Assuming DataPeriod is a DateTime
                    SubmissionId = n.SubmissionId // Added so the View can link to the Details page
                }).ToList(),

                // Pagination Metadata from API
                CurrentPage = dashboardData.CurrentPage,
                TotalPages = dashboardData.TotalPages,
                TotalCount = dashboardData.TotalCount
            };

            return View(viewModel);
        }


        public async Task<IActionResult> Details(int? month, int? year, string? submissionId, List<string>? statusFilter, List<string>? typeFilter, int page = 1)
        {
            int pageSize = 10;

            // 1. Safety check for the submission ID
            if (string.IsNullOrEmpty(submissionId))
            {
                return RedirectToAction("Index");
            }

            // 2. Call the API Service using the specific submission ID
            // We pass the status filters and page number to the API for server-side processing
            var response = await _armsDashboardService.GetKpiNetworkDetails(submissionId, statusFilter, typeFilter, page);

            int filterYear = (int)(year ?? response.SelectedYear);
            int filterMonth = (int)(month ?? response.SelectedMonth);

            if (response == null)
            {
                return NotFound();
            }

            // 3. Map the API response to your ViewModel
            var viewModel = new ArmsDetailsViewModel
            {
                SubmissionId = submissionId, // Maintain the submission ID for context
                Hnid = response.HnId,
                NetworkName = response.NetworkName,
                SelectedMonth = filterMonth,
                SelectedYear = filterYear,

                // Pass the filtered and grouped results from the API
                StatusFilter = statusFilter ?? new List<string>(),
                TypeFilter = typeFilter ?? new List<string>(),

                // Paging metadata provided by the API
                CurrentPage = response.CurrentPage.Value,
                TotalPages = response.TotalPages.Value,
                TotalElements = response.TotalElements.Value,
                PageSize = pageSize,

                // Maintain the filter state for the back button
                BackToListUrl = Url.Action("Index", new { month, year }),

                // NEW: Root level assignment for Carbon Emission metrics
                TotalCarbonEmission = (decimal?)response.TotalCarbonEmission,
                CarbonCalculationInputs = response.CarbonCalculationInputs?.ToDictionary(
                                        kvp => kvp.Key,
                                        kvp => new CarbonInputUiDisplayViewModel
                                        {
                                            Label = kvp.Value.Label,
                                            Value = (double)kvp.Value.Value,
                                            Unit = kvp.Value.Unit
                                        })
            };


            viewModel.GroupedElements = response.GroupedElements.ToDictionary(
                group => $"{group.ElementType}|{group.ElementId}",
                group => new ElementGroupViewModel
                {
                    KpiRows = group.Kpis.Select(k => new KpiRowViewModel
                    {
                        KpiId = k.KpiName,
                        Value = k.Value.Value,
                        Status = k.Status,
                        Unit = k.Unit
                    }).ToList()
                }
            );


            if (response.AggregatedKpis != null)
            {
                viewModel.AggregatedKpis = response.AggregatedKpis.Select(k => new KpiRowViewModel
                {
                    KpiId = k.KpiName,
                    Value = k.Value.Value,
                    Status = k.Status,
                    Unit = k.Unit
                }).ToList();
            }

            // Fetch history on the server
            viewModel.AuditHistory = await _armsDashboardService.GetSubmissionHistory(submissionId);

            this.ShowBackButton("Index", "KpiDashboard", new { month, year });

            return View(viewModel);
        }

        public class NetworkListViewModel
        {
            public string? SearchTerm { get; set; } = string.Empty;
            public int? SelectedMonth { get; set; }
            public int SelectedYear { get; set; }
            public List<HeatNetworkRowViewModel> Networks { get; set; } = new();

            // Dropdown helpers
            public List<int> AvailableYears => Enumerable.Range(2022, (DateTime.Now.Year - 2022) + 1).OrderByDescending(y => y).ToList();
            public Dictionary<int, string> Months => new()
            {
                { 1, "January" }, { 2, "February" }, { 3, "March" }, { 4, "April" },
                { 5, "May" }, { 6, "June" }, { 7, "July" }, { 8, "August" },
                { 9, "September" }, { 10, "October" }, { 11, "November" }, { 12, "December" }
            };

            public int? CurrentPage { get; set; }
            public int? TotalPages { get; set; }
            public int? TotalCount { get; set; }
        }

        public class ArmsDetailsViewModel
        {
            // Network Info
            public string SubmissionId { get; set; } = string.Empty; // Added to maintain context for the details page
            public string Hnid { get; set; } = string.Empty;
            public string NetworkName { get; set; } = string.Empty;

            // Selection Context
            public int SelectedMonth { get; set; }
            public int SelectedYear { get; set; }
            public List<string> StatusFilter { get; set; } = new();
            public List<string> TypeFilter { get; set; } = new();

            // The Data: Grouped by Element ID
            public Dictionary<string, ElementGroupViewModel> GroupedElements { get; set; } = new();

            public List<KpiRowViewModel> AggregatedKpis { get; set; } = new();

            // Helper for the "Back" link
            public string BackToListUrl { get; set; } = string.Empty;


            // Pagination Properties
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public int TotalElements { get; set; }
            public int PageSize { get; set; } = 10; // Default to 10 per page

            public int FromRecord => ((CurrentPage - 1) * PageSize) + 1;
            public int ToRecord => Math.Min(CurrentPage * PageSize, TotalElements);

            public bool HasPreviousPage => CurrentPage > 1;
            public bool HasNextPage => CurrentPage < TotalPages;

            public List<KpiHistoryResponse?> AuditHistory { get; set; } = new(); // Placeholder for any audit history data you might want to display


            public decimal? TotalCarbonEmission { get; set; }

            public Dictionary<string, CarbonInputUiDisplayViewModel>? CarbonCalculationInputs { get; set; }
        }

        public class CarbonInputUiDisplayViewModel
        {
            public string Label { get; set; } = null!;
            public double Value { get; set; }

            public string? Unit { get; set; }
        }

        public class HeatNetworkRowViewModel
        {
            public string Hnid { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Provider { get; set; } = string.Empty;

            public string SubmissionId { get; set; } = string.Empty; // Added to link to details page
            public string DataPeriod { get; set; } = string.Empty; // The "Reporting Period" column
        }

        [ExcludeFromCodeCoverage]
        public class HeatNetworkStaticData
        {
            public string Hnid { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Provider { get; set; } = string.Empty;

            public DateTime ReportingPeriod { get; set; }

            // Helper property for the sidebar display
            public string DisplayName => $"{Hnid} ({Name})";
        }

        [ExcludeFromCodeCoverage]
        public class ArmsDashboardViewModel
        {
            // Core Identification
            public string Hnid { get; set; } = string.Empty;
            public string NetworkName { get; set; } = string.Empty;
            public string ArmsProvider { get; set; } = string.Empty;

            // Side Panel Search & Navigation
            public string SearchTerm { get; set; } = string.Empty;
            public List<HeatNetworkStaticData> AllNetworks { get; set; } = new();

            // Period Filtering (Monthly Database Posts)
            public int SelectedMonth { get; set; }
            public int SelectedYear { get; set; }

            public List<int> AvailableYears => Enumerable.Range(2022, (DateTime.Now.Year - 2022) + 1)
                                                         .OrderByDescending(y => y).ToList();

            public Dictionary<int, string> Months => new()
            {
                { 1, "January" }, { 2, "February" }, { 3, "March" }, { 4, "April" },
                { 5, "May" }, { 6, "June" }, { 7, "July" }, { 8, "August" },
                { 9, "September" }, { 10, "October" }, { 11, "November" }, { 12, "December" }
            };

            // KPI Status Filtering
            public List<string> StatusFilter { get; set; } = new();

            // Main Data: Grouped by Element ID
            public Dictionary<string, List<KpiRowViewModel>> GroupedElements { get; set; } = new();

            // Pagination & Counters
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public int TotalElements { get; set; } // Added back
            public int FromRecord { get; set; }
            public int ToRecord { get; set; }

            public bool HasPreviousPage => CurrentPage > 1;
            public bool HasNextPage => CurrentPage < TotalPages;
        }

        public class ElementGroupViewModel
        {
            public List<KpiRowViewModel> KpiRows { get; set; } = new();
        }

        public class KpiRowViewModel
        {
            public string KpiId { get; set; }
            public double Value { get; set; }
            public double Threshold { get; set; }
            public double LowerLimit { get; set; }
            public double UpperLimit { get; set; }
            public string Status { get; set; }
            public string? Unit { get; set; }
            //public DateTime ReportingPeriod { get; internal set; }
        }

        public enum KPIAssessmentStatus
        {
            [Description("Undefined")]
            /// <summary>
            /// when the rule has not been applied to the value, or if the value is missing/invalid and cannot be assessed.
            /// </summary>
            Undefined = 0,

            [Description("Pass")]
            /// <summary>
            /// Value is within limits and meets/exceeds the target threshold.
            /// </summary>
            Pass = 1,

            [Description("Fail")]
            /// <summary>
            /// Value is within limits but does not meet the target threshold.
            /// </summary>
            Fail = 2,

            [Description("Outside Limit")]
            /// <summary>
            /// Value is physically or logically outside the allowed Upper/Lower bounds.
            /// </summary>
            OutsideLimit = 3
        }
    }
}
