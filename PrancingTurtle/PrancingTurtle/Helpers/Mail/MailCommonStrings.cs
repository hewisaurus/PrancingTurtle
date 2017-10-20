using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Logging;

namespace PrancingTurtle.Helpers.Mail
{
    static class MailCommonStrings
    {
        private static readonly ILogger Logger = new NLogHandler();

        internal const string Sender = "The Prancing Turtle";

        internal static string RegisteredBody(string link)
        {
            return "Thanks for registering at The Prancing Turtle! " +
                   "Please confirm this email address by clicking the following link." +
                   Environment.NewLine + Environment.NewLine + link;
        }
        internal const string RegisteredSubject = "Prancing Turtle account management - verification required (please confirm your email address)";

        internal static string PasswordResetBody(string ipAddress, string link)
        {
            return "We received a request from " + ipAddress + " to reset the password for your PrancingTurtle account." + Environment.NewLine +
                   "To continue the reset process, please click on the following link." + Environment.NewLine + Environment.NewLine +
                   link + Environment.NewLine + Environment.NewLine +
                   "If you did not request this password reset, consider changing the password to your email account. Your PrancingTurtle account password has not been modified.";
        }

        internal const string ResetPasswordSubject = "Prancing Turtle account management - password reset link requested";
    }
}