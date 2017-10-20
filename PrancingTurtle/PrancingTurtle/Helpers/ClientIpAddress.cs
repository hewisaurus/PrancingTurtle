using System.Web;

namespace PrancingTurtle.Helpers
{
    public class ClientIpAddress
    {
        public static string GetIPAddress(HttpRequestBase request)
        {
            var clientIp = request.UserHostAddress;
            var fwdFor = request.Headers["X-Forwarded-For"];
            if (fwdFor != null)
            {
                var fwdArray = fwdFor.Split(',');
                clientIp = fwdArray[0].Trim();
                //_logger.Debug(string.Format("Client forwarded by proxy, client IP is {0}", clientIp));
            }

            return clientIp;

            //string szRemoteAddr = request.UserHostAddress;
            //string szXForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];
            //string szIP = "";

            //if (szXForwardedFor == null)
            //{
            //    szIP = szRemoteAddr;
            //}
            //else
            //{
            //    szIP = szXForwardedFor;
            //    if (szIP.IndexOf(",") > 0)
            //    {
            //        string[] arIPs = szIP.Split(',');

            //        foreach (string item in arIPs)
            //        {
            //            //if (!isPrivateIP(item))
            //            //{
            //                return item;
            //            //}
            //        }
            //    }
            //}
            //return szIP;
        }
    }
}