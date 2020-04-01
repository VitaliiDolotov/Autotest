using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CrowdpicAutomation.DTO;
using OpenPop.Mime;
using OpenPop.Pop3;

namespace Autotest.Helpers
{
    class EmailHelper
    {
        private static string CompleteRegistrationProcessMailSubject = "Complete Registration Process";

        public static string GetCompleteRegistrationUrl(UserDto user)
        {
            var mail = GetNewEmail(user, CompleteRegistrationProcessMailSubject);
            var mailContent = GetMailContent(mail);
            var url = GetUrlFromMail(mailContent);
            return url;
        }

        private static string GetUrlFromMail(string emailContent)
        {
            string pattern = @"(?:(?:https?|ftp|file):\/\/|www\.|ftp\.)(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.]*\)|[-A-Z0-9+&@#\/%=~_|$?!:,.])*(?:\([-A-Z0-9+&@#\/%=~_|$?!:,.]*\)|[A-Z0-9+&@#\/%=~_|$])";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(emailContent);

            try
            {
                return matches[0].ToString();
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to find complete registration link in the email content: {e}");
            }
        }

        private static string GetMailContent(Message message)
        {
            //Message content
            StringBuilder builder = new StringBuilder();
            OpenPop.Mime.MessagePart plainText = message.FindFirstPlainTextVersion();
            if (plainText != null)
            {
                // We found some plaintext!
                builder.Append(plainText.GetBodyAsText());
            }
            else
            {
                // Might include a part holding html instead
                OpenPop.Mime.MessagePart html = message.FindFirstHtmlVersion();
                if (html != null)
                {
                    // We found some html!
                    builder.Append(html.GetBodyAsText());
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Wait for new email that was sent during nearest 30 second with appropriate subject
        /// </summary>
        /// <param name="user"></param>
        /// <param name="expectedMailSubject"></param>
        /// <returns></returns>
        private static Message GetNewEmail(UserDto user, string expectedMailSubject)
        {
            int attempts = 30;
            //Message should be sent not early than 15 seconds ago
            var startWaitingTime = DateTime.Now.AddSeconds(-15).ToUniversalTime();
            //Get last message
            var message = GetLastMessage(user);
            //Wait for new messages
            while (message == null || message.Headers.DateSent < startWaitingTime ||
                   !message.Headers.Subject.Equals(expectedMailSubject))
            {
                if (message != null)
                {
                    System.Diagnostics.Trace.WriteLine(
                        $"Message time: {message.Headers.DateSent} vs now: {startWaitingTime}");
                    System.Diagnostics.Trace.WriteLine(message.Headers.DateSent < startWaitingTime);
                }

                Thread.Sleep(1000);
                attempts--;
                message = GetLastMessage(user);
                if (attempts < 1)
                    break;
            }

            if (message.Headers.DateSent < startWaitingTime)
                throw new Exception("Email was not received");

            return message;
        }

        private static Message GetLastMessage(UserDto user)
        {
            using (Pop3Client client = new Pop3Client())
            {
                try
                {
                    // Connect to the server
                    client.Connect("pop.gmail.com", 995, true);

                    // Authenticate ourselves towards the server
                    client.Authenticate(user.Email, user.Password);

                    // Get the number of messages in the inbox
                    int messageCount = client.GetMessageCount();

                    Message message = client.GetMessage(messageCount);

                    return message;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
    }
}
