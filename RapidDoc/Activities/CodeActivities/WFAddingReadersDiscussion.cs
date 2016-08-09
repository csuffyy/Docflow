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

    public sealed class WFAddingReadersDiscussion : CodeActivity
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
        public IWorkflowService _serviceWorkflow { get; set; }

        [Browsable(false)]
        [Inject]
        public IDocumentReaderService _serviceDocumentReader { get; set; }


        [Browsable(false)]
        [Inject]
        public IEmailService _serviceEmail { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            Guid documentId = context.GetValue(this.inputDocumentId);
            Dictionary<string, Object> documentData = context.GetValue(this.inputDocumentData);
            string currentUserId = context.GetValue(this.inputCurrentUser);
            Task.Run(() =>
            {
                _service = DependencyResolver.Current.GetService<IDocumentService>();
                _serviceWorkflow = DependencyResolver.Current.GetService<IWorkflowService>();
                _serviceDocumentReader = DependencyResolver.Current.GetService<IDocumentReaderService>();
                _serviceEmail = DependencyResolver.Current.GetService<IEmailService>();

                var document = _service.Find(documentId);

                if ((string)documentData["Users"] != null && !String.IsNullOrEmpty((string)documentData["Users"]))
                {
                    List<FileTable> docFile = new List<FileTable>();

                    string[] usersAndRoles = _service.GetUserListFromStructure((string)documentData["Users"]);
                    List<string> users = _serviceWorkflow.EmplAndRolesToUserList(documentId, usersAndRoles);

                    IEmailService _EmailService = DependencyResolver.Current.GetService<IEmailService>();
                    var documentModel = _service.GetDocumentView(document.RefDocumentId, document.ProcessTable.TableName);
                    List<string> readers = _serviceWorkflow.EmplAndRolesToReaders(documentId, usersAndRoles);
                    _serviceDocumentReader.SaveOrderReader(document, readers.ToArray(), currentUserId);

                    _EmailService.SendReaderEmail(documentId, readers);
                }
            });
        }
    }
}
