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

namespace RapidDoc.Controllers
{
    [Authorize(Roles = "ActiveUser")]
    [Culture]
    public class BasicController : Controller
    {
        [Inject]
        public IMapper ModelMapper { get; set; }

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
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            Response.Cookies.Add(cookie);
            return Redirect(returnUrl);
        }

        public ActionResult ChangeCompany(string companyId, string returnUrl)
        {
            IAccountService serviceAccount = DependencyResolver.Current.GetService<IAccountService>();
            ICompanyService serviceCompany = DependencyResolver.Current.GetService<ICompanyService>();

            var company = serviceCompany.FirstOrDefault(x => x.AliasCompanyName == companyId);
            if (company == null)
            {
                return HttpNotFound();
            }

            var user = serviceAccount.FirstOrDefault(x => x.UserName == User.Identity.Name);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.CompanyTableId = company.Id;
            serviceAccount.SaveDomain(user);

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
    }
}
