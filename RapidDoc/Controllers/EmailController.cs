﻿using AutoMapper;
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
using System.IO;

namespace RapidDoc.Controllers
{
    [Authorize(Roles = "Administrator, SetupAdministrator")]
    public class EmailController : BasicController
    {
        private readonly IEmailService _Service;

        public EmailController(IEmailService Service, ICompanyService companyService, IAccountService accountService)
            : base(companyService, accountService)
        {
            _Service = Service;
        }

        public ActionResult Edit()
        {
            var model = _Service.FirstOrDefaultView(x => x.SmtpServer != String.Empty);

            if (model == null)
            {
                _Service.InitializeMailParameter();
                model = _Service.FirstOrDefaultView(x => x.SmtpServer != String.Empty);
                if (model == null)
                {
                    return HttpNotFound();
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EmailParameterView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    EmailParameterTable emailTable = _Service.FirstOrDefault(x => x.Id != null);
                    if (emailTable.SuperPass != model.SuperPass)
                        model.SuperPass = _Service.CryptStringSHA256(model.SuperPass);

                    _Service.Save(model);
                    return RedirectToAction("Index", "Document");
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
