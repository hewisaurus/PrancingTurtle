using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace PrancingTurtle.Helpers
{
    public static class MvcHtmlExtensions
    {
        /// <summary>
        /// Creates a link that will open a jQuery UI dialog form.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="linkText">The inner text of the anchor element</param>
        /// <param name="dialogContentUrl">The url that will return the content to be loaded into the dialog window</param>
        /// <param name="dialogTitle">The title to be displayed in the dialog window</param>
        /// <param name="updateTargetId">The id of the div that should be updated after the form submission</param>
        /// <param name="updateUrl">The url that will return the content to be loaded into the traget div</param>
        /// <returns></returns>
        public static MvcHtmlString DialogFormLink(this HtmlHelper htmlHelper, string linkText, string dialogContentUrl,
            string dialogId, string dialogTitle, string updateTargetId, string updateUrl)
        {
            TagBuilder builder = new TagBuilder("a");
            builder.SetInnerText(linkText);
            builder.Attributes.Add("href", dialogContentUrl);
            builder.Attributes.Add("data-dialog-title", dialogTitle);
            builder.Attributes.Add("data-update-target-id", updateTargetId);
            builder.Attributes.Add("data-update-url", updateUrl);

            // Add a css class named dialogLink that will be
            // used to identify the anchor tag and to wire up
            // the jQuery functions
            builder.AddCssClass("dialogLink");

            return new MvcHtmlString(builder.ToString());
        }

        public static MvcHtmlString ImageActionLink(this HtmlHelper htmlHelper, string linkClass, string imgClass, string title, string onClickContainer, string onClickBody, int itemId)
        {
            var html = string.Format("<i class=\"{0}\" title=\"{1}\" onclick=\"showModal('#{2}', '#{3}' , {4})\"></i>", imgClass, title, onClickContainer, onClickBody, itemId);
            return new MvcHtmlString(html);
        }

        public static MvcHtmlString ImageActionLink(this HtmlHelper htmlHelper, string action, string linkClass, string imgClass)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var url = urlHelper.Action(action);
            var html = string.Format("<a class=\"{0}\" href=\"{1}\"><i class=\"{2}\"></i></a>", linkClass, url, imgClass);
            return new MvcHtmlString(html);
        }

        public static MvcHtmlString ImageActionLink(this HtmlHelper htmlHelper, string action, string linkClass, string imgClass, string linkText)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var url = urlHelper.Action(action);
            //var html = string.Format("<a class=\"{0}\" href=\"{1}\"><i class=\"{2}\"></i> {3}</a>", linkClass, url, imgClass, linkText);
            var html = string.Format("<a class=\"{0}\" href=\"{1}\"><i class=\"{2}\"></i></a> <a class=\"{0}\" href=\"{1}\">{3}</a>", linkClass, url, imgClass, linkText);
            return new MvcHtmlString(html);
        }

        public static MvcHtmlString ImageActionLink(this HtmlHelper htmlHelper, string action, object routeValue, string linkClass, string imgClass)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var url = urlHelper.Action(action, routeValue);
            var html = string.Format("<a class=\"{0}\" href=\"{1}\"><i class=\"{2}\"></i></a>", linkClass, url, imgClass);
            return new MvcHtmlString(html);
        }

        public static MvcHtmlString ImageActionLink(this HtmlHelper htmlHelper, string action, string controller, object routeValue, string linkClass, string imgClass)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var url = urlHelper.Action(action, controller, routeValue);
            var html = string.Format("<a class=\"{0}\" href=\"{1}\"><i class=\"{2}\"></i></a>", linkClass, url, imgClass);
            return new MvcHtmlString(html);
        }

        public static MvcHtmlString ImageActionLink(this HtmlHelper htmlHelper, string action, string controller, object routeValue, string linkClass, string imgClass, string linkText, bool splitLinks = true)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var url = urlHelper.Action(action, controller, routeValue);
            //var html = string.Format("<a class=\"{0}\" href=\"{1}\"><i class=\"{2}\"></i> {3}</a>", linkClass, url, imgClass, linkText);
            var html = "";
            html = string.Format(splitLinks 
                ? "<a class=\"{0}\" href=\"{1}\"><i class=\"{2}\"></i></a> <a class=\"{0}\" href=\"{1}\">{3}</a>" 
                : "<a class=\"{0}\" href=\"{1}\"><i class=\"{2}\"></i> {3}</a>", linkClass, url, imgClass, linkText);
            return new MvcHtmlString(html);
        }

        public static MvcHtmlString ImageActionLink(this HtmlHelper htmlHelper, string url, string imgClass)
        {
            var html = string.Format("<a href=\"{0}\"><i class=\"{1}\"></i></a>", url, imgClass);
            return new MvcHtmlString(html);
        }

        public static MvcHtmlString ImageActionLinkPopup(this HtmlHelper htmlHelper, string controller, string action, string parameters, string imageClass, string style, string title)
        {
            var html = string.Format(
                        "<a href=\"#\"><i class=\"{3}\" style=\"{4}\" title=\"{5}\"" +
                                  "onclick=\"window.open('/{0}/{1}?{2}'," +
                        "'_blank', 'height=' + screen.height + ',width=' + screen.width + ',resizable=yes,scrollbars=yes,toolbar=yes,menubar=yes,location=yes')\"></i></a>",
                        controller, action, parameters, imageClass, style, title);
            return new MvcHtmlString(html);
        }

        public static IHtmlString AssemblyVersion(this HtmlHelper helper)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return MvcHtmlString.Create(version);
        }
    }
}