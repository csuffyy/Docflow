using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Services;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Extensions;
using RapidDoc.Models.Grids;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RapidDoc.Controllers
{
    [Authorize(Roles = "Administrator, SetupAdministrator")]
    public class PortalParametersController : BasicController
    {
        private readonly IPortalParametersService _Service;
        private readonly IDepartmentService _DepartmentService;
        private readonly IEmplService _EmplService;

        public PortalParametersController(IPortalParametersService Service, ICompanyService companyService, IAccountService accountService, IDepartmentService departmentService, IEmplService emplService)
            : base(companyService, accountService)
        {
            _Service = Service;
            _DepartmentService = departmentService;
            _EmplService = emplService;
        }
       
        public ActionResult Edit()
        {          
            var model = _Service.GetAllView().FirstOrDefault();

            if (model == null)
            {
                PortalParametersTable domainTable = new PortalParametersTable { ReportDepartments = "" };

                _Service.SaveDomain(domainTable);

                return RedirectToAction("Edit");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(PortalParametersView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _Service.Save(model);
                    return RedirectToAction("Edit");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
                }
            }

            return View(model);
        }

       
    }
}