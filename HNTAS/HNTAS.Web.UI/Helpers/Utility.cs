using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Soa;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace HNTAS.Web.UI.Helpers
{
    public static class Utility
    {
        public static void ShowBackButton(this Controller controller, string action, string controllerName)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action, controllerName);
        }
        public static void ShowBackButton(this Controller controller, string action)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action);
        }
        public static void ShowBackButton(this Controller controller, string action, string controllerName, object? routeValues = null)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action, controllerName, routeValues);
        }

        public static void ShowBackButton(this Controller controller, string action, string controllerName, string fragement)
        {
            controller.ViewBag.ShowBackButton = true;
            controller.ViewBag.BackLinkUrl = controller.Url.Action(action, controllerName, "", "", "", fragement);
        }

        public static string CapitalizeCommaSeparated(string input)
        {

            if (string.IsNullOrWhiteSpace(input))
                return input;

            var words = input.Split(',')
                             .Select(w => w.Trim())
                             .Where(w => !string.IsNullOrEmpty(w))
                             .ToList();

            if (words.Count == 0)
                return string.Empty;

            var processedWords = words.Select((w, index) =>
                index == words.Count - 1
                    ? w.ToUpper()
                    : char.ToUpper(w[0]) + w.Substring(1).ToLower());

            return string.Join(", ", processedWords);

        }

        public static List<HeatNetworkElementOption> GetElementOptions()
        {
            return new List<HeatNetworkElementOption>
            {
                new() { Id = HeatNetworkElementType.EnergyCentre, Label = "Energy centre", Hint = "Only 1 allowed per heat network unless part of a closed loop." },
                new() { Id = HeatNetworkElementType.ConsumerConnection, Label = "Consumer connections" },
            };
        }

        public static List<SelectItemOption> GetContributorSelectList(string userRole)
        {
            if (userRole == UserRole.ResponsibleParty.ToString() || userRole == UserRole.NetworkManager.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.DesignatedDutyHolder).ToString(), Text = "Designated duty holder" },
                        new SelectItemOption { Value = ((int)ContributorRole.Contributor).ToString(), Text = " Contributor" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
            }
            else if (userRole == ContributorRole.DesignatedDutyHolder.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.Contributor).ToString(), Text = "Contributor" },
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
            }            
            else if (userRole == ContributorRole.Assessor.ToString())
            {
                return new List<SelectItemOption>
                    {
                        new SelectItemOption { Value = ((int)ContributorRole.Assessor).ToString(), Text = "Assessor" }
                    };
            }
            else
            {
                return null;
            }
        }

        public static async Task<string> GetUserRoleByUserHNMapping(UserResponse user, string hnId)
        {
            var userRole = "";
            if (user?.Roles?.Contains(UserRole.ResponsibleParty) == true)
            {
                userRole = UserRole.ResponsibleParty.ToString();
            }
            else if (user?.Roles?.Contains(UserRole.NetworkManager) == true)
            {
                userRole = UserRole.NetworkManager.ToString();
            }
            else
            {
                foreach (var mapping in user.HnRoleMappings)
                {
                    if (mapping.HnId == hnId)
                    {
                        userRole = mapping.Role.ToString();
                    }
                }
            }
            return userRole;
        }

        public static async Task<List<SelectItemOption>?> GetHeatNetworkSelectListAsync(List<HeatNetworkUserResponse> response)
        {
            //var response = await _userService.GetUserHeatNetworks(userId);
            if (response == null) return null;

            return response.Select(hn => new SelectItemOption
            {
                Value = hn.HnId,
                Text = $"{hn.HnId} - {hn.Name}"
            }).ToList();
        }

        /// <summary>
        /// Validates that input is a comma-separated integer list, e.g., "10" or "10, 11".
        /// Parses to List<int>.
        /// </summary>
        public static bool TryParseIntArray(string? input, out List<int> values, out string error)
        {
            values = new List<int>();
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                error = "Enter integers separated by commas, e.g., 10 or 10, 11.";
                return false;
            }

            var parts = input.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                error = "Enter integers separated by commas, e.g., 10 or 10, 11.";
                return false;
            }

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    error = "Found an empty value. Use format like 10 or 10, 11 (no trailing commas).";
                    return false;
                }

                // Integer-only
                if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                {
                    error = $"'{part}' is not a valid integer. Use format like 10 or 10, 11.";
                    return false;
                }

                values.Add(i);
            }

            return true;
        }

        /// <summary>
        /// Validates that input is a comma-separated number list (ints or decimals), e.g., "10", "10, 11.5".
        /// Parses to List<decimal> for precision.
        /// </summary>
        /// <remarks>
        /// Uses invariant culture: decimal point is '.' (e.g., 11.5). Thousands separators are not allowed.
        /// </remarks>
        public static bool TryParseIntNumber(string? input, out List<decimal> values, out string error)
        {
            values = new List<decimal>();
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                error = "Required. Enter numbers separated by commas, e.g., 10 or 10, 11.5.";
                return false;
            }

            var parts = input.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                error = "Enter numbers separated by commas, e.g., 10 or 10, 11.5.";
                return false;
            }

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    error = "Found an empty value. Use format like 10 or 10, 11.5 (no trailing commas).";
                    return false;
                }

                // Numbers: integers or decimals. InvariantCulture with '.' as decimal separator.
                if (!decimal.TryParse(part, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
                {
                    error = $"'{part}' is not a valid number. Use format like 10 or 10, 11.5 (decimal point is '.').";
                    return false;
                }

                values.Add(d);
            }

            return true;
        }
    }
}