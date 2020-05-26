using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using WebApp.Classes.Connector;

namespace WebApp.Classes
{
    public class MailUtil
    {
        public static QueryResult SendEmail(Connector.QueryParameter queryParameter)
        {
            QueryResult result = new QueryResult();

            try
            {
                char splitMail = ';';
                using (MailMessage mailMessage = new MailMessage())
                {
                    AppHttpHandlerEmailConfig mailCfg = AppHttpHandler.AppHttpHandlerConfig.Email;
                    string server = mailCfg.Server;
                    int port = mailCfg.Port;

                    string username = mailCfg.Username;
                    string password = mailCfg.Password;
                    string defaultEmail = mailCfg.SenderAddress;
                    NetworkCredential credential = new NetworkCredential(username, password);
                    if (queryParameter["MAIL_FROM"] != null && !string.IsNullOrEmpty(queryParameter["MAIL_FROM"].ToString()))
                    {
                        defaultEmail = queryParameter["MAIL_FROM"].ToString();
                    }
                    mailMessage.From = new MailAddress(defaultEmail);

                    if (queryParameter["MAIL_TO"] != null && !string.IsNullOrEmpty(queryParameter["MAIL_TO"].ToString()))
                    {
                        string[] arrTo = queryParameter["MAIL_TO"].ToString().Split(splitMail);
                        arrTo.ToList<string>().ForEach(t => mailMessage.To.Add(new MailAddress(t)));
                    }

                    if (queryParameter["MAIL_CC"] != null && !string.IsNullOrEmpty(queryParameter["MAIL_CC"].ToString()))
                    {
                        string[] arrCC = queryParameter["MAIL_CC"].ToString().Split(splitMail);
                        arrCC.ToList<string>().ForEach(t => mailMessage.CC.Add(new MailAddress(t)));
                    }

                    if (queryParameter["MAIL_BCC"] != null && !string.IsNullOrEmpty(queryParameter["MAIL_BCC"].ToString()))
                    {
                        string[] arrBcc = queryParameter["MAIL_BCC"].ToString().Split(splitMail);
                        arrBcc.ToList<string>().ForEach(t => mailMessage.Bcc.Add(new MailAddress(t)));
                    }

                    if (queryParameter.Files != null && queryParameter.Files.Count > 0)
                    {
                        foreach (Connector.FileParameter fileParameter in queryParameter.Files)
                        {
                            mailMessage.Attachments.Add(new System.Net.Mail.Attachment(fileParameter.File.FullName));
                        }
                    }

                    mailMessage.Subject = queryParameter["MAIL_SUBJECT"].ToString();
                    mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                    mailMessage.Body = queryParameter["MAIL_BODY"].ToString();
                    mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Priority = MailPriority.Normal;

                    using (SmtpClient smtpClient = new SmtpClient(server, port))
                    {
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = credential;
                        smtpClient.EnableSsl = mailCfg.EnableSSL;
                        smtpClient.Send(mailMessage);
                    }
                }

                result.Success = true;
            }
            catch (SmtpFailedRecipientsException ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}