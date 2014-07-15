﻿using System;
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

namespace RapidDoc.Activities
{

    public sealed class WFChooseSpecificUser : CodeActivity
    {
        public InArgument<string> inputUserName { get; set; }
        public InArgument<Guid> inputDocumentId { get; set; }
        public InArgument<string> inputCurrentUser { get; set; }
        public InArgument<DocumentState> inputStep { get; set; }
        public OutArgument<string> outputBookmark { get; set; }
        public InArgument<bool> useManual { get; set; }
        public InArgument<int> slaOffset { get; set; }
        public InArgument<bool> executionStep { get; set; }
        public OutArgument<bool> outputSkipStep { get; set; }
        public OutArgument<DocumentState> outputStep { get; set; }

        [Inject]
        public IWorkflowService _service { get; set; }
     
        protected override void Execute(CodeActivityContext context)
        {
            string userName = context.GetValue(this.inputUserName);
            Guid documentId = context.GetValue(this.inputDocumentId);
            DocumentState documentStep = context.GetValue(this.inputStep);
            string currentUser = context.GetValue(this.inputCurrentUser);
            bool useManual = context.GetValue(this.useManual);
            int slaOffset = context.GetValue(this.slaOffset);
            bool executionStep = context.GetValue(this.executionStep);

            _service = DependencyResolver.Current.GetService<IWorkflowService>();
            WFUserFunctionResult userFunctionResult = _service.WFSpecificUser(documentId, userName);

            if (userFunctionResult.Skip == false) 
                _service.CreateTrackerRecord(documentStep, documentId, this.DisplayName, userFunctionResult.Users,  currentUser, this.Id, useManual, slaOffset, executionStep);
            else if(executionStep == true)
                _service.CreateTrackerRecord(documentStep, documentId, this.DisplayName, userFunctionResult.Users, currentUser, this.Id, useManual, slaOffset, executionStep);
            outputBookmark.Set(context, this.DisplayName.Replace("<step>", ""));
            outputSkipStep.Set(context, executionStep ? false : userFunctionResult.Skip);
            outputStep.Set(context, documentStep);
        }
    }
}
