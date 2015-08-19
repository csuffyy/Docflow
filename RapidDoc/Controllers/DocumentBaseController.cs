using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Services;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Extensions;
using RapidDoc.Models.Repository;
using System.Globalization;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;


namespace RapidDoc.Controllers
{
    public class DocumentBaseController : BasicController
    {
        private readonly IDocumentBaseService _Service;
        private readonly IDepartmentService _DepartmentService;
        private readonly IEmplService _EmplService;
        private readonly IProcessService _ProcessService;
        private readonly IDocumentService _DocumentService;

        protected UserManager<ApplicationUser> UserManager { get; private set; }

        public DocumentBaseController(IDocumentBaseService Service, IDepartmentService departmentService, IEmplService emplService, IProcessService processService, ICompanyService companyService, IAccountService accountService, IDocumentService documentService)
            : base(companyService, accountService)
        {
            _Service = Service;
            _DepartmentService = departmentService;
            _EmplService = emplService;
            _ProcessService = processService;
            _DocumentService = documentService;

            ApplicationDbContext dbContext = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
        }

        public ActionResult IndexRequest()
        {
            RequestBaseView requestBaseView = new RequestBaseView();
            
            requestBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            requestBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(requestBaseView);
        }

        public ActionResult IndexOfficeMemo()
        {
            OfficeMemoBaseView requestBaseView = new OfficeMemoBaseView();

            requestBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            requestBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(requestBaseView);
        }

        public ActionResult IndexTask()
        {
            TaskBaseView requestBaseView = new TaskBaseView();

            requestBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            requestBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(requestBaseView);
        }

        public ActionResult IndexOrder()
        {
            OrderBaseView requestBaseView = new OrderBaseView();

            requestBaseView.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            requestBaseView.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return View(requestBaseView);
        }
    
        [HttpPost]
        public ActionResult Search(DocumentType documentType, int filterType, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.FilterType = filterType;
            switch (documentType)
            {
                case DocumentType.Request:
                    return View("_DocumentBaseRequest", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.OfficeMemo:
                    return View("_DocumentBaseOfficeMemo", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.Task:
                    if (filterType == (int)TaskFilterType.Executors)
                        return View("_DocumentBaseTask", _Service.GetAllViewUserDocumentWithExecutors(documentType, startDate, endDate));
                    else
                        return View("_DocumentBaseTask", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
                case DocumentType.Order:
                    return View("_DocumentBaseOrder", _Service.GetAllViewUserDocument(documentType, startDate, endDate));
            }
            return new EmptyResult();
        }

    }
}