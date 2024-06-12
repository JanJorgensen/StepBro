using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Utils
{
    [Public]
    public class MailSender
    {
        public string ShownFromMailAddress { get; set; }
        public string ToMailAddress { get; set; }
        public string AccountMailAddress { get; set; } = null;
        public string AccountPassword { get; set; } = null;
        public string SubjectOfMessage { get; set; } = "Mail from StepBro";
        public string BodyOfMessage { get; set; } = "StepBro has sent you a message";
        public bool IsBodyHtml { get; set; } = false;
        public string SmtpServer { get; set; } = "smtp-mail.outlook.com"; // So default credentials on windows machines work
        public long SmtpPort { get; set; } = 587; // Default port for most SMTP Servers
        public bool EnableSsl { get; set; } = true;

        public MailSender(string shownFromMailAddress, string toMailAddress)
        {
            ShownFromMailAddress = shownFromMailAddress;
            ToMailAddress = toMailAddress;
        }

        public MailSender(string fromMailAddress, string toMailAddress, string accountPassword) : this(fromMailAddress, toMailAddress)
        {
            AccountMailAddress = fromMailAddress;
            AccountPassword = accountPassword;
        }

        public MailSender(string shownFromMailAddress, string toMailAddress, string accountMailAddress, string accountPassword) : this(shownFromMailAddress, toMailAddress)
        {
            AccountMailAddress = accountMailAddress;
            AccountPassword = accountPassword;
        }

        public MailSender(
            string shownFromMailAddress,
            string toMailAddress,
            string accountMailAddress,
            string accountPassword,
            string subjectOfMessage,
            string bodyOfMessage,
            bool isBodyHtml,
            string smtpServer,
            long smtpPort,
            bool enableSsl) : this(shownFromMailAddress, toMailAddress)
        {
            AccountMailAddress = accountMailAddress;
            AccountPassword = accountPassword;
            SubjectOfMessage = subjectOfMessage;
            BodyOfMessage = bodyOfMessage;
            IsBodyHtml = isBodyHtml;
            SmtpServer = smtpServer;
            SmtpPort = smtpPort;
            EnableSsl = enableSsl;
        }

        public bool SendMail([Implicit] ICallContext context, string subject, string body)
        {
            SubjectOfMessage = subject;
            BodyOfMessage = body;
            return SendMail(context);
        }

        public bool SendMail([Implicit] ICallContext context)
        {
            bool result = true;

            using (var message = new MailMessage())
            {
                message.To.Add(new MailAddress(ToMailAddress, ToMailAddress));
                message.From = new MailAddress(ShownFromMailAddress, ShownFromMailAddress);
                message.Subject = SubjectOfMessage;
                message.Body = BodyOfMessage;
                message.IsBodyHtml = IsBodyHtml; // change to true if body msg is in html

                using (var client = new SmtpClient(SmtpServer))
                {
                    if (AccountMailAddress == null || AccountPassword == null)
                    {
                        client.UseDefaultCredentials = true;
                    }
                    else
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(AccountMailAddress, AccountPassword);
                    }

                    client.Port = (int)SmtpPort;
                    client.EnableSsl = EnableSsl;

                    try
                    {
                        client.Send(message); // Email sent
                    }
                    catch (Exception e)
                    {
                        // Email not sent, log exception
                        context.ReportError(e.Message);
                        result = false;
                    }
                }
            }

            return result;
        }
    }
}
