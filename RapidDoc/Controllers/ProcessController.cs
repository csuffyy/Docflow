﻿using AutoMapper;
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
using Microsoft.AspNet.Identity.EntityFramework;
using System.IO;
using Simple.ImageResizer;

namespace RapidDoc.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ProcessController : BasicController
    {
        private readonly IProcessService _Service;
        private readonly IGroupProcessService _GroupProcessService;
        private readonly IWorkScheduleService _WorkScheduleService;
        private readonly IDocumentService _DocumentService;

        public ProcessController(IProcessService service, IGroupProcessService groupProcessService, IWorkScheduleService workScheduleService, ICompanyService companyService, IAccountService accountService, IDocumentService documentService)
            : base(companyService, accountService)
        {
            _Service = service;
            _GroupProcessService = groupProcessService;
            _WorkScheduleService = workScheduleService;
            _DocumentService = documentService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Grid()
        {
            var grid = new ProcessAjaxPagingGrid(_Service.GetAllView(), 1, false);
            return PartialView("_ProcessGrid", grid);
        }

        public JsonResult GetProcessList(int page)
        {
            var grid = new ProcessAjaxPagingGrid(_Service.GetAllView(), page, true);

            return Json(new
            {
                Html = RenderPartialViewToString("_ProcessGrid", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var items = Mapper.Map<IEnumerable<IdentityRole>, IEnumerable<RoleViewModel>>(context.Roles).ToList();
            items.Insert(0, new RoleViewModel { Name = UIElementRes.UIElement.NoValue, Id = String.Empty });
            ViewBag.RolesList = new SelectList(items, "Id", "Name", null);
            context.Dispose();

            ViewBag.GroupProcessList = _GroupProcessService.GetDropListGroupProcess(null);
            ViewBag.WorkScheduleList = _WorkScheduleService.GetDropListWorkSchedule(null);
            return View();
        }


        [HttpPost]
        public ActionResult Create(ProcessView model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.MandatoryFileTypes != null)
                        model.MandatoryFileTypes = model.MandatoryFileTypes.ToUpper();
                    _Service.Save(model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
                }
            }

            ApplicationDbContext context = new ApplicationDbContext();
            var items = Mapper.Map<IEnumerable<IdentityRole>, IEnumerable<RoleViewModel>>(context.Roles).ToList();
            items.Insert(0, new RoleViewModel { Name = UIElementRes.UIElement.NoValue, Id = String.Empty });
            ViewBag.RolesList = new SelectList(items, "Id", "Name", null);
            context.Dispose();

            ViewBag.GroupProcessList = _GroupProcessService.GetDropListGroupProcess(null);
            ViewBag.WorkScheduleList = _WorkScheduleService.GetDropListWorkSchedule(null);
            return View(model);
        }

        public ActionResult Edit(Guid id)
        {
            var model = _Service.FindView(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            ApplicationDbContext context = new ApplicationDbContext();
            var items = Mapper.Map<IEnumerable<IdentityRole>, IEnumerable<RoleViewModel>>(context.Roles).ToList();
            items.Insert(0, new RoleViewModel { Name = UIElementRes.UIElement.NoValue, Id = String.Empty });
            ViewBag.RolesList = new SelectList(items, "Id", "Name", model.RoleId);
            context.Dispose();

            ViewBag.GroupProcessList = _GroupProcessService.GetDropListGroupProcess(model.GroupProcessTableId);
            ViewBag.WorkScheduleList = _WorkScheduleService.GetDropListWorkSchedule(model.WorkScheduleTableId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ProcessView model, HttpPostedFileBase files, Guid documentFileId)
        {
            ActionResult view;

            if (files != null)
            {
                return view = AjaxUpload(files, documentFileId);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.MandatoryFileTypes != null)
                        model.MandatoryFileTypes = model.MandatoryFileTypes.ToUpper();
                    _Service.Save(model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.GetOriginalException().Message);
                }
            }

            ApplicationDbContext context = new ApplicationDbContext();
            var items = Mapper.Map<IEnumerable<IdentityRole>, IEnumerable<RoleViewModel>>(context.Roles).ToList();
            items.Insert(0, new RoleViewModel { Name = UIElementRes.UIElement.NoValue, Id = String.Empty });
            ViewBag.RolesList = new SelectList(items, "Id", "Name", model.RoleId);
            context.Dispose();

            ViewBag.GroupProcessList = _GroupProcessService.GetDropListGroupProcess(model.GroupProcessTableId);
            ViewBag.WorkScheduleList = _WorkScheduleService.GetDropListWorkSchedule(model.WorkScheduleTableId);
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

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            try
            {
                _DocumentService.DeleteFiles(id);                      
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

        public ActionResult Detail(Guid id)
        {
            var model = _Service.FindView(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }
        [HttpGet]
        public JsonResult GetAllFileDocument(Guid id)
        {
            var statuses = new List<RapidDoc.Controllers.DocumentController.ViewDataUploadFilesResult>();
            var files = _DocumentService.GetAllFilesDocument(id);

            foreach (var file in files)
            {
                var thumbnail = new byte[] { };
                System.IO.FileStream inFile;
                byte[] binaryData;

                if (file.Thumbnail.Length == 0)
                {
                    inFile = new System.IO.FileStream(Server.MapPath("~/Content/FileUpload/content-types/64/Text.png"),
                                System.IO.FileMode.Open,
                                System.IO.FileAccess.Read);
                    binaryData = new Byte[inFile.Length];
                    long bytesRead = inFile.Read(binaryData, 0,
                                         (int)inFile.Length);
                    inFile.Close();
                    thumbnail = binaryData;
                }
                else
                {
                    thumbnail = file.Thumbnail;
                }


                statuses.Add(new RapidDoc.Controllers.DocumentController.ViewDataUploadFilesResult()
                {
                    name = file.FileName,
                    size = file.ContentLength,
                    url = @"/Document/DownloadFile/" + file.Id.ToString(),
                    deleteUrl = @"/Document/DeleteFile/" + file.Id.ToString(),
                    thumbnailUrl = @"data:image/png;base64," + Convert.ToBase64String(thumbnail),
                    deleteType = "DELETE"
                });
            }

            var uploadedFiles = new
            {
                files = statuses.ToArray()
            };

            JsonResult result = Json(uploadedFiles, JsonRequestBehavior.AllowGet);
            result.ContentType = "text/plain";
            return result;
        }

        public JsonResult AjaxUpload(HttpPostedFileBase files, Guid documentFileId)
        {
            var statuses = new List<RapidDoc.Controllers.DocumentController.ViewDataUploadFilesResult>();
            System.IO.FileStream inFile;
            byte[] binaryData;
            string contentType;

            if (files != null && !string.IsNullOrEmpty(files.FileName) && documentFileId != Guid.Empty)
            {
                BinaryReader binaryReader = new BinaryReader(files.InputStream);
                byte[] data = binaryReader.ReadBytes(files.ContentLength);

                var thumbnail = new byte[] { };
                contentType = files.ContentType.ToString().ToUpper();
                thumbnail = GetThumbnail(data, contentType);

                // here you can save your file to the database...
                FileTable doc = new FileTable();
                doc.DocumentFileId = documentFileId;
                doc.FileName = files.FileName;
                doc.ContentType = contentType;
                doc.ContentLength = files.ContentLength;
                doc.Data = data;
                doc.Thumbnail = thumbnail;

                Guid Id = _DocumentService.SaveFile(doc);

                if (thumbnail.Length == 0)
                {
                    inFile = new System.IO.FileStream(Server.MapPath("~/Content/FileUpload/content-types/64/Text.png"),
                                System.IO.FileMode.Open,
                                System.IO.FileAccess.Read);
                    binaryData = new Byte[inFile.Length];
                    long bytesRead = inFile.Read(binaryData, 0,
                                         (int)inFile.Length);
                    inFile.Close();
                    thumbnail = binaryData;
                }

                statuses.Add(new RapidDoc.Controllers.DocumentController.ViewDataUploadFilesResult()
                {
                    name = doc.FileName,
                    size = doc.ContentLength,
                    url = @"/Document/DownloadFile/" + Id.ToString(),
                    deleteUrl = @"/Document/DeleteFile/" + Id.ToString(),
                    thumbnailUrl = @"data:image/png;base64," + Convert.ToBase64String(thumbnail),
                    deleteType = "DELETE"
                });
            }

            var uploadedFiles = new
            {
                files = statuses.ToArray()
            };

            JsonResult result = Json(uploadedFiles);
            result.ContentType = "text/plain";
         
            return result;
        }

        private byte[] GetThumbnail(byte[] fileData, string contentType)
        {
            var thumbnail = new byte[] { };

            if (contentType == "IMAGE/PNG"
                || contentType == "IMAGE/GIF"
                || contentType == "IMAGE/JPEG"
                || contentType == "IMAGE/BMP")
            {
                thumbnail = ImageResizer(fileData);
            }

            return thumbnail;
        }

        private byte[] ImageResizer(byte[] entireImage)
        {
            try
            {
                ImageResizer resizer = new ImageResizer(entireImage);
                return resizer.Resize(64, 64, false, ImageEncoding.Png);
            }
            catch (Exception)
            {
                return new byte[] { };
            }
        }
    }

}
