using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Services;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;

namespace RapidDoc.Controllers
{
    public class ErrorController : BasicController
    {
        private readonly IHistoryUserService _HistoryUserService;

        public ErrorController(ICompanyService companyService, IAccountService accountService, IHistoryUserService historyUserService)
            : base(companyService, accountService)
        {
            _HistoryUserService = historyUserService;
        }

        //public ActionResult PageNotFound()
        //{
        //    return View();
        //}

        public ActionResult PageNotFound(Guid? id)
        {
            if (id != null)
            {
                List<HistoryUserView> history = _HistoryUserService.GetPartialView(x => x.DocumentTableId == id && x.DocumentNum != null).ToList();
                return View(history);
            }
            else
                return View();
        }

        public ActionResult Exception()
        {
            var exception = Session["application_error"] as Exception;

            IEnumerable<String> model = new List<String>();

            if (exception != null)
            {
                model = GetExceptionDescription(exception);
            }

            return View(model);
        }

        private static IEnumerable<String> GetExceptionDescription(Exception ex)
        {
            var list = new List<String> { ex.Message };

            if (ex.InnerException != null)
            {
                list.AddRange(GetExceptionDescription(ex.InnerException));
            }

            return list;
        }
    }
}