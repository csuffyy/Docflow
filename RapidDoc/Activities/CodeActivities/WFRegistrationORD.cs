using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ninject;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Services;
using RapidDoc.Models.ViewModels;
using System.ComponentModel;

namespace RapidDoc.Activities.CodeActivities
{
    public sealed class WFRegistrationORD : CodeActivity
    {
        [RequiredArgument]
        public InArgument<Guid> inputDocumentId { get; set; }

        [RequiredArgument]
        public InArgument<Dictionary<string, Object>> inputDocumentData { get; set; }

        [RequiredArgument]
        public InArgument<string> inputCurrentUser { get; set; }

        [Browsable(false)]
        [Inject]
        public IDocumentService _service { get; set; }

        [Browsable(false)]
        [Inject]
        public IAccountService _serviceAccount { get; set; }

        [Browsable(false)]
        [Inject]
        public IProcessService _serviceProcess { get; set; }

        [Browsable(false)]
        [Inject]
        public ISearchService _serviceSearch { get; set; }

        [Browsable(false)]
        [Inject]
        public IWorkflowService _serviceWorkflow { get; set; }

        [Browsable(false)]
        [Inject]
        public IDocumentReaderService _serviceDocumentReader { get; set; }

        [Browsable(false)]
        [Inject]
        public IDocumentSubcriptionService _serviceDocumentSubcriptionService { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            _service = DependencyResolver.Current.GetService<IDocumentService>();
            _serviceAccount = DependencyResolver.Current.GetService<IAccountService>();
            _serviceProcess = DependencyResolver.Current.GetService<IProcessService>();
            _serviceSearch = DependencyResolver.Current.GetService<ISearchService>();
            _serviceWorkflow = DependencyResolver.Current.GetService<IWorkflowService>();
            _serviceDocumentReader = DependencyResolver.Current.GetService<IDocumentReaderService>();
            _serviceDocumentSubcriptionService = DependencyResolver.Current.GetService<IDocumentSubcriptionService>();
            Guid documentId = context.GetValue(this.inputDocumentId);
            Dictionary<string, Object> documentData = context.GetValue(this.inputDocumentData);
            string currentUserId = context.GetValue(this.inputCurrentUser);
            Guid? bookingNumberId = Guid.Empty;
            Guid? cancelDocumentId = Guid.Empty;
            Guid? additionDocumentId = Guid.Empty;
            var document = _service.Find(documentId);

            if(documentData.ContainsKey("NumberSeriesBookingTableId"))
            {
                if ((Guid?)documentData["NumberSeriesBookingTableId"] != null)
                    bookingNumberId = (Guid)documentData["NumberSeriesBookingTableId"];
            }

            if (documentData.ContainsKey("ControlUsers") && documentData.ContainsKey("ControlDate"))
            {
                if(!String.IsNullOrEmpty((string)documentData["ControlUsers"]))
                {
                    USR_TAS_DailyTasks_View docModel = new USR_TAS_DailyTasks_View();
                    docModel.MainField = (string)documentData["Subject"] + 
                        (string)documentData["MainField"] + (string)documentData["MainFieldTranslate"];

                    DateTime? controlDate = (DateTime?)documentData["ControlDate"];
                    if (controlDate == null)
                    {
                        controlDate = DateTime.Now.Date.AddDays(2);
                    }

                    docModel.ExecutionDate = controlDate;
                    docModel.Users = (string)documentData["ControlUsers"];
                    docModel.RefDocumentId = documentId;
                    docModel.RefDocNum = document.DocumentNum;
                    ApplicationUser user = _serviceAccount.Find(currentUserId);
                    ProcessTable processTable = _serviceProcess.FirstOrDefault(x => x.TableName == "USR_TAS_DailyTasks");
                    var taskDocumentId = _service.SaveDocument(docModel, "USR_TAS_DailyTasks", processTable.Id, Guid.NewGuid(), user, false, false);
                    DocumentTable documentTable = _service.Find(taskDocumentId);

                    Task.Run(() =>
                    {
                        IReviewDocLogService _ReviewDocLogServiceTask = DependencyResolver.Current.GetService<IReviewDocLogService>();
                        IHistoryUserService _HistoryUserServiceTask = DependencyResolver.Current.GetService<IHistoryUserService>();
                        _ReviewDocLogServiceTask.SaveDomain(new ReviewDocLogTable { DocumentTableId = documentId }, "", user);
                        _HistoryUserServiceTask.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = Models.Repository.HistoryType.NewDocument }, user.Id);
                    });

                    _serviceSearch.SaveSearchData(taskDocumentId, docModel, "USR_TAS_DailyTasks", currentUserId);
                    Dictionary<string, object> taskDocumentData = new Dictionary<string, object>();
                    taskDocumentData.Add("ExecutionDate", docModel.ExecutionDate);
                    taskDocumentData.Add("MainField", docModel.MainField);
                    taskDocumentData.Add("Users", docModel.Users);
                    _serviceWorkflow.RunWorkflow(documentTable, "USR_TAS_DailyTasks", taskDocumentData, currentUserId);
                }
            }

            if (documentData.ContainsKey("CancelDocumentId") && documentData.ContainsKey("CancelOrder"))
            {
                if ((bool)documentData["CancelOrder"] == true && (Guid?)documentData["CancelDocumentId"] != null)
                {
                    cancelDocumentId = (Guid)documentData["CancelDocumentId"];
                    var documentCancel = _service.Find(cancelDocumentId);
                    if (documentCancel != null)
                    {
                        documentCancel.Cancel = true;
                        documentCancel.CancelDocumentId = documentId;
                        _service.UpdateDocument(documentCancel, currentUserId);
                    }
                }
            }

            if (documentData.ContainsKey("AdditionDocumentId") && documentData.ContainsKey("Addition"))
            {
                if ((bool)documentData["Addition"] == true && (Guid?)documentData["AdditionDocumentId"] != null)
                {
                    additionDocumentId = (Guid)documentData["AdditionDocumentId"];
                    var documentAddition = _service.Find(additionDocumentId);
                    if (documentAddition != null)
                    {
                        ProcessView processView = _serviceProcess.FindView(documentAddition.ProcessTableId, currentUserId);
                        var documentRef = _service.GetDocumentView(documentAddition.RefDocumentId, processView.TableName);
                        documentRef.AdditionCount++;
                        _service.UpdateDocumentFields(documentRef, processView);
                    }
                }
            }

            _service.ORDRegistration(documentId, currentUserId, bookingNumberId);

            if (documentData.ContainsKey("ListSubcription"))
            {
                if (!String.IsNullOrEmpty((string)documentData["ListSubcription"]))
                {
                    List<FileTable> docFile = new List<FileTable>();

                    string[] usersAndRoles = _service.GetUserListFromStructure((string)documentData["ListSubcription"]);
                    List<string> users = _serviceWorkflow.EmplAndRolesToUserList(usersAndRoles);
                    _serviceDocumentSubcriptionService.SaveSubscriber(documentId, users.ToArray(), currentUserId);
                    IEmailService _EmailService = DependencyResolver.Current.GetService<IEmailService>();
                    var documentModel = _service.GetDocumentView(document.RefDocumentId, document.ProcessTable.TableName);
                    if ((bool)documentData["AddReaders"] == true)
                    {
                        List<string> readers = _serviceWorkflow.EmplAndRolesToReaders(usersAndRoles);
                        _serviceDocumentReader.SaveReader(documentId, readers.ToArray(), currentUserId);
                    }

                    if ((bool)documentData["AddAttachment"] == true)
                        docFile = _service.GetAllFilesDocument(document.FileId).ToList();

                    _EmailService.SendORDForUserEmail(documentId, users, documentModel, docFile);
                }
            }
        }
    }
}