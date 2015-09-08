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

    public sealed class WFCheckParentDepartment : CodeActivity<bool>
    {
        [Browsable(false)]
        [Inject]
        public IDocumentService _service { get; set; }


        [Browsable(false)]
        [Inject]
        public IDepartmentService _serviceDepartment { get; set; }

        [RequiredArgument]
        public InArgument<string> inputGroupName { get; set; }

        [RequiredArgument]
        public InArgument<Guid> inputDocumentId { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            _service = DependencyResolver.Current.GetService<IDocumentService>();
            _serviceDepartment = DependencyResolver.Current.GetService<IDepartmentService>();

            Guid documentId = context.GetValue(this.inputDocumentId);
            string groupName = context.GetValue(this.inputGroupName);

            DocumentTable docTable = _service.Find(documentId);

            return _serviceDepartment.checkParentDepartment(groupName, docTable.ApplicationUserCreatedId);
        }
    }
}
