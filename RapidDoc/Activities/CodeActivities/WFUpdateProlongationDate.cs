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
using System.ComponentModel;
using RapidDoc.Models.Repository;


namespace RapidDoc.Activities.CodeActivities
{

    public sealed class WFUpdateProlongationDate : CodeActivity
    {       
        [RequiredArgument]
        public InArgument<Guid> RefDocId { get; set; }

        [RequiredArgument]
        public InArgument<DateTime> ProlongationDate { get; set; }

        [RequiredArgument]
        public InArgument<string> inputCurrentUser { get; set; }

        [Browsable(false)]
        [Inject]
        public IDocumentService _service { get; set; }

        [Browsable(false)]
        [Inject]
        public IEmailService _serviceEmail { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Guid refDocId = context.GetValue(this.RefDocId);
            DateTime prolongationDate = context.GetValue(this.ProlongationDate);
            string currentUserId = context.GetValue(this.inputCurrentUser);

            _service = DependencyResolver.Current.GetService<IDocumentService>();
            Guid? documentRef = _service.UpdateProlongationDate(refDocId, prolongationDate, currentUserId);

            if (documentRef != null)
            {
                DocumentTable documentTableRef = _service.Find(documentRef);
                DocumentTable documentTable = _service.Find(refDocId);

                if (documentTableRef != null && documentTableRef.DocType == DocumentType.Protocol)
                {
                    _serviceEmail = DependencyResolver.Current.GetService<IEmailService>();
                    _serviceEmail.SendProlongationResultInitiator(documentTableRef.Id, documentTableRef.ApplicationUserCreatedId, prolongationDate, documentTable.DocumentNum, documentTable.DocumentText);
                }
            }
        }
    }
}
