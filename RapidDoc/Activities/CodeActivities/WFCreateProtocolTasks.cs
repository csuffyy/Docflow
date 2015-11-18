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
using System.Text.RegularExpressions;

namespace RapidDoc.Activities.CodeActivities
{

    public sealed class WFCreateProtocolTasks : CodeActivity
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

            DocumentTable documentTable = _service.Find(documentId);
            ProcessTable processTable = _serviceProcess.FirstOrDefault(x => x.Id == documentTable.ProcessTableId);
            var documentView = _service.GetDocumentView(documentTable.RefDocumentId, processTable.TableName);
            List<PRT_QuestionList_Table> questionList = documentView.QuestionList;

            foreach (var question in questionList)
            {
                string questionText = question.Question;

                foreach (var decision in question.DecisionList)
                {
                    if ((decision.Decision != null && decision.Decision != String.Empty) &&
                        (decision.Users != null && decision.Users != String.Empty))
                    {
                        if (decision.Separated == true)
                        {
                            string initailStructure = decision.Users;
                            string[] arrayTempStructrue = initailStructure.Split(',');

                            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
                            string[] arrayStructure = arrayTempStructrue.Where(a => isGuid.IsMatch(a) == true).ToArray();

                            foreach (var item in arrayStructure)
                            {
                                string seprateUser = item + "," + arrayTempStructrue[Array.IndexOf(arrayTempStructrue, item) + 1];
                                CreateTask(seprateUser, decision, question, documentTable, questionText, documentId, currentUserId);

                            }
                        }
                        else
                            CreateTask(decision.Users, decision, question, documentTable, questionText, documentId, currentUserId);
                    }
                }
            }
        }

        void CreateTask(string users, PRT_DecisionList_Table decision, PRT_QuestionList_Table question, DocumentTable documentTable, string questionText, Guid documentId, string currentUserId)
        {
            USR_TAS_DailyTasks_View docModel = new USR_TAS_DailyTasks_View();
            docModel.MainField = questionText + "\n" + decision.Decision;

            DateTime? controlDate = decision.ControlDate != null ? decision.ControlDate : DateTime.Now;

            docModel.ExecutionDate = controlDate;
            docModel.Users = users;
            docModel.RefDocumentId = documentId;
            docModel.RefDocNum = documentTable.DocumentNum;
            ApplicationUser user = _serviceAccount.Find(currentUserId);
            ProcessTable processTable = _serviceProcess.FirstOrDefault(x => x.TableName == "USR_TAS_DailyTasks");
            var taskDocumentId = _service.SaveDocument(docModel, "USR_TAS_DailyTasks", processTable.Id, documentTable.FileId, user, false, false);
            documentTable = _service.Find(taskDocumentId);

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
