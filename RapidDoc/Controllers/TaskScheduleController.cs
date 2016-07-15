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
using System.IO;
using Simple.ImageResizer;
using System.Net;


namespace RapidDoc.Controllers
{
    public class TaskScheduleController : BasicController
    {
         private readonly ITaskScheduleService _Service;
         private readonly IDocumentService _DocumentService;
         private readonly ITaskScheduleHistroyService _TaskScheduleHistroyService;
         private readonly ISystemService _SystemService;

        public TaskScheduleController(ITaskScheduleService Service, ICompanyService companyService, IAccountService accountService, IDocumentService documentService, ITaskScheduleHistroyService taskScheduleHistroyService, ISystemService systemService)
            : base(companyService, accountService)
        {
            _Service = Service;
            _DocumentService = documentService;
            _TaskScheduleHistroyService = taskScheduleHistroyService;
            _SystemService = systemService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Grid()
        {
            List<TaskScheduleView> allList = new List<TaskScheduleView>();
            allList.AddRange(_Service.GetTaskScheduleView());
            allList.ForEach(x => { x.MainField = _SystemService.DeleteAllTags(_SystemService.DeleteAllSpecialCharacters(x.MainField));
                x.MainField = x.MainField.Count() > 10 ? x.MainField.Substring(0, 10) : x.MainField; });
            var grid = new TaskScheduleAjaxPagingGrid(allList, 1, false);
            return PartialView("_TaskScheduleGrid", grid);
        }

        public JsonResult GetList(int page)
        {
            List<TaskScheduleView> allList = new List<TaskScheduleView>();
            allList.AddRange(_Service.GetTaskScheduleView());
            allList.ForEach(x =>
            {
                x.MainField = _SystemService.DeleteAllTags(_SystemService.DeleteAllSpecialCharacters(x.MainField));
                x.MainField = x.MainField.Count() > 10 ? x.MainField.Substring(0, 10) : x.MainField;
            });

            var grid = new TaskScheduleAjaxPagingGrid(allList, page, true);

            return Json(new
            {
                Html = RenderPartialViewToString("_TaskScheduleGrid", grid),
                HasItems = grid.DisplayingItemsCount >= grid.Pager.PageSize
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            TaskScheduleView taskSchedule = new TaskScheduleView();
            taskSchedule.fileId = Guid.NewGuid();
            taskSchedule.RefDate = null;
            return View(taskSchedule);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(TaskScheduleView model)
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

        public ActionResult Edit(Guid id)
        {
            var model = _Service.FindView(id);     
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(TaskScheduleView model)
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

        public ActionResult Delete(Guid id)
        {
            var model = _Service.FindView(id);

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
        public JsonResult GetAllFileTaskSchedule(Guid id)
        {
            var statuses = new List<ViewDataUploadFilesResult>();
            var files = _Service.GetAllFilesTaskSchedule(id);
            ApplicationUser user = _AccountService.Find(User.Identity.GetUserId());

            foreach (var file in files)
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
        public JsonResult AjaxUpload(Guid fileId, HttpPostedFileBase files)
        {
            var statuses = new List<ViewDataUploadFilesResult>();
            System.IO.FileStream inFile;
            byte[] binaryData;
            string contentType;

            if (files != null && !string.IsNullOrEmpty(files.FileName) && fileId != Guid.Empty)
            {
                BinaryReader binaryReader = new BinaryReader(files.InputStream);
                byte[] data = binaryReader.ReadBytes(files.ContentLength);

                var thumbnail = new byte[] { };
                contentType = files.ContentType.ToString().ToUpper();
                thumbnail = GetThumbnail(data, contentType);

                // here you can save your file to the database...
                FileTable doc = new FileTable();
                doc.DocumentFileId = fileId;
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

        public ActionResult GetDelegationTaskModal(Guid taskScheduleId)
        {
            var model = _TaskScheduleHistroyService.GetTaskScheduleHistory(taskScheduleId);

            return PartialView("~/Views/TaskSchedule/_TaskScheduleHistory.cshtml", model);
        }

    }
}