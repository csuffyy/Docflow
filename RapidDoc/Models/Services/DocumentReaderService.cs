using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Transactions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RapidDoc.Models.Services
{
    public interface IDocumentReaderService
    {
        IEnumerable<DocumentReaderTable> GetAll();
        IEnumerable<DocumentReaderTable> GetPartial(Expression<Func<DocumentReaderTable, bool>> predicate);
        DocumentReaderTable FirstOrDefault(Expression<Func<DocumentReaderTable, bool>> predicate);
        bool Contains(Expression<Func<DocumentReaderTable, bool>> predicate);
        bool ContainsRoleUser(Guid documentId, string userId);
        List<string> SaveReader(DocumentTable document, string[] listdata, string currentUserId);
        List<string> SaveOrderReader(DocumentTable document, string[] listdata, string currentUserId);
        List<string> AddReader(DocumentTable document, List<IdentityUserRole> listdata);
        void SaveDomain(DocumentReaderTable domainTable, string currentUserId = "");
        void Delete(Guid documentId);
        void Delete(Expression<Func<DocumentReaderTable, bool>> predicate);
        DocumentReaderTable Find(Guid id);
    }

    public class DocumentReaderService : IDocumentReaderService
    {
        private IRepository<DocumentReaderTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork _uow;
        private readonly IEmplService _EmplService;
        private readonly IHistoryUserService _HistoryUserService;
        protected RoleManager<ApplicationRole> RoleManager { get; private set; }

        public DocumentReaderService(IUnitOfWork uow, IEmplService emplService, IHistoryUserService historyUserService)
        {
            _uow = uow;
            repo = uow.GetRepository<DocumentReaderTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(_uow.GetDbContext<ApplicationDbContext>()));
            _EmplService = emplService;
            _HistoryUserService = historyUserService;
        }
        public IEnumerable<DocumentReaderTable> GetAll()
        {
            return repo.All();
        }
        public IEnumerable<DocumentReaderTable> GetPartial(Expression<Func<DocumentReaderTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public DocumentReaderTable FirstOrDefault(Expression<Func<DocumentReaderTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }
        public bool Contains(Expression<Func<DocumentReaderTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }
        public List<string> SaveReader(DocumentTable document, string[] listdata, string currentUserId)
        {
            List<string> newReader = new List<string>();
            string addReadersDescription = String.Empty;
            string removeReadersDescription = String.Empty;
            ApplicationUser currentUser = repoUser.GetById(currentUserId);
            List<EmplTable> emplList = _EmplService.GetAllIntercompany().ToList();

            if (listdata != null)
            {
                foreach (string userId in listdata)
                {
                    if ((Contains(x => x.DocumentTableId == document.Id && x.UserId == userId && x.RoleId == null) == false) &&
                        (Contains(x => x.DocumentTableId == document.Id && x.RoleId == userId) == false))
                    {
                        var empl = _EmplService.GetEmployer(userId, currentUser.CompanyTableId);
                        if (empl == null)
                        {
                            ApplicationRole role = RoleManager.FindById(userId);

                            if (role != null)
                            {
                                var names = role.Users;
                                if (names != null && names.Count() > 0)
                                {
                                    DocumentReaderTable reader = new DocumentReaderTable();
                                    reader.DocumentTableId = document.Id;
                                    reader.UserId = userId;
                                    reader.RoleId = userId;
                                    SaveDomain(reader, currentUserId);

                                    foreach (IdentityUserRole name in names)
                                    {
                                        if (emplList.Exists(x => x.ApplicationUserId == name.UserId && x.Enable == true))
                                        {
                                            newReader.Add(name.UserId);
                                        }
                                    }

                                    addReadersDescription += role.Description + "; ";
                                }
                            }
                        }
                        else
                        {
                            if (empl.Enable == true)
                            {
                                addReadersDescription += empl.FullName + "; ";

                                DocumentReaderTable reader = new DocumentReaderTable();
                                reader.DocumentTableId = document.Id;
                                reader.UserId = userId;
                                SaveDomain(reader, currentUserId);
                                newReader.Add(userId);
                            }
                        }
                    }
                }
            }

            var currentReaders = GetPartial(x => x.DocumentTableId == document.Id && x.RoleId == null).ToList();
            var currentReadersGroup = GetPartial(x => x.DocumentTableId == document.Id && x.RoleId != null).GroupBy(x => new { RoleId = x.RoleId, CreateUserId = x.ApplicationUserCreatedId }).ToList();
            if (listdata == null)
                Delete(x => x.DocumentTableId == document.Id && x.ApplicationUserCreatedId == currentUserId);

            foreach (var item in currentReaders)
            {
                if (item.ApplicationUserCreatedId == currentUserId)
                {
                    if (listdata != null)
                    {
                        if (listdata.Contains(item.UserId) == false)
                        {
                            var empl = _EmplService.GetEmployer(item.UserId, currentUser.CompanyTableId);
                            removeReadersDescription += empl.FullName + "; ";
                            Delete(x => x.DocumentTableId == document.Id && x.UserId == item.UserId && x.RoleId == null);
                        }
                    }
                    else
                    {
                        var empl = _EmplService.GetEmployer(item.UserId, currentUser.CompanyTableId);
                        removeReadersDescription += empl.FullName + "; ";
                    }
                }
            }

            foreach (var item in currentReadersGroup)
            {
                if (item.Key.CreateUserId == currentUserId)
                {
                    ApplicationRole role = RoleManager.FindById(item.Key.RoleId);
                    if (listdata != null)
                    {
                        if (listdata.Contains(item.Key.RoleId) == false && item.Key.CreateUserId == currentUserId)
                        {
                            removeReadersDescription += role.Description + "; ";
                            Delete(x => x.DocumentTableId == document.Id && x.RoleId == item.Key.RoleId);
                        }
                    }
                    else
                    {
                        removeReadersDescription += role.Description + "; ";
                    }
                }
            }

            if (addReadersDescription.Length > 0)
            {
                _HistoryUserService.SaveHistory(document.Id, Models.Repository.HistoryType.AddReader, currentUserId, document.DocumentNum, document.ProcessName, document.CreatedBy, addReadersDescription);
            }
            if (removeReadersDescription.Length > 0)
            {
                _HistoryUserService.SaveHistory(document.Id, Models.Repository.HistoryType.RemoveReader, currentUserId, document.DocumentNum, document.ProcessName, document.CreatedBy, removeReadersDescription);
            }

            return newReader;
        }

        public List<string> SaveOrderReader(DocumentTable document, string[] listdata, string currentUserId)
        {
            List<string> newReader = new List<string>();
            string addReadersDescription = String.Empty;
            ApplicationUser currentUser = repoUser.GetById(currentUserId);
            List<EmplTable> emplList = _EmplService.GetAllIntercompany().ToList();

            if (listdata != null)
            {
                foreach (string userId in listdata)
                {
                    if ((Contains(x => x.DocumentTableId == document.Id && x.UserId == userId && x.RoleId == null) == false) &&
                        (Contains(x => x.DocumentTableId == document.Id && x.RoleId == userId) == false))
                    {
                        var empl = _EmplService.GetEmployer(userId, currentUser.CompanyTableId);
                        if (empl == null)
                        {
                            ApplicationRole role = RoleManager.FindById(userId);

                            var names = role.Users;
                            if (names != null && names.Count() > 0)
                            {
                                DocumentReaderTable reader = new DocumentReaderTable();
                                reader.DocumentTableId = document.Id;
                                reader.UserId = userId;
                                reader.RoleId = userId;
                                SaveDomain(reader, currentUserId);
                                newReader.Add(userId);

                                addReadersDescription += role.Description + "; ";
                            }
                        }
                        else
                        {
                            if (empl.Enable == true)
                            {
                                addReadersDescription += empl.FullName + "; ";
                                DocumentReaderTable reader = new DocumentReaderTable();
                                reader.DocumentTableId = document.Id;
                                reader.UserId = userId;
                                SaveDomain(reader, currentUserId);
                                newReader.Add(userId);
                            }
                        }
                    }
                }
            }

            if (addReadersDescription.Length > 0)
            {
                _HistoryUserService.SaveHistory(document.Id, Models.Repository.HistoryType.AddReader, currentUser.Id, document.DocumentNum, document.ProcessName, document.CreatedBy, addReadersDescription);
            }

            return newReader;
        }
        public List<string> AddReader(DocumentTable document, List<IdentityUserRole> listdata)
        {
            List<string> newReader = new List<string>();
            string addReadersDescription = String.Empty;
            string removeReadersDescription = String.Empty;
            ApplicationUser currentUser = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());

            if (listdata != null)
            {
                foreach (var user in listdata)
                {
                    if (Contains(x => x.DocumentTableId == document.Id && x.UserId == user.UserId) == false)
                    {
                        newReader.Add(user.UserId);
                        var empl = _EmplService.GetEmployer(user.UserId, currentUser.CompanyTableId);
                        addReadersDescription += empl.FullName + "; ";

                        DocumentReaderTable reader = new DocumentReaderTable();
                        reader.DocumentTableId = document.Id;
                        reader.UserId = user.UserId;
                        SaveDomain(reader);
                    }
                }
            }

            if (addReadersDescription.Length > 0)
            {
                _HistoryUserService.SaveHistory(document.Id, Models.Repository.HistoryType.AddReader, currentUser.Id, document.DocumentNum, document.ProcessName, document.CreatedBy, addReadersDescription);
            }

            return newReader;
        }
        public void SaveDomain(DocumentReaderTable domainTable, string currentUserId = "")
        {
            if (currentUserId != string.Empty)
            {
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = currentUserId;
                domainTable.ApplicationUserModifiedId = currentUserId;
            }
            else
            {
                string userId = HttpContext.Current.User.Identity.GetUserId();
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userId;
                domainTable.ApplicationUserModifiedId = userId;
            }
            repo.Add(domainTable);
            _uow.Commit();
        }
        public void Delete(Guid documentId)
        {
            repo.Delete(x => x.DocumentTableId == documentId);
            _uow.Commit();
        }
        public void Delete(Expression<Func<DocumentReaderTable, bool>> predicate)
        {
            repo.Delete(predicate);
            _uow.Commit();
        }
        public DocumentReaderTable Find(Guid id)
        {
            return repo.GetById(id);
        }


        public bool ContainsRoleUser(Guid documentId, string userId)
        {
            var listGroup = GetPartial(x => x.DocumentTableId == documentId && x.RoleId != null).GroupBy(x => x.RoleId).ToList();
            foreach (var item in listGroup)
            {
                ApplicationRole role = RoleManager.FindById(item.Key);

                var names = role.Users;
                if (names != null && names.Count() > 0)
                {
                    if (names.ToList().Exists(x => x.UserId == userId))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}