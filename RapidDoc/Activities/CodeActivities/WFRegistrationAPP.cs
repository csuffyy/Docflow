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
using RapidDoc.Models.Repository;

namespace RapidDoc.Activities.CodeActivities
{

    public sealed class WFRegistrationAPP : CodeActivity
    {
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
        public ISystemService _serviceSystem { get; set; }

        [RequiredArgument]
        public InArgument<Dictionary<string, Object>> inputDocumentData { get; set; }

        [RequiredArgument]
        public InArgument<Guid> inputDocumentId { get; set; }

        [RequiredArgument]
        public InArgument<string> inputCurrentUser { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            _service = DependencyResolver.Current.GetService<IDocumentService>();
            _serviceAccount = DependencyResolver.Current.GetService<IAccountService>();
            _serviceProcess = DependencyResolver.Current.GetService<IProcessService>();
            _serviceSearch = DependencyResolver.Current.GetService<ISearchService>();
            _serviceWorkflow = DependencyResolver.Current.GetService<IWorkflowService>();

            Dictionary<string, Object> documentData = context.GetValue(this.inputDocumentData);
            Guid documentId = context.GetValue(this.inputDocumentId);
            string currentUserId = context.GetValue(this.inputCurrentUser);
            var document = _service.Find(documentId);
            if ((documentData.ContainsKey("RegistrationNum") && documentData["RegistrationNum"] == null) || !documentData.ContainsKey("RegistrationNum"))
                _service.APPRegistration(documentId, currentUserId);

            if (documentData.ContainsKey("RegistrationNum") && documentData["RegistrationNum"] != null)
            {
                if (documentData.ContainsKey("Whom") && documentData.ContainsKey("ExecutionDate"))
                {
                    if (!String.IsNullOrEmpty((string)documentData["Whom"]))
                    {
                        USR_TAS_DailyTasks_View docModel = new USR_TAS_DailyTasks_View();
                        docModel.MainField = document.DocumentText;

                        DateTime? controlDate = (DateTime?)documentData["ExecutionDate"];
                        if (controlDate == null)
                        {
                            controlDate = DateTime.Now.Date.AddMonths(1);
                        }

                        docModel.ExecutionDate = controlDate;
                        docModel.Users = (string)documentData["Whom"];
                        docModel.RefDocumentId = documentId;
                        docModel.RefDocNum = document.DocumentNum;
                        ApplicationUser user = _serviceAccount.Find(currentUserId);
                        ProcessTable processTable = _serviceProcess.FirstOrDefault(x => x.TableName == "USR_TAS_DailyTasks" && x.CompanyTableId == user.CompanyTableId);

                        List<FileTable> docFile = _service.GetAllFilesDocument(document.FileId).ToList();
                        Guid newDocFileId = Guid.NewGuid();
                        docFile.ForEach(x => _service.DuplicateFile(x, user.Id, newDocFileId));

                        var taskDocumentId = _service.SaveDocument(docModel, "USR_TAS_DailyTasks", processTable.Id, newDocFileId, user, false, false);
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
            }
        }
    }
}
