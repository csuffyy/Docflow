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
using System.Web;
using RapidDoc;
using RapidDoc.Models.Infrastructure;
using System.Runtime.CompilerServices;
using System.Web.Mvc;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using System.ComponentModel;

namespace RapidDoc.Activities
{
    
    public sealed class WFChooseUpManager : CodeActivity
    {
        public InArgument<string> inputSystemName { get; set; }

        [RequiredArgument]
        public InArgument<Guid> inputDocumentId { get; set; }

        [RequiredArgument]
        public InArgument<int> inputLevel { get; set; }

        [RequiredArgument]
        public InArgument<string> inputCurrentUser { get; set; }

        [RequiredArgument]
        public InArgument<DocumentState> inputStep { get; set; }

        [RequiredArgument]
        public OutArgument<string> outputBookmark { get; set; }

        public InArgument<bool> useManual { get; set; }

        public InArgument<int> slaOffset { get; set; }

        [RequiredArgument]
        public InArgument<string> profileName { get; set; }

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
            int level = context.GetValue(this.inputLevel);
            Guid documentId = context.GetValue(this.inputDocumentId);
            DocumentState documentStep = context.GetValue(this.inputStep);
            string currentUserId = context.GetValue(this.inputCurrentUser); 
            bool useManual = context.GetValue(this.useManual);
            int slaOffset = context.GetValue(this.slaOffset);
            string profileName = context.GetValue(this.profileName);
            bool executionStep = context.GetValue(this.executionStep);
            bool noneSkipStep = context.GetValue(this.noneSkip);

            _service = DependencyResolver.Current.GetService<IWorkflowService>();
            WFUserFunctionResult userFunctionResult = _service.WFMatchingUpManager(documentId, currentUserId, level, profileName);

            if (executionStep == true || noneSkipStep == true || userFunctionResult.Skip == false)
            {
                _service.CreateTrackerRecord(systemName, documentStep, documentId, this.DisplayName, userFunctionResult.Users, currentUserId, this.Id, useManual, slaOffset, executionStep);
                outputSkipStep.Set(context, false);
            }
            else
                outputSkipStep.Set(context, true);

            outputBookmark.Set(context, this.DisplayName);
            outputStep.Set(context, documentStep);
        }
    }
}
