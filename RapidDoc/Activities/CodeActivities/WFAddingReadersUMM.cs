using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using RapidDoc.Models.Services;
using RapidDoc.Models.DomainModels;
using RapidDoc.App_Start;
using System.Activities.Tracking;
using Ninject;
using System.Web.Mvc;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using System.ComponentModel;

namespace RapidDoc.Activities.CodeActivities
{
    public sealed class WFAddingReadersUMM : CodeActivity
    {
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

        [RequiredArgument]
        public InArgument<string> inputUsersGroup { get; set; }

        [RequiredArgument]
        public InArgument<Guid> inputDocumentId { get; set; }

        [RequiredArgument]
        public InArgument<string> inputCurrentUser { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            _service = DependencyResolver.Current.GetService<IDocumentService>();
            _serviceWorkflow = DependencyResolver.Current.GetService<IWorkflowService>();
            _serviceDocumentReader = DependencyResolver.Current.GetService<IDocumentReaderService>();
            _serviceEmail = DependencyResolver.Current.GetService<IEmailService>();

            Guid documentId = context.GetValue(this.inputDocumentId);
            string currentUserId = context.GetValue(this.inputCurrentUser);
            string  roleDocumentId = context.GetValue(this.inputUsersGroup);
            var document = _service.Find(documentId);

            List<string> readers = _serviceWorkflow.GroupToUserList(documentId, roleDocumentId);

            if (readers.Count() > 0)
            {
                List<string> newReader = _serviceDocumentReader.SaveOrderReader(document.Id, readers.ToArray(), currentUserId);
                _serviceEmail.SendReaderEmail(documentId, newReader.Distinct().ToList());
            }
        }
    }
}