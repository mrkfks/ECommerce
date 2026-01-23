using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dashboard.Web.Helpers
{
    [HtmlTargetElement(Attributes = "is-active-route")]
    public class ActiveRouteTagHelper : TagHelper
    {
        private IDictionary<string, string> _routeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        [HtmlAttributeName("asp-action")]
        public string? Action { get; set; }

        [HtmlAttributeName("asp-controller")]
        public string? Controller { get; set; }

        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues
        {
            get
            {
                return this._routeValues;
            }
            set
            {
                this._routeValues = value;
            }
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; } = null!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (ShouldBeActive())
            {
                MakeActive(output);
            }

            output.Attributes.RemoveAll("is-active-route");
        }

        private bool ShouldBeActive()
        {
            if (ViewContext?.RouteData?.Values == null)
            {
                return false;
            }
            string? currentController = ViewContext.RouteData.Values["Controller"]?.ToString();
            string? currentAction = ViewContext.RouteData.Values["Action"]?.ToString();

            if (!string.IsNullOrWhiteSpace(Controller) && !string.Equals(Controller, currentController, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Action) && !string.Equals(Action, currentAction, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var values = ViewContext.RouteData.Values;
            foreach (KeyValuePair<string, string> routeValue in RouteValues)
            {
                if (!values.TryGetValue(routeValue.Key, out var valObj))
                {
                    return false;
                }

                if (valObj == null)
                {
                    if (routeValue.Value != null)
                        return false;
                }
                else
                {
                    if (valObj.ToString() != routeValue.Value)
                        return false;
                }
            }

            return true;
        }

        private void MakeActive(TagHelperOutput output)
        {
            var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
            if (classAttr == null)
            {
                output.Attributes.Add("class", "active");
                return;
            }

            var current = classAttr.Value?.ToString() ?? string.Empty;
            if (!current.Contains("active", StringComparison.OrdinalIgnoreCase))
            {
                var newVal = string.IsNullOrWhiteSpace(current) ? "active" : current + " active";
                output.Attributes.SetAttribute("class", newVal);
            }
        }
    }
}
