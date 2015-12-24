using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Net;
using System.Text;
using RazorEngine;
using System.Web.Hosting;
using System.Globalization;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;
using RazorEngine.Templating;

namespace RapidDoc.Models.Services
{
    public interface IEmailService
    {
        EmailParameterTable FirstOrDefault(Expression<Func<EmailParameterTable, bool>> predicate);
        EmailParameterView FirstOrDefaultView(Expression<Func<EmailParameterTable, bool>> predicate);
        void Save(EmailParameterView viewTable);
        void SaveDomain(EmailParameterTable domainTable);
        EmailParameterTable Find(Guid id);
        void InitializeMailParameter();
        void SendEmail(EmailParameterTable emailParameter, string[] emailTo, string subject, string body);
        void SendInitiatorEmail(Guid documentId);
        void SendInitiatorEmailDocAdding(Guid documentId);
        void SendExecutorEmail(Guid documentId, string additionalTextCZ = "");
        void SendInitiatorRejectEmail(Guid documentId);
        void SendInitiatorClosedEmail(Guid documentId);
        void SendInitiatorCommentEmail(Guid documentId, string lastComment);
        void SendDelegationEmplEmail(DelegationView delegationView);
        void SendReaderEmail(Guid documentId, List<string> newReader);
        void SendNewExecutorEmail(Guid documentId, string userId, string additionalTextCZ = "");
        void SendNewExecutorEmail(Guid documentId, List<string> userListId, string additionalTextCZ = "");
        void SendSLAWarningEmail(string userId, IEnumerable<DocumentTable> documents);
        void SendSLADisturbanceEmail(string userId, IEnumerable<DocumentTable> documents);
        void SendReminderEmail(ApplicationUser user, List<DocumentTable> documents);
        void SendReminderTasksEmail(ApplicationUser user, List<DocumentTable> documents, List<USR_TAS_DailyTasks_Table> listDocuments);
        void SendFailedRoutesAdministrator(List<ReportProcessesView> listProcesses);
        void SendNewModificationUserEmail(Guid documentId, string userId, string additionalTextCZ = "");
        void SendNoteReadyModificationUserEmail(Guid documentId, string userId, string additionalTextCZ = "");
        void SendNotificationForUserEmail(Guid documentId, string userId, string additionalTextCZ = "");
        void SendORDForUserEmail(Guid documentId, List<string> users, dynamic model);
        void SendUsersClosedEmail(Guid documentId, List<string> users);
        void SendProlongationResultInitiator(Guid documentId, string userId, DateTime prolongationDate, string taskNum, string taskText);
        string CryptStringSHA256(string pass);
        bool CheckSuperPassHash(string pass);
    }

    public class EmailService : IEmailService
    {
        private IRepository<EmailParameterTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IRepository<EmplTable> repoEmpl;
        private IUnitOfWork _uow;
        private readonly IDocumentService _DocumentService;
        private readonly IDocumentReaderService _DocumentReaderService;
        private readonly ISystemService _SystemService;

        public EmailService(IUnitOfWork uow, IDocumentService documentService, IDocumentReaderService documentReaderService, ISystemService systemService)
        {
            _uow = uow;
            repo = uow.GetRepository<EmailParameterTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            repoEmpl = uow.GetRepository<EmplTable>();
            _DocumentService = documentService;
            _DocumentReaderService = documentReaderService;
            _SystemService = systemService;
        }

        public EmailParameterTable FirstOrDefault(Expression<Func<EmailParameterTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public EmailParameterView FirstOrDefaultView(Expression<Func<EmailParameterTable, bool>> predicate)
        {
            return Mapper.Map<EmailParameterTable, EmailParameterView>(FirstOrDefault(predicate));
        }

        public void Save(EmailParameterView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new EmailParameterTable();
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
            else
            {
                var domainTable = Find(viewTable.Id ?? Guid.Empty);
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
        }

        public void SaveDomain(EmailParameterTable domainTable)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userId;
                domainTable.ApplicationUserModifiedId = userId;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = userId;
                repo.Update(domainTable);
            }
            _uow.Commit();
        }

        public EmailParameterTable Find(Guid id)
        {
            return repo.GetById(id);
        }

        public void InitializeMailParameter()
        {
            EmailParameterTable domainTable = new EmailParameterTable();
            domainTable.SmtpServer = "SERVER NAME";
            domainTable.Email = "user@company.com";
            domainTable.UserName = "username";
            SaveDomain(domainTable);
        }

        public void SendEmail(EmailParameterTable emailParameter, string[] emailTo, string subject, string body)
        {
            if (emailTo == null || emailTo.Length == 0)
                return;

            SmtpClient smtpClient = new SmtpClient(emailParameter.SmtpServer, emailParameter.SmtpPort);
            smtpClient.EnableSsl = emailParameter.EnableSsl;
            smtpClient.Timeout = emailParameter.Timeout;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = true;
            smtpClient.Credentials = new NetworkCredential(emailParameter.UserName, emailParameter.Password);

            foreach (string email in emailTo)
            {
                MailMessage message = new MailMessage();
                smtpClient.SendCompleted += (s, e) =>
                {
                    smtpClient.Dispose();
                    message.Dispose();
                };

                message.From = new MailAddress(emailParameter.Email);
                message.Subject = subject == null ? "" : subject;
                message.Body = body == null ? "" : body;
                message.IsBodyHtml = true;
                message.BodyEncoding = UTF8Encoding.UTF8;
                message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                message.To.Add(email);

                try
                {
                    smtpClient.Send(message);
                }
                catch
                {
                    continue;
                }
            }
        }

        public void SendInitiatorEmail(Guid documentId)
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;

            ApplicationUser user = repoUser.GetById(documentTable.ApplicationUserCreatedId);
            if (!String.IsNullOrEmpty(user.Email))
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendInitiatorEmail, DocumentText = documentTable.DocumentText }, "emailTemplateDefault");
                    SendEmail(emailParameter, new string[] { user.Email }, String.Format("Вы создали новый документ [{0}]", documentTable.DocumentNum), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendExecutorEmail(Guid documentId, string additionalTextCZ = "")
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null || (documentTable.DocumentState != DocumentState.Agreement && documentTable.DocumentState != DocumentState.Execution && documentTable.DocType != DocumentType.Task))
                return;

            dynamic ViewBag = new DynamicViewBag();
            ViewBag.AdditionalText = additionalTextCZ;

            var userList = _DocumentService.GetSignUsersDirect(documentTable);

            if (userList.Count < 20)
            {
                foreach (var user in userList)
                {
                    if (!String.IsNullOrEmpty(user.Email))
                    {
                        string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                        EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                        EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                        if (emailParameter == null)
                            return;

                        string processName = documentTable.ProcessName;

                        new Task(() =>
                        {
                            string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                            string razorText = System.IO.File.ReadAllText(absFile);

                            string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                            CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                            Thread.CurrentThread.CurrentCulture = ci;
                            Thread.CurrentThread.CurrentUICulture = ci;
                            string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendExecutorEmail, DocumentText = documentTable.DocumentText}, ViewBag, "emailTemplateDefaultWithCZ");
                            SendEmail(emailParameter, new string[] { user.Email }, String.Format("Требуется ваша подпись, документ [{0}]", documentTable.DocumentNum), body);
                            ci = CultureInfo.GetCultureInfo(currentLang);
                            Thread.CurrentThread.CurrentCulture = ci;
                            Thread.CurrentThread.CurrentUICulture = ci;
                        }).Start();
                    }
                }
            }
            else
            {
                List<string> emails = userList.Where(x => x.Email != String.Empty).GroupBy(x => x.Email).Select(x => x.Key).ToList();
                if (emails != null && emails.Count > 0)
                {
                    string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";

                    EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                    if (emailParameter == null)
                        return;

                    string processName = documentTable.ProcessName;

                    new Task(() =>
                    {
                        string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicBulkEmailTemplate.cshtml";
                        string razorText = System.IO.File.ReadAllText(absFile);

                        string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                        CultureInfo ci = CultureInfo.GetCultureInfo("ru-RU");
                        Thread.CurrentThread.CurrentCulture = ci;
                        Thread.CurrentThread.CurrentUICulture = ci;
                        string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, BodyText = UIElementRes.UIElement.SendExecutorEmail, DocumentText = documentTable.DocumentText }, ViewBag, "emailBulkTemplateDefault");
                        SendEmail(emailParameter, emails.ToArray(), String.Format("Требуется ваша подпись, документ [{0}]", documentTable.DocumentNum), body);
                        ci = CultureInfo.GetCultureInfo(currentLang);
                        Thread.CurrentThread.CurrentCulture = ci;
                        Thread.CurrentThread.CurrentUICulture = ci;
                    }).Start();
                }
            }
        }

        public void SendInitiatorRejectEmail(Guid documentId)
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;

            List<string> userList = new List<string>();
            userList.Add(documentTable.ApplicationUserCreatedId);

            if (documentTable.ProcessTable.TableName == "USR_REQ_IT_CTP_IncidentIT")
            {
                var tableModel = _DocumentService.RouteCustomRepository(documentTable.ProcessTable.TableName).GetById(documentTable.RefDocumentId);
                if (tableModel != null)
                {
                    string[] array = tableModel.Users.Split(',');
                    Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
                    string[] result = array.Where(a => isGuid.IsMatch(a) == true).ToArray();
                    foreach (var item in result)
                    {
                        Guid emplId = Guid.Parse(item);
                        EmplTable empl = repoEmpl.GetById(emplId);
                        if (empl != null && empl.ApplicationUserId != null && empl.ApplicationUserId != documentTable.ApplicationUserCreatedId)
                            userList.Add(empl.ApplicationUserId);
                    }
                }
            }

            foreach (var userId in userList)
            {
                ApplicationUser user = repoUser.GetById(userId);
                if (!String.IsNullOrEmpty(user.Email))
                {
                    string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                    EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                    EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                    if (emailParameter == null)
                        return;

                    string processName = documentTable.ProcessName;

                    new Task(() =>
                    {
                        string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                        string razorText = System.IO.File.ReadAllText(absFile);

                        string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                        CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                        Thread.CurrentThread.CurrentCulture = ci;
                        Thread.CurrentThread.CurrentUICulture = ci;
                        string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendInitiatorRejectEmail, DocumentText = documentTable.DocumentText }, "emailTemplateDefault");
                        SendEmail(emailParameter, new string[] { user.Email }, String.Format("Ваш документ [{0}] был отменен", documentTable.DocumentNum), body);
                        ci = CultureInfo.GetCultureInfo(currentLang);
                        Thread.CurrentThread.CurrentCulture = ci;
                        Thread.CurrentThread.CurrentUICulture = ci;
                    }).Start();
                }
            }
        }

        public void SendInitiatorClosedEmail(Guid documentId)
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;

            ApplicationUser user = repoUser.GetById(documentTable.ApplicationUserCreatedId);
            if (!String.IsNullOrEmpty(user.Email))
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendInitiatorClosedEmail, DocumentText = documentTable.DocumentText }, "emailTemplateDefault");
                    SendEmail(emailParameter, new string[] { user.Email }, String.Format("Ваш документ [{0}] закрыт", documentTable.DocumentNum), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendInitiatorCommentEmail(Guid documentId, string lastComment)
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null || documentTable.DocType != DocumentType.Request)
                return;

            var users = repoUser.FindAll(x => x.Id == documentTable.ApplicationUserCreatedId).ToList();

            var currentReaders = _DocumentReaderService.GetPartial(x => x.DocumentTableId == documentId).ToList();
            foreach (var reader in currentReaders)
                users.Add(repoUser.GetById(reader.UserId));

            var signUsers = _DocumentService.GetSignUsersDirect(documentTable);
            foreach (var signUser in signUsers)
            {
                if (users.Any(x => x.Id == signUser.Id))
                    continue;
                else
                    users.Add(signUser);
            }

            foreach (var user in users)
            {
                if (!String.IsNullOrEmpty(user.Email) && user.UserName != HttpContext.Current.User.Identity.Name)
                {
                    string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                    EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                    EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                    if (emailParameter == null)
                        return;

                    string processName = documentTable.ProcessName;

                    new Task(() =>
                    {
                        string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\CommentEmailTemplate.cshtml";
                        string razorText = System.IO.File.ReadAllText(absFile);

                        string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                        CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                        Thread.CurrentThread.CurrentCulture = ci;
                        Thread.CurrentThread.CurrentUICulture = ci;
                        string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendInitiatorCommentEmail, LastComment = lastComment, DocumentText = documentTable.DocumentText }, "emailTemplateComment");
                        SendEmail(emailParameter, new string[] { user.Email }, String.Format("Новый комментарий в документе [{0}]", documentTable.DocumentNum), body);
                        ci = CultureInfo.GetCultureInfo(currentLang);
                        Thread.CurrentThread.CurrentCulture = ci;
                        Thread.CurrentThread.CurrentUICulture = ci;
                    }).Start();
                }
            }
        }

        public void SendDelegationEmplEmail(DelegationView delegationView)
        {
            EmplTable emplTableFrom = repoEmpl.GetById(delegationView.EmplTableFromId);
            EmplTable emplTableTo = repoEmpl.GetById(delegationView.EmplTableToId);
            ApplicationUser user = repoUser.GetById(emplTableTo.ApplicationUserId);

            if (!String.IsNullOrEmpty(user.Email))
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString();

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\DelegationEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentUri = documentUri, EmplNameTo = emplTableTo.FullName, EmplNameFrom = emplTableFrom.FullName, BodyText = UIElementRes.UIElement.SendDelegationEmplEmail }, "emailTemplateDelegation");
                    SendEmail(emailParameter, new string[] { user.Email }, "На вас настроенно делегирование", body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendReaderEmail(Guid documentId, List<string> newReader)
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;

            List<string> emails = repoUser.FindAll(x => newReader.Contains(x.Id) && x.Email != String.Empty).GroupBy(x => x.Email).Select(x => x.Key).ToList();

            if (emails != null && emails.Count > 0)
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicBulkEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo("ru-RU");
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, BodyText = UIElementRes.UIElement.SendReaderEmail, DocumentText = documentTable.DocumentText }, "emailBulkTemplateDefault");
                    SendEmail(emailParameter, emails.ToArray(), String.Format("Вас добавили читателем, документ [{0}]", documentTable.DocumentNum), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendNewExecutorEmail(Guid documentId, string userId, string additionalTextCZ = "")
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;
            ApplicationUser user = repoUser.GetById(userId);

            dynamic ViewBag = new DynamicViewBag();
            ViewBag.AdditionalText = additionalTextCZ;

            if (!String.IsNullOrEmpty(user.Email))
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendExecutorEmail, DocumentText = documentTable.DocumentText, AdditionalText = additionalTextCZ }, ViewBag, "emailTemplateDefault");
                    SendEmail(emailParameter, new string[] { user.Email }, String.Format("Требуется ваша подпись, документ [{0}]", documentTable.DocumentNum), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendNewExecutorEmail(Guid documentId, List<string> userListId,string additionalTextCZ = "")
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;

            dynamic ViewBag = new DynamicViewBag();
            ViewBag.AdditionalText = additionalTextCZ;

            List<string> emails = repoUser.FindAll(x => userListId.Contains(x.Id) && x.Email != String.Empty).GroupBy(x => x.Email).Select(x => x.Key).ToList();

            if (emails != null && emails.Count > 0)
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicBulkEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo("ru-RU");
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, BodyText = UIElementRes.UIElement.SendExecutorEmail, DocumentText = documentTable.DocumentText }, ViewBag, "emailBulkTemplateDefault");
                    SendEmail(emailParameter, emails.ToArray(), String.Format("Требуется ваша подпись, документ [{0}]", documentTable.DocumentNum), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendSLAWarningEmail(string userId, IEnumerable<DocumentTable> documents)
        {
            ApplicationUser user = repoUser.GetById(userId);
            List<string> documentUrls = new List<string>();
            List<string> documentNums = new List<string>();
            List<string> documentText = new List<string>();

            if (!String.IsNullOrEmpty(user.Email))
            {
                int num = 0;
                foreach (var documentTable in documents)
                {
                    num++;
                    documentUrls.Add("http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true");
                    documentNums.Add(documentTable.DocumentNum + " - " + documentTable.ProcessName);
                    documentText.Add(documentTable.DocumentText);
                }

                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\SLAEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentUri = "", DocumentUris = documentUrls.ToArray(), DocumentNums = documentNums.ToArray(), documentText = documentText.ToArray(), EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendSLAWarningEmail }, "emailTemplateSLAStatus");
                    SendEmail(emailParameter, new string[] { user.Email }, "Срок исполнения подходит к концу", body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendSLADisturbanceEmail(string userId, IEnumerable<DocumentTable> documents)
        {
            ApplicationUser user = repoUser.GetById(userId);
            List<string> documentUrls = new List<string>();
            List<string> documentNums = new List<string>();
            List<string> documentText = new List<string>();

            if (!String.IsNullOrEmpty(user.Email))
            {
                int num = 0;
                foreach (var documentTable in documents)
                {
                    num++;
                    documentUrls.Add("http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true");
                    documentNums.Add(documentTable.DocumentNum + " - " + documentTable.ProcessName);
                    documentText.Add(documentTable.DocumentText);
                }

                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\SLAEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentUri = "", DocumentUris = documentUrls.ToArray(), DocumentNums = documentNums.ToArray(), documentText = documentText.ToArray(), EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendSLADisturbanceEmail }, "emailTemplateSLAStatus");
                    SendEmail(emailParameter, new string[] { user.Email }, "Исполнение по документам просрочено", body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendReminderEmail(ApplicationUser user, List<DocumentTable> documents)
        {
            List<string> documentUrls = new List<string>();
            List<string> documentNums = new List<string>();
            List<string> documentText = new List<string>();

            if (!String.IsNullOrEmpty(user.Email))
            {
                int num = 0;
                foreach (var documentTable in documents)
                {
                    num++;
                    documentUrls.Add("http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true");
                    documentNums.Add(documentTable.DocumentNum + " - " + documentTable.ProcessName);

                    if (!String.IsNullOrEmpty(documentTable.DocumentText) && documentTable.DocumentText.Length > 80)
                        documentText.Add(documentTable.DocumentText.Substring(0, 80) + "...");
                    else
                        documentText.Add(documentTable.DocumentText);
                }

                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\SLAEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentUri = "", DocumentUris = documentUrls.ToArray(), DocumentNums = documentNums.ToArray(), documentText = documentText.ToArray(), EmplName = emplTable.FullName, BodyText = "Документы на подписи" }, "emailTemplateSLAStatus");
                    SendEmail(emailParameter, new string[] { user.Email }, "У вас на подписи находятся следующие документы", body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendFailedRoutesAdministrator(List<ReportProcessesView> listProcesses)
        {
            List<string> processUrls = new List<string>();
            List<string> stageNames = new List<string>();
            List<string> filterTexts = new List<string>();

            RoleManager<ApplicationRole> RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(_uow.GetDbContext<ApplicationDbContext>()));

            if (RoleManager.RoleExists("SetupAdministrator"))
            {
                foreach (ReportProcessesView reportProcess in listProcesses)
                {
                    processUrls.Add("http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + reportProcess.Process.CompanyTable.AliasCompanyName + "/Process/Edit/" + reportProcess.Process.Id);
                    stageNames.Add(reportProcess.StageName + " - " + reportProcess.Process.TableName);
                    filterTexts.Add(reportProcess.FilterText);
                }
            
                var names = RoleManager.FindByName("SetupAdministrator").Users;
                if (names != null && names.Count() > 0)
                {
                    foreach (IdentityUserRole name in names)
                    {
                        ApplicationUser user = repoUser.Find(x => (x.UserName == name.UserId || x.Id == name.UserId) && x.Enable == true);
                        if (!String.IsNullOrEmpty(user.Email))
                        {
                            EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                            EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                            if (emailParameter == null)
                                return;

                            new Task(() =>
                            {
                                string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\RoutEmailTemplate.cshtml";
                                string razorText = System.IO.File.ReadAllText(absFile);

                                string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                                CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                                Thread.CurrentThread.CurrentCulture = ci;
                                Thread.CurrentThread.CurrentUICulture = ci;
                                string body = Razor.Parse(razorText, new { DocumentUri = "", DocumentUris = processUrls.ToArray(), DocumentNums = stageNames.ToArray(), documentText = filterTexts.ToArray(), EmplName = emplTable.FullName, BodyText = "У вас несколько ошибочных маршрутов" }, "emailTemplateRoutes");
                                SendEmail(emailParameter, new string[] { user.Email }, "Маршруты процессов на исправление", body);
                                ci = CultureInfo.GetCultureInfo(currentLang);
                                Thread.CurrentThread.CurrentCulture = ci;
                                Thread.CurrentThread.CurrentUICulture = ci;
                            }).Start();
                        }   
                    }
                }              
            }
        }


        public void SendInitiatorEmailDocAdding(Guid documentId)
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;

            ApplicationUser user = repoUser.GetById(documentTable.ApplicationUserCreatedId);
            if (!String.IsNullOrEmpty(user.Email))
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.AddingNewDocEmail, DocumentText = documentTable.DocumentText }, "emailTemplateDefault");
                    SendEmail(emailParameter, new string[] { user.Email }, String.Format("Добавлен новый файл в документ [{0}]", documentTable.DocumentNum), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }


        public void SendReminderTasksEmail(ApplicationUser user, List<DocumentTable> documents, List<USR_TAS_DailyTasks_Table> listDocuments)
        {
            List<string> documentUrls = new List<string>();
            List<string> documentNums = new List<string>();
            List<string> documentText = new List<string>();

            if (!String.IsNullOrEmpty(user.Email))
            {
                int num = 0; string countHours = String.Empty;
                foreach (var documentTable in documents.Where(x => x.DocumentState == DocumentState.Agreement || x.DocumentState == DocumentState.Execution))
                {
                    num++;
                    documentUrls.Add("http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true");
                    documentNums.Add(documentTable.DocumentNum + " - " + documentTable.ProcessName);
                    USR_TAS_DailyTasks_Table countHourTable = listDocuments.FirstOrDefault(x => x.DocumentTableId == documentTable.Id);

                    if(countHourTable.ProlongationDate == null)
                        countHours = DateTime.UtcNow > countHourTable.ExecutionDate ? "Задача просрочена" : String.Format("Осталось {0} дн.",
                        (int)Math.Abs(Math.Round(((DateTime)countHourTable.ExecutionDate - DateTime.UtcNow).TotalDays)));
                    else
                        countHours = DateTime.UtcNow > countHourTable.ProlongationDate ? "Задача просрочена" : String.Format("Осталось {0} дн.", (int)Math.Abs(Math.Round(((DateTime)countHourTable.ProlongationDate - DateTime.UtcNow).TotalDays)));

                    if (!String.IsNullOrEmpty(documentTable.DocumentText) && documentTable.DocumentText.Length > 60)
                        documentText.Add(countHours + " " + documentTable.DocumentText.Substring(0, 60) + "...");
                    else
                        documentText.Add(countHours + " " + documentTable.DocumentText);
                }

                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\SLAEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentUri = "", DocumentUris = documentUrls.ToArray(), DocumentNums = documentNums.ToArray(), documentText = documentText.ToArray(), EmplName = emplTable.FullName, BodyText = "задачи на выполнении" }, "emailTemplateSLAStatus");
                    SendEmail(emailParameter, new string[] { user.Email }, "У вас на выполнении находятся следующие задачи", body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }


        public void SendNewModificationUserEmail(Guid documentId, string userId, string additionalTextCZ = "")
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;
            ApplicationUser user = repoUser.GetById(userId);

            dynamic ViewBag = new DynamicViewBag();
            ViewBag.AdditionalText = additionalTextCZ;

            if (!String.IsNullOrEmpty(user.Email))
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendModificationUserEmail, DocumentText = documentTable.DocumentText, AdditionalText = additionalTextCZ }, ViewBag, "emailTemplateDefault");
                    SendEmail(emailParameter, new string[] { user.Email }, String.Format("Требуется доработка документа [{0}]", documentTable.DocumentNum), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }


        public void SendNoteReadyModificationUserEmail(Guid documentId, string userId, string additionalTextCZ = "")
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;
            ApplicationUser user = repoUser.GetById(userId);

            dynamic ViewBag = new DynamicViewBag();
            ViewBag.AdditionalText = additionalTextCZ;

            if (!String.IsNullOrEmpty(user.Email))
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = "Новая версия документа", DocumentText = documentTable.DocumentText, AdditionalText = additionalTextCZ }, ViewBag, "emailTemplateDefault");
                    SendEmail(emailParameter, new string[] { user.Email }, String.Format("Новая версия документа [{0}]", documentTable.DocumentNum), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }


        public void SendNotificationForUserEmail(Guid documentId, string userId, string additionalTextCZ = "")
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;
            ApplicationUser user = repoUser.GetById(userId);

            dynamic ViewBag = new DynamicViewBag();
            ViewBag.AdditionalText = additionalTextCZ;

            if (!String.IsNullOrEmpty(user.Email))
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                ApplicationUser currentWatchingUser = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
                EmplTable currentWatchingEmplTable = repoEmpl.Find(x => x.ApplicationUserId == currentWatchingUser.Id && x.Enable == true);

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = "Уведомление о просмотре документа", DocumentText = documentTable.DocumentText, AdditionalText = additionalTextCZ }, ViewBag, "emailTemplateDefault");
                    SendEmail(emailParameter, new string[] { user.Email }, String.Format("Просмотрен пользователем {1} документ [{0}]", documentTable.DocumentNum, currentWatchingEmplTable.FullName), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public void SendORDForUserEmail(Guid documentId, List<string> users, dynamic model)
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;

            List<string> emailList = repoUser.FindAll(x => users.Contains(x.Id) && x.Email != String.Empty).GroupBy(x => x.Email).Select(x => x.Key).ToList();

            if (emailList == null || emailList.Count == 0)
                return;

            string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
            
            EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
            if (emailParameter == null)
                return;

            string processName = documentTable.ProcessName;
            string subject = Regex.Replace(model.Subject, @"\t|\n|\r", "");
                   
            new Task(() =>
            {
                string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\ORDEmailTemplate.cshtml";
                string razorText = System.IO.File.ReadAllText(absFile);

                string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                CultureInfo ci = CultureInfo.GetCultureInfo("ru-RU");
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, OrderNum = model.OrderNum, OrderDate = model.OrderDate.ToShortDateString(), Subject = model.Subject, MainField = model.MainField, MainFieldTranslate = model.MainFieldTranslate, SignTitle = model.SignTitle, SignName = model.SignName }, "emailORD");
                SendEmail(emailParameter, emailList.ToArray(), String.Format("{0} - {1} - {2}", documentTable.DocumentNum, processName, subject), body);
                ci = CultureInfo.GetCultureInfo(currentLang);
                
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }).Start();
        }


        public void SendUsersClosedEmail(Guid documentId, List<string> users)
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;

            foreach (var item in users.Distinct())
	        {
                ApplicationUser user = repoUser.GetById(item);
                if (!String.IsNullOrEmpty(user.Email))
                {

                    string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                    EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                    EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                    if (emailParameter == null)
                        return;

                    string processName = documentTable.ProcessName;

                    new Task(() =>
                    {
                        string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                        string razorText = System.IO.File.ReadAllText(absFile);

                        string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                        CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                        Thread.CurrentThread.CurrentCulture = ci;
                        Thread.CurrentThread.CurrentUICulture = ci;
                        string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = UIElementRes.UIElement.SendInitiatorClosedEmail, DocumentText = documentTable.DocumentText }, "emailTemplateDefault");
                        SendEmail(emailParameter, new string[] { user.Email }, String.Format("Ваш документ [{0}] закрыт", documentTable.DocumentNum), body);
                        ci = CultureInfo.GetCultureInfo(currentLang);
                        Thread.CurrentThread.CurrentCulture = ci;
                        Thread.CurrentThread.CurrentUICulture = ci;
                    }).Start();
                }
            }         
        }

        public void SendProlongationResultInitiator(Guid documentId, string userId, DateTime prolongationDate, string taskNum, string taskText)
        {
            var documentTable = _DocumentService.Find(documentId);
            if (documentTable == null)
                return;

            ApplicationUser user = repoUser.GetById(userId);
            if (!String.IsNullOrEmpty(user.Email))
            {
                string documentUri = "http://" + ConfigurationManager.AppSettings.Get("WebSiteUrl").ToString() + "/" + documentTable.CompanyTable.AliasCompanyName + "/Document/ShowDocument/" + documentTable.Id + "?isAfterView=true";
                EmplTable emplTable = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.Enable == true);

                EmailParameterTable emailParameter = FirstOrDefault(x => x.SmtpServer != String.Empty);
                if (emailParameter == null)
                    return;

                string processName = documentTable.ProcessName;

                new Task(() =>
                {
                    string absFile = HostingEnvironment.ApplicationPhysicalPath + @"Views\\EmailTemplate\\BasicEmailTemplate.cshtml";
                    string razorText = System.IO.File.ReadAllText(absFile);

                    string currentLang = Thread.CurrentThread.CurrentCulture.Name;
                    CultureInfo ci = CultureInfo.GetCultureInfo(user.Lang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                    string body = Razor.Parse(razorText, new { DocumentNum = String.Format("{0} - {1}", documentTable.DocumentNum, processName), DocumentUri = documentUri, EmplName = emplTable.FullName, BodyText = String.Format("Поручение {0}, продлили до {1}", taskNum, prolongationDate.ToShortDateString()), DocumentText = taskText }, "emailTemplateDefault");
                    SendEmail(emailParameter, new string[] { user.Email }, String.Format("Продление срока исполнения поручения [{0}]", taskNum), body);
                    ci = CultureInfo.GetCultureInfo(currentLang);
                    Thread.CurrentThread.CurrentCulture = ci;
                    Thread.CurrentThread.CurrentUICulture = ci;
                }).Start();
            }
        }

        public string CryptStringSHA256(string pass)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(pass);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            String hashPass = System.Text.Encoding.ASCII.GetString(data);

            return hashPass;
        }

        public bool CheckSuperPassHash(string pass)
        {
            return String.Compare(CryptStringSHA256(pass), FirstOrDefault(x => x.SuperPass != String.Empty).SuperPass) == 0 ? true : false;
        }
    }
}