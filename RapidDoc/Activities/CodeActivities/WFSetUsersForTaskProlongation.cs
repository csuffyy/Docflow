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

    public sealed class WFSetUsersForTaskProlongation : CodeActivity
    {
        public InArgument<string> inputSystemName { get; set; }

        [RequiredArgument]
        public InArgument<List<WFTrackerUsersTable>> inputUserNames { get; set; }

        [RequiredArgument]
        public InArgument<Guid> inputDocumentId { get; set; }

        [RequiredArgument]
        public InArgument<string> inputCurrentUser { get; set; }

        [RequiredArgument]
        public InArgument<DocumentState> inputStep { get; set; }

        [RequiredArgument]
        public InArgument<int> inputActivityId { get; set; }

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

        [Browsable(false)]
        [Inject]
        public IWorkflowTrackerService _trackerService { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string systemName = context.GetValue(this.inputSystemName);
            List<WFTrackerUsersTable> userNames = context.GetValue(this.inputUserNames);
            Guid documentId = context.GetValue(this.inputDocumentId);
            DocumentState documentStep = context.GetValue(this.inputStep);
            string currentUserId = context.GetValue(this.inputCurrentUser);
            bool useManual = context.GetValue(this.useManual);
            int slaOffset = context.GetValue(this.slaOffset);
            bool executionStep = context.GetValue(this.executionStep);
            bool noneSkipStep = context.GetValue(this.noneSkip);
            int inputActivityId = context.GetValue(this.inputActivityId);

            _service = DependencyResolver.Current.GetService<IWorkflowService>();
            _trackerService = DependencyResolver.Current.GetService<IWorkflowTrackerService>();

            WFTrackerTable trackerTable = _trackerService.FirstOrDefault(x => x.ActivityID == inputActivityId.ToString() && x.DocumentTableId == documentId);

            _service.CreateTrackerRecord(systemName, documentStep, documentId, trackerTable.ActivityName, userNames, currentUserId, inputActivityId.ToString(), useManual, slaOffset, executionStep);
                
            outputSkipStep.Set(context, false);
            outputBookmark.Set(context, trackerTable.ActivityName);
            outputStep.Set(context, documentStep);          
        }
    }
}
