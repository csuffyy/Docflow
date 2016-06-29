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
using RapidDoc.Models.Grids;

namespace RapidDoc.Controllers
{
    [Authorize(Roles = "Administrator, SetupAdministrator")]
    public class NumberSeqController : BasicController
    {
        private readonly INumberSeqService _Service;

        public NumberSeqController(INumberSeqService Service, ICompanyService companyService, IAccountService accountService)
            : base(companyService, accountService)
        {
            _Service = Service;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Booking(Guid id)
        {
            ViewBag.NumberSeqId = id;
            return View();
        }

        public ActionResult Grid()
        {
            var grid = new NumberSeriesAjaxPagingGrid(_Service.GetAllView(), 1, false);
            return PartialView("_NumberSeriesGrid", grid);
        }

        public ActionResult BookingGrid(Guid id)
        {
            var grid = new NumberSeriesBookingAjaxPagingGrid(_Service.GetPartialViewBooking(x => x.NumberSeriesTableId == id && x.Enable == true), 1, false);
            return PartialView("_NumberSeriesBookingGrid", grid);
        }

        public JsonResult GetList(int page)
        {
            var grid = new NumberSeriesAjaxPagingGrid(_Service.GetAllView(), page, true);

            return Json(new
            {
                Html = RenderPartialViewToString("_NumberSeriesGrid", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBookingList(Guid id, int page)
        {
            var grid = new NumberSeriesBookingAjaxPagingGrid(_Service.GetPartialViewBooking(x => x.NumberSeriesTableId == id && x.Enable == true), page, true);

            return Json(new
            {
                Html = RenderPartialViewToString("_NumberSeriesBookingGrid", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NumberSeriesView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _Service.Save(model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
                }
            }

            return View(model);
        }

        public ActionResult BookingCreate(Guid id)
        {
            NumberSeriesTable numberTable = _Service.Find(id);
            NumberSeriesBookingView model = new NumberSeriesBookingView();
            model.Prefix = numberTable.Prefix;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookingCreate(NumberSeriesBookingView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.NumberSeriesTableId = model.Id ?? Guid.Empty;
                    model.Id = null;
                    model.Enable = true;
                    _Service.SaveBooking(model);
                    return RedirectToAction("Booking", new { id = model.NumberSeriesTableId });
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
            var model = _Service.FindView(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        public ActionResult BookingEdit(Guid id)
        {
            var model = _Service.FindViewBooking(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NumberSeriesView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _Service.Save(model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookingEdit(NumberSeriesBookingView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Enable = true;
                    _Service.SaveBooking(model);
                    return RedirectToAction("Booking", new { id = model.NumberSeriesTableId });
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
            var model = _Service.FindView(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        public ActionResult BookingDelete(Guid id)
        {
            var model = _Service.FindViewBooking(id);

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
                _Service.Delete(id);
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
            }

            var model = _Service.FindView(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost, ActionName("BookingDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult BookingDeleteConfirmed(Guid id)
        {
            try
            {
                var table = _Service.FindBooking(id);
                _Service.DeleteBooking(id);
                return RedirectToAction("Booking", new { id = table.NumberSeriesTableId });
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
            }

            var model = _Service.FindViewBooking(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult Detail(Guid id)
        {
            var model = _Service.FindView(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        public ActionResult BookingDetail(Guid id)
        {
            var model = _Service.FindViewBooking(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }
    }
}
