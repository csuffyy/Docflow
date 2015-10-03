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

    public sealed class WFRegistrationOUT : CodeActivity
    {
        [Browsable(false)]
        [Inject]
        public IDocumentService _service { get; set; }

        [RequiredArgument]
        public InArgument<Dictionary<string, Object>> inputDocumentData { get; set; }

        [RequiredArgument]
        public InArgument<Guid> inputDocumentId { get; set; }

        [RequiredArgument]
        public InArgument<string> inputCurrentUser { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            _service = DependencyResolver.Current.GetService<IDocumentService>();

            Dictionary<string, Object> documentData = context.GetValue(this.inputDocumentData);
            Guid documentId = context.GetValue(this.inputDocumentId);
            string currentUserId = context.GetValue(this.inputCurrentUser);
            var document = _service.Find(documentId);
            Guid? bookingNumberId = Guid.Empty;

            if (documentData.ContainsKey("NumberSeriesBookingTableId"))
            {
                if ((Guid?)documentData["NumberSeriesBookingTableId"] != null)
                    bookingNumberId = (Guid)documentData["NumberSeriesBookingTableId"];
            }
            _service.OUTRegistration(documentId, currentUserId, bookingNumberId);
        }
    }
}
