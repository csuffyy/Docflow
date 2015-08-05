using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.CSharp.RuntimeBinder;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Interfaces;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using System.Text.RegularExpressions;

namespace RapidDoc.Models.Services
{
    public interface IDocumentBaseService
    {
        List<DocumentBaseView> GetAllViewUserDocument(DocumentType type, DateTime? startDate, DateTime? endDate);
    }

    public class DocumentBaseService : IDocumentBaseService
    {
        private IRepository<SearchTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IRepository<EmplTable> repoEmpl;
        private IUnitOfWork _uow;
        private readonly IDocumentService _DocumentService;
        private readonly ISystemService _SystemService;
        private readonly IItemCauseService _ItemCauseService;
        

        protected UserManager<ApplicationUser> UserManager { get; private set; }

        public DocumentBaseService(IUnitOfWork uow, IDocumentService documentService, ISystemService systemService, IItemCauseService itemCauseService)
        {
            _uow = uow;
            repo = uow.GetRepository<SearchTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            repoEmpl = uow.GetRepository<EmplTable>();
            _DocumentService = documentService;
            _SystemService = systemService;
            _ItemCauseService = itemCauseService;

            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_uow.GetDbContext<ApplicationDbContext>()));
        }

        public List<DocumentBaseView> GetAllViewUserDocument(DocumentType type, DateTime? startDate, DateTime? endDate)
        {
            List<DocumentBaseView> items, editedItems = new List<DocumentBaseView>();

            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            DateTime currentDate = DateTime.UtcNow;
            
            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            if (UserManager.IsInRole(user.Id, "Administrator"))
            {
                items = (from document in contextQuery.DocumentTable
                            where document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate)
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                            join department in contextQuery.DepartmentTable on empl.DepartmentTableId equals department.Id
                            orderby document.ModifiedDate descending
                            select new DocumentBaseView
                            {
                                ActivityName = document.ActivityName,
                                ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                                ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                                CompanyTableId = document.CompanyTableId,
                                CreatedDate = document.CreatedDate,
                                DocumentNum = document.DocumentNum,
                                DocumentState = document.DocumentState,
                                Id = document.Id,
                                ModifiedDate = document.ModifiedDate,
                                ProcessTableId = document.ProcessTableId,
                                AliasCompanyName = company.AliasCompanyName,
                                ProcessName = process.ProcessName,
                                CreatedBy = empl.SecondName + " " + empl.FirstName,
                                DepartmentName = department.DepartmentName,
                                UserName = empl.SecondName + " " + empl.FirstName + " " + empl.MiddleName,
                                DocumentRefId = document.RefDocumentId,
                                ProcessTableName = process.TableName
                            }).ToList();

            }
            else
            {
                    items = (from document in contextQuery.DocumentTable
                            where
                                (document.ApplicationUserCreatedId == user.Id ||
                                    contextQuery.ModificationUsersTable.Any(m => m.UserId == user.Id && m.DocumentTableId == document.Id && document.DocumentState == DocumentState.Created) ||
                                    contextQuery.WFTrackerTable.Any(x => x.DocumentTableId == document.Id && ((x.SignUserId == null && x.TrackerType == TrackerType.Waiting) ||
 
                                        (x.SignUserId == user.Id && (x.TrackerType == TrackerType.Approved || x.TrackerType == TrackerType.Cancelled))) && x.Users.Any(b => b.UserId == user.Id)) ||

                                    ((contextQuery.DocumentReaderTable.Any(r => r.DocumentTableId == document.Id && r.UserId == user.Id) || (

                                    contextQuery.DocumentReaderTable.Any(d => d.RoleId != null && d.DocumentTableId == document.Id && contextQuery.Roles.Where(r => r.Id == d.RoleId).ToList().Any(x => x.Users.ToList().Any(z => z.UserId == user.Id)))


                                        )) && document.DocumentState != DocumentState.Created) ||
                                    (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == user.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                                    && d.CompanyTableId == user.CompanyTableId
                                    && (d.GroupProcessTableId == document.ProcessTable.Id || d.GroupProcessTableId == null)
                                    && (d.ProcessTableId == document.ProcessTableId || d.ProcessTableId == null)
                                    && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.SignUserId == null && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId))
                                    ))
                                ) && document.DocType == type && (document.CreatedDate >= startDate && document.CreatedDate <= endDate)
                            join company in contextQuery.CompanyTable on document.CompanyTableId equals company.Id
                            join process in contextQuery.ProcessTable on document.ProcessTableId equals process.Id
                            let empl = contextQuery.EmplTable.Where(p => p.ApplicationUserId == document.ApplicationUserCreatedId).OrderByDescending(p => p.Enable).FirstOrDefault()
                            join department in contextQuery.DepartmentTable on empl.DepartmentTableId equals department.Id
                            orderby String.IsNullOrEmpty(document.ActivityName), document.ModifiedDate descending
                            select new DocumentBaseView
                            {
                                ActivityName = document.ActivityName,
                                ApplicationUserCreatedId = document.ApplicationUserCreatedId,
                                ApplicationUserModifiedId = document.ApplicationUserModifiedId,
                                CompanyTableId = document.CompanyTableId,
                                CreatedDate = document.CreatedDate,
                                DocumentNum = document.DocumentNum,
                                DocumentState = document.DocumentState,
                                Id = document.Id,
                                ModifiedDate = document.ModifiedDate,
                                ProcessTableId = document.ProcessTableId,
                                AliasCompanyName = company.AliasCompanyName,
                                ProcessName = process.ProcessName,
                                CreatedBy = empl.SecondName + " " + empl.FirstName,
                                DepartmentName = department.DepartmentName,
                                UserName = empl.SecondName + " " + empl.FirstName + " " + empl.MiddleName,
                                DocumentRefId = document.RefDocumentId,
                                ProcessTableName = process.TableName
                            }).ToList();                       
            }
            switch (type)
	        {
		        case DocumentType.Request:                   
                    return items;
                                     
                case DocumentType.OfficeMemo:
                        foreach (var item in items)
                        {                          
                            var documentView = _DocumentService.GetDocumentView(item.DocumentRefId, item.ProcessTableName);
                            item.ItemCaseNumber = documentView.ItemCauseNumber;
                            if (documentView.ItemCauseTableId != Guid.Empty && documentView.ItemCauseTableId != null)
                            item.ItemCaseName = _ItemCauseService.Find((Guid)documentView.ItemCauseTableId).CaseName;
                            item.DocumentTitle = documentView._DocumentTitle;
                            editedItems.Add(item);
                        }

                        return editedItems;
                case DocumentType.Task:
                        return items;
                case DocumentType.Order:
                        foreach (var item in items)
                        {
                            var documentView = _DocumentService.GetDocumentView(item.DocumentRefId, item.ProcessTableName);
                            if (!String.IsNullOrEmpty(documentView.OrderNum))
                                item.OrderNumber = documentView.OrderNum;
                            item.DocumentTitle = documentView.Subject;
                            editedItems.Add(item);
                        }
                        return editedItems;
             }

             return null;
            }
        }
    }
