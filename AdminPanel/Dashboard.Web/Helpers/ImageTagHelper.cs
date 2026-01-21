using Microsoft.AspNetCore.Razor.TagHelpers;
using Dashboard.Web.Helpers;

namespace Dashboard.Web.TagHelpers;

// <img ecommerce-src="@item.ImageUrl" />
[HtmlTargetElement("img", Attributes = "ecommerce-src")]
public class ImageTagHelper : TagHelper
{
    [HtmlAttributeName("ecommerce-src")]
    public string? ImagePath { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var url = ImageHelper.GetImageUrl(ImagePath);
        output.Attributes.SetAttribute("src", url);
    }
}
