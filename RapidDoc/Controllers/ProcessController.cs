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
using Microsoft.AspNet.Identity;

namespace RapidDoc.Controllers
{
    [Authorize(Roles = "Administrator, SetupAdministrator")]
    public class ProcessController : BasicController
    {
        private readonly IProcessService _Service;
        private readonly IGroupProcessService _GroupProcessService;
        private readonly IWorkScheduleService _WorkScheduleService;
        private readonly IDocumentService _DocumentService;

        protected RoleManager<ApplicationRole> RoleManager { get; private set; }

        public ProcessController(IProcessService service, IGroupProcessService groupProcessService, IWorkScheduleService workScheduleService, ICompanyService companyService, IAccountService accountService, IDocumentService documentService)
            : base(companyService, accountService)
        {
            _Service = service;
            _GroupProcessService = groupProcessService;
            _WorkScheduleService = workScheduleService;
            _DocumentService = documentService;
            RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(new ApplicationDbContext()));
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

        public JsonResult GetList(int page)
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
            var items = Mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleViewModel>>(RoleManager.Roles).ToList();
            items.Insert(0, new RoleViewModel { Name = UIElementRes.UIElement.NoValue, Id = String.Empty });
            ViewBag.RolesList = new SelectList(items, "Id", "Name", null);
            ViewBag.DocumentBaseRolesList = new SelectList(items, "Id", "Name", null);
            ViewBag.StartReaderRolesList = new SelectList(items, "Id", "Name", null);
            ViewBag.AfterEndRolesList = new SelectList(items, "Id", "Name", null);
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

            var items = Mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleViewModel>>(RoleManager.Roles).ToList();
            items.Insert(0, new RoleViewModel { Name = UIElementRes.UIElement.NoValue, Id = String.Empty });
            ViewBag.RolesList = new SelectList(items, "Id", "Name", null);
            ViewBag.DocumentBaseRolesList = new SelectList(items, "Id", "Name", null);
            ViewBag.StartReaderRolesList = new SelectList(items, "Id", "Name", null);
            ViewBag.AfterEndRolesList = new SelectList(items, "Id", "Name", null);
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

            using (ApplicationDbContext dbContext = new ApplicationDbContext())
            {
                RoleManager<ApplicationRole> RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(dbContext));
                var items = Mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleViewModel>>(RoleManager.Roles).ToList();
                items.Insert(0, new RoleViewModel { Name = UIElementRes.UIElement.NoValue, Id = String.Empty });
                ViewBag.RolesList = new SelectList(items, "Id", "Name", model.RoleId);
                ViewBag.DocumentBaseRolesList = new SelectList(items, "Id", "Name", model.DocumentBaseRoleId);
                ViewBag.StartReaderRolesList = new SelectList(items, "Id", "Name", model.StartReaderRoleId);
                ViewBag.AfterEndRolesList = new SelectList(items, "Id", "Name", model.AfterEndReaderRoleId);
                ViewBag.GroupProcessList = _GroupProcessService.GetDropListGroupProcess(model.GroupProcessTableId);
                ViewBag.WorkScheduleList = _WorkScheduleService.GetDropListWorkSchedule(model.WorkScheduleTableId);
            }

            return View(model);
        }

        [HttpGet]
        public FileContentResult DownloadFile(Guid Id)
        {
            FileTable fileTable = _DocumentService.GetFile(Id);
            return File(fileTable.Data, fileTable.ContentType, fileTable.FileName);
        }

        [HttpPost]
        public ActionResult Edit(ProcessView model)
        {
            var filesXaml = _DocumentService.GetAllXAMLDocument(GuidNull2Guid(model.Id)).FirstOrDefault();
            if (filesXaml == null && model.isApproved == true)
                ModelState.AddModelError(string.Empty,  ValidationRes.ValidationResource.ErrorProcessXAML);

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

            var items = Mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleViewModel>>(RoleManager.Roles).ToList();
            items.Insert(0, new RoleViewModel { Name = UIElementRes.UIElement.NoValue, Id = String.Empty });
            ViewBag.RolesList = new SelectList(items, "Id", "Name", model.RoleId);
            ViewBag.DocumentBaseRolesList = new SelectList(items, "Id", "Name", model.DocumentBaseRoleId);
            ViewBag.StartReaderRolesList = new SelectList(items, "Id", "Name", model.StartReaderRoleId);
            ViewBag.AfterEndRolesList = new SelectList(items, "Id", "Name", model.AfterEndReaderRoleId);
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
            var statuses = new List<ViewDataUploadFilesResult>();
            var files = _DocumentService.GetAllFilesDocument(id);

            foreach (var file in files.Where(x => x.ContentType != "APPLICATION/XAML+XML"))
            {
                var thumbnail = new byte[] { };
                System.IO.FileStream inFile;
                byte[] binaryData;

                if (file.Thumbnail != null && file.Thumbnail.Length == 0)
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


                statuses.Add(new ViewDataUploadFilesResult()
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

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult AjaxUpload(HttpPostedFileBase files, Guid documentId)
        {
            var statuses = new List<ViewDataUploadFilesResult>();
            System.IO.FileStream inFile;
            byte[] binaryData;
            string contentType;

            if (files != null && !string.IsNullOrEmpty(files.FileName) && documentId != Guid.Empty)
            {
                BinaryReader binaryReader = new BinaryReader(files.InputStream);
                byte[] data = binaryReader.ReadBytes(files.ContentLength);

                var thumbnail = new byte[] { };
                contentType = files.ContentType.ToString().ToUpper();
                thumbnail = GetThumbnail(data, contentType);

                // here you can save your file to the database...
                FileTable doc = new FileTable();
                doc.DocumentFileId = documentId;
                doc.FileName = files.FileName;
                doc.ContentType = contentType;
                doc.ContentLength = files.ContentLength;
                doc.Data = data;
                doc.Thumbnail = thumbnail;

                Guid Id = _DocumentService.SaveFile(doc, User.Identity.GetUserId());

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

                statuses.Add(new ViewDataUploadFilesResult()
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

        [HttpPost]
        public ActionResult FileUploadWF(HttpPostedFileBase fileWF, Guid id)
        {
            if (fileWF != null)
            {
                if (fileWF != null && fileWF.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(fileWF.FileName);
                    string contentType = fileWF.ContentType.ToString().ToUpper();
                    BinaryReader binaryReader = new BinaryReader(fileWF.InputStream);
                    byte[] data = binaryReader.ReadBytes(fileWF.ContentLength);

                    FileTable fileTable = _DocumentService.GetAllXAMLDocument(id).OrderByDescending(x => Convert.ToInt32(x.Version)).FirstOrDefault();
                    string currentVersion = "0";

                    if (fileTable != null)
                    {
                        currentVersion = fileTable.Version;
                    }

                    FileTable doc = new FileTable();
                    doc.DocumentFileId = id;
                    doc.FileName = fileName;
                    doc.ContentType = contentType;
                    doc.ContentLength = fileWF.ContentLength;
                    doc.Data = data;
                    doc.Version = Convert.ToString(Convert.ToInt32(currentVersion) + 1);
                    doc.VersionName = "Version " + Convert.ToString(Convert.ToInt32(currentVersion) + 1);
                    _DocumentService.SaveFile(doc, User.Identity.GetUserId());

                    return DownloadFileWF(id);
                }
            }

            return null;
        }

        public ActionResult DownloadFileWF(Guid processId)
        {
            var files = _DocumentService.GetAllFilesDocument(processId).Where(x => x.ContentType == "APPLICATION/XAML+XML").OrderByDescending(x => x.CreatedDate).ToList();
            return PartialView("_DownloadFileWF", files);
        }

        public ActionResult DeleteFileWF(Guid id, Guid processId)
        {
            _DocumentService.DeleteFile(id);
            var files = _DocumentService.GetAllFilesDocument(processId).Where(x => x.ContentType == "APPLICATION/XAML+XML").ToList();
            return RedirectToAction("Edit", new { id = processId });
        }

        [HttpPost]
        public void UpdateFileWF(Guid id, string versionName, string version, string versionComments)
        {
            FileTable file = _DocumentService.GetFile(id);

            if(file != null)
            {
                if(versionName != "undefined")
                    file.VersionName = versionName;

                if (version != "undefined")
                    file.Version = version;

                if (versionComments != "undefined")
                    file.VersionComments = versionComments;
                _DocumentService.UpdateFile(file);
            }
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

        public string GetProcessName(Guid? id)
        {
            DocumentTable document = _DocumentService.FirstOrDefault(x => x.Id == id);
            if (document != null)
                return document.ProcessName;

            return String.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && RoleManager != null)
            {
                RoleManager.Dispose();
                RoleManager = null;
            }
            base.Dispose(disposing);
        }
    }

}
