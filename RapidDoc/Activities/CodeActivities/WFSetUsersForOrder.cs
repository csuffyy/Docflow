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

    public sealed class WFSetUsersForOrder : CodeActivity
    {
        public InArgument<string> inputSystemName { get; set; }

        [RequiredArgument]
        public InArgument<string> inputUserId { get; set; }

        [RequiredArgument]
        public InArgument<Guid> inputDocumentId { get; set; }

        [RequiredArgument]
        public InArgument<string> inputCurrentUser { get; set; }

        [RequiredArgument]
        public InArgument<DocumentState> inputStep { get; set; }

        [RequiredArgument]
        public OutArgument<string> outputBookmark { get; set; }

        public InArgument<bool> useManual { get; set; }

        public InArgument<int> slaOffset { get; set; }

        public InArgument<bool> executionStep { get; set; }

        [RequiredArgument]
        public OutArgument<bool> outputSkipStep { get; set; }

        [RequiredArgument]
        public OutArgument<DocumentState> outputStep { get; set; }

        public InArgument<bool> noneSkip { get; set; }

        [Browsable(false)]
        [Inject]
        public IWorkflowService _service { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string systemName = context.GetValue(this.inputSystemName);
            string userid = context.GetValue(this.inputUserId);
            Guid documentId = context.GetValue(this.inputDocumentId);
            DocumentState documentStep = context.GetValue(this.inputStep);
            string currentUserId = context.GetValue(this.inputCurrentUser);
            bool useManual = context.GetValue(this.useManual);
            int slaOffset = context.GetValue(this.slaOffset);
            bool executionStep = context.GetValue(this.executionStep);
            bool noneSkipStep = context.GetValue(this.noneSkip);

            _service = DependencyResolver.Current.GetService<IWorkflowService>();

            List<WFTrackerUsersTable> userList = new List<WFTrackerUsersTable>();
            userList.Add(new WFTrackerUsersTable { UserId = userid });
            _service.CreateTrackerRecord(systemName, documentStep, documentId, this.DisplayName, userList, currentUserId, this.Id + userid, useManual, slaOffset, executionStep);
                
            outputSkipStep.Set(context, false);
            outputBookmark.Set(context, this.DisplayName);
            outputStep.Set(context, documentStep);          
        }
    }
}
