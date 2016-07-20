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

namespace RapidDoc.Controllers
{
    [Authorize(Roles = "Administrator, SetupAdministrator")]
    public class CompanyController : BasicController
    {
        private readonly IDomainService _DomainService;

        public CompanyController(ICompanyService companyService, IDomainService DomainService, IAccountService accountService)
            : base(companyService, accountService)
        {
            _DomainService = DomainService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Rename(Guid id)
        {
            ViewBag.CompanyId = id;
            return View();
        }

        public ActionResult Grid()
        {
            var grid = new CompanyAjaxPagingGrid(_CompanyService.GetAllView(), 1, false);
            return PartialView("_CompanyGrid", grid);
        }

        public ActionResult RenameGrid(Guid id)
        {
            var grid = new RenameCompanyAjaxPagingGrid(_CompanyService.GetPartialViewRename(x => x.CompanyTableId == id), 1, false);
            return PartialView("_RenameCompanyGrid", grid);
        }

        public JsonResult GetList(int page)
        {
            var grid = new CompanyAjaxPagingGrid(_CompanyService.GetAllView(), page, true);

            return Json(new
            {
                Html = RenderPartialViewToString("_CompanyGrid", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRenameList(Guid id, int page)
        {
            var grid = new RenameCompanyAjaxPagingGrid(_CompanyService.GetPartialViewRename(x => x.CompanyTableId == id), page, true);

            return Json(new
            {
                Html = RenderPartialViewToString("_RenameCompanyGrid", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            ViewBag.DomainList = _DomainService.GetDropListDomainNull(null);
            return View();
        }

        public ActionResult RenameCreate(Guid id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _CompanyService.Save(model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
                }
            }
            ViewBag.DomainList = _DomainService.GetDropListDomainNull(null);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenameCreate(RenameCompanyView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CompanyTableId = model.Id ?? Guid.Empty;
                    model.Id = null;
                    _CompanyService.SaveRename(model);
                    return RedirectToAction("Rename", new { id = model.CompanyTableId });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
                }
            }

            return View(model);
        }

        public ActionResult Edit(Guid id)
        {
            var model = _CompanyService.FindView(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.DomainList = _DomainService.GetDropListDomainNull(model.DomainTableId);
            return View(model);
        }

        public ActionResult RenameEdit(Guid id)
        {
            var model = _CompanyService.FindViewRename(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CompanyView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _CompanyService.Save(model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
                }
            }
            ViewBag.DomainList = _DomainService.GetDropListDomainNull(model.DomainTableId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenameEdit(RenameCompanyView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _CompanyService.SaveRename(model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
                }
            }

            return View(model);
        }

        public ActionResult Delete(Guid id)
        {
            var model = _CompanyService.FindView(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        public ActionResult RenameDelete(Guid id)
        {
            var model = _CompanyService.FindViewRename(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _CompanyService.Delete(id);
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
            }

            var model = _CompanyService.FindView(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost, ActionName("RenameDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult RenameDeleteConfirmed(Guid id)
        {
            try
            {
                var table = _CompanyService.FindRename(id);
                _CompanyService.DeleteRename(id);
                return RedirectToAction("Rename", new { id = table.CompanyTableId });
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
            }

            var model = _CompanyService.FindViewRename(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Detail(Guid id)
        {
            var model = _CompanyService.FindView(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        public ActionResult RenameDetail(Guid id)
        {
            var model = _CompanyService.FindViewRename(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }
    }
}
