﻿using Ninject;
using RapidDoc.Filters;
using RapidDoc.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Models.Services;
using System.IO;
using RapidDoc.Models.Repository;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RapidDoc.Models.DomainModels;
using Microsoft.AspNet.Identity;
using RapidDoc.Models.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Routing;

namespace RapidDoc.Controllers
{
    [Authorize(Roles = "ActiveUser")]
    [Culture]
    public class BasicController : AsyncController
    {
        protected readonly ICompanyService _CompanyService;
        protected readonly IAccountService _AccountService;

        public BasicController(ICompanyService companyService, IAccountService accountService)
        {
            _CompanyService = companyService;
            _AccountService = accountService;
        }

        [Inject]
        public IMapper ModelMapper { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (filterContext.RouteData.Values.Any(x => x.Key == "company"))
            {
                if (filterContext.ActionParameters.Count() == 0)
                {
                    var companyId = filterContext.RouteData.Values["company"].ToString();
                    ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());

                    if (companyId != user.AliasCompanyName)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { 
                            { "controller", filterContext.RouteData.Values["controller"].ToString() }, 
                            { "action", filterContext.RouteData.Values["action"].ToString() }, 
                            { "company", user.AliasCompanyName } 
                        });
                        return;
                    }
                }
                else
                {
                    var companyId = filterContext.RouteData.Values["company"].ToString();
                    if (filterContext.RouteData.RouteHandler != null && !String.IsNullOrEmpty(companyId))
                    {
                        ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
                        if (user != null)
                        {
                            if (user.AliasCompanyName != companyId)
                            {
                                using (ApplicationDbContext dbContext = new ApplicationDbContext())
                                {
                                    UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));

                                    if (UserManager.IsInRole(user.Id, "ChangeCompany") || UserManager.IsInRole(user.Id, "Administrator"))
                                    {
                                        var companyList = _CompanyService.GetAll().ToList();
                                        if (companyList != null)
                                        {
                                            var company = companyList.FirstOrDefault(x => x.AliasCompanyName == companyId);
                                            if (company != null)
                                            {
                                                user.CompanyTableId = company.Id;
                                                _AccountService.SaveDomain(user);
                                            }
                                        }
                                    }
                                    UserManager.Dispose();
                                    UserManager = null;
                                }
                            }
                        }
                    }
                }
            }
        }

        public ActionResult ChangeCulture(string id)
        {
            string returnUrl = Request.UrlReferrer.PathAndQuery;

            List<string> cultures = Lang.GetISOCodes();
            if (!cultures.Contains(id))
            {
                id = Lang.DefaultLang();
            }

            HttpCookie cookie = Request.Cookies["lang"];
            if (cookie != null)
                cookie.Value = id;
            else
            {

                cookie = new HttpCookie("lang");
                cookie.HttpOnly = false;
                cookie.Value = id;
                cookie.Expires = DateTime.UtcNow.AddYears(1);
            }
            Response.Cookies.Add(cookie);
            return Redirect(returnUrl);
        }

        [Authorize(Roles = "ChangeCompany, Administrator")]
        public ActionResult ChangeCompany(string companyId, string returnUrl)
        {
            var company = _CompanyService.FirstOrDefault(x => x.AliasCompanyName == companyId);
            if (company == null)
                return HttpNotFound();

            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());
            if (user == null)
                return HttpNotFound();

            user.CompanyTableId = company.Id;
            _AccountService.SaveDomain(user);

            if (returnUrl.Length >= 4)
            {
                returnUrl = returnUrl.Substring(4);
            }
            return Redirect("/" + companyId.ToString());
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        protected string GetAttributeDisplayName(PropertyInfo property)
        {
            string result = String.Empty;
            var atts = property.GetCustomAttributes(
                typeof(DisplayAttribute), true);
            if (atts.Length == 0)
                return result;

            var displayAttr = ((DisplayAttribute)atts[0]);
            result = displayAttr.Name;

            if (displayAttr.ResourceType != null)
                result = displayAttr.GetName();

            return result;
        }

        public Guid GuidNull2Guid(Guid? value)
        {
            return value ?? Guid.Empty;
        }

        public DateTime GetLocalTime(DateTime value, string timeZone)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(value), timeZoneInfo);
        }

        [AllowAnonymous]
        [OutputCache(Duration = 43200, VaryByParam = "none")]
        public ActionResult GetCompanyList()
        {
            return PartialView("_CompanyList", _CompanyService.GetAllView());
        }
    }
}
