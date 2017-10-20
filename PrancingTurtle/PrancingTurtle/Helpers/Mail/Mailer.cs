using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;
using Logging;

namespace PrancingTurtle.Helpers.Mail
{
    static class Mailer
    {

        private static readonly ILogger Logger = new NLogHandler();

        /// <summary>
        /// It's still blocking, but no on the same thread as the web request, so it appears to happen instantly.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        internal static void SendEmail_NoWait(string to, string subject, string body)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    using (var client = new SmtpClient(AccountInfo.Server, AccountInfo.Port))
                    {
                        client.Credentials = new NetworkCredential(AccountInfo.Username, AccountInfo.Password);
                        client.EnableSsl = AccountInfo.UseSsl;
                        client.Send($"{MailCommonStrings.Sender} <{AccountInfo.SendingAddress}>", to, subject, body);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Logger.Debug($"An email to {to} failed to send! {ex.Message}: {ex.InnerException.Message}");
                    }
                    else
                    {
                        Logger.Debug($"An email to {to} failed to send! {ex.Message}");
                        Logger.Debug($"An email to {to} failed to send! {ex.Data}");
                    }
                }
            });
        }
    }
}