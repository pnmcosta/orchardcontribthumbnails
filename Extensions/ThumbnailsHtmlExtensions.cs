using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Mvc.Html;
using Orchard.Mvc.Extensions;
using Contrib.Thumbnails.Services;
using System.Web.Routing;
namespace Contrib.Thumbnails.Extensions
{
    public static class ThumbnailsHtmlExtensions
    {
        public static MvcHtmlString Thumbnail(this HtmlHelper htmlHelper, string src, string alt, int width, int height, object htmlAttributes)
        {
            return htmlHelper.Thumbnail(src, alt, width, height, new RouteValueDictionary(htmlAttributes));
        }
        public static MvcHtmlString Thumbnail(this HtmlHelper htmlHelper, string src, string alt, int width, int height, IDictionary<string, object> htmlAttributes)
        {
            IThumbnailsService thumbnailsService = htmlHelper.GetWorkContext().Resolve<IThumbnailsService>();
            return htmlHelper.Image(thumbnailsService.GetThumbnail(src, width, height), alt, htmlAttributes);
        }
    }
}