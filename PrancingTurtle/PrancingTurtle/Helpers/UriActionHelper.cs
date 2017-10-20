using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;

namespace PrancingTurtle.Helpers
{
    public static class UriActionHelper
    {
        internal static Uri GetPublicFacingUrl(HttpRequestBase request, NameValueCollection serverVariables)
        {
            //Contract.Requires<ArgumentNullException>(request != null);
            //Contract.Requires<ArgumentNullException>(serverVariables != null);

            // Due to URL rewriting, cloud computing (i.e. Azure)
            // and web farms, etc., we have to be VERY careful about what
            // we consider the incoming URL.  We want to see the URL as it would
            // appear on the public-facing side of the hosting web site.
            // HttpRequest.Url gives us the internal URL in a cloud environment,
            // So we use a variable that (at least from what I can tell) gives us
            // the public URL:
            if (serverVariables["HTTP_HOST"] != null)
            {
                //ErrorUtilities.VerifySupported(request.Url.Scheme == Uri.UriSchemeHttps || request.Url.Scheme == Uri.UriSchemeHttp, "Only HTTP and HTTPS are supported protocols.");
                string scheme = serverVariables["HTTP_X_FORWARDED_PROTO"] ?? request.Url.Scheme;
                Uri hostAndPort = new Uri(scheme + Uri.SchemeDelimiter + serverVariables["HTTP_HOST"]);
                UriBuilder publicRequestUri = new UriBuilder(request.Url);
                publicRequestUri.Scheme = scheme;
                publicRequestUri.Host = hostAndPort.Host;
                publicRequestUri.Port = hostAndPort.Port; // CC missing Uri.Port contract that's on UriBuilder.Port
                return publicRequestUri.Uri;
            }
            // Failover to the method that works for non-web farm enviroments.
            // We use Request.Url for the full path to the server, and modify it
            // with Request.RawUrl to capture both the cookieless session "directory" if it exists
            // and the original path in case URL rewriting is going on.  We don't want to be
            // fooled by URL rewriting because we're comparing the actual URL with what's in
            // the return_to parameter in some cases.
            // Response.ApplyAppPathModifier(builder.Path) would have worked for the cookieless
            // session, but not the URL rewriting problem.
            return new Uri(request.Url, request.RawUrl);
        }
        public static string AbsoluteAction(this UrlHelper url, string actionName, string controllerName, object routeValues = null)
        {
            Uri publicFacingUrl = GetPublicFacingUrl(url.RequestContext.HttpContext.Request, url.RequestContext.HttpContext.Request.ServerVariables);
            string relAction = url.Action(actionName, controllerName, routeValues);
            //this will always have a / in front of it.
            var newPort = publicFacingUrl.Port == 80 || publicFacingUrl.Port == 443 ? "" : ":" + publicFacingUrl.Port.ToString();
            return publicFacingUrl.Scheme + Uri.SchemeDelimiter + publicFacingUrl.Host + newPort + relAction;
        }
    }
}