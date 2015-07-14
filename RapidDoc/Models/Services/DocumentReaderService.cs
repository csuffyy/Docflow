﻿using AutoMapper;
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
        List<string> SaveReader(Guid documentId, string[] listdata);
        List<string> AddReader(Guid documentId, List<IdentityUserRole> listdata);
        List<string> AddOrderReader(Guid documentId, List<string> listdata, string currentUserId);
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
        public List<string> SaveReader(Guid documentId, string[] listdata)
        {
            List<string> newReader = new List<string>();
            string addReadersDescription = String.Empty;
            string removeReadersDescription = String.Empty;
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            List<EmplTable> emplList = _EmplService.GetAll().ToList();

            if (listdata != null)
            {
                foreach (string userId in listdata)
                {
                    if ((Contains(x => x.DocumentTableId == documentId && x.UserId == userId && x.RoleId == null) == false) &&
                        (Contains(x => x.DocumentTableId == documentId && x.RoleId == userId && x.UserId != userId) == false))
                    {                       
                        var empl = _EmplService.GetEmployer(userId, user.CompanyTableId);
                        if (empl == null)
                        {
                            ApplicationRole role = RoleManager.FindById(userId);

                            var names = role.Users;
                            if (names != null && names.Count() > 0)
                            {
                                DocumentReaderTable reader = new DocumentReaderTable();
                                reader.DocumentTableId = documentId;
                                reader.UserId = userId;
                                reader.RoleId = userId;
                                SaveDomain(reader);

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
                        else
                        {
                            addReadersDescription += empl.FullName + "; ";

                            DocumentReaderTable reader = new DocumentReaderTable();
                            reader.DocumentTableId = documentId;
                            reader.UserId = userId;
                            SaveDomain(reader);
                            newReader.Add(userId);
                        }
                    }
                }
            }

            var currentReaders = GetPartial(x => x.DocumentTableId == documentId && x.RoleId == null).ToList();
            var currentReadersGroup = GetPartial(x => x.DocumentTableId == documentId && x.RoleId != null).GroupBy(x => x.RoleId).ToList();
            if(listdata == null)
                Delete(documentId);

            foreach (var item in currentReaders)
            {
                if(listdata != null)
                {
                    if (listdata.Contains(item.UserId) == false)
                    {
                        var empl = _EmplService.GetEmployer(item.UserId, user.CompanyTableId);
                        removeReadersDescription += empl.FullName + "; ";
                        Delete(x => x.DocumentTableId == documentId && x.UserId == item.UserId && x.RoleId == null);
                    }
                }
                else
                {
                    var empl = _EmplService.GetEmployer(item.UserId, user.CompanyTableId);
                    removeReadersDescription += empl.FullName + "; ";
                }
            }

            foreach (var item in currentReadersGroup)
            {
                ApplicationRole role = RoleManager.FindById(item.Key);
                if (listdata != null)
                {
                    if (listdata.Contains(item.Key) == false)
                    {                   
                        removeReadersDescription += role.Description + "; ";
                        Delete(x => x.DocumentTableId == documentId && x.RoleId == item.Key);
                    }
                }
                else
                {
                    removeReadersDescription += role.Description + "; ";
                }
            }

            if (addReadersDescription.Length > 0)
            {
                _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = HistoryType.AddReader, Description = addReadersDescription }, user.Id);
            }
            if (removeReadersDescription.Length > 0)
            {
                _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = HistoryType.RemoveReader, Description = removeReadersDescription }, user.Id);
            }

            return newReader;
        }
        public List<string> AddReader(Guid documentId, List<IdentityUserRole> listdata)
        {
            List<string> newReader = new List<string>();
            string addReadersDescription = String.Empty;
            string removeReadersDescription = String.Empty;
            ApplicationUser currentUser = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());

            if (listdata != null)
            {
                foreach (var user in listdata)
                {
                    if (Contains(x => x.DocumentTableId == documentId && x.UserId == user.UserId) == false)
                    {
                        newReader.Add(user.UserId);
                        var empl = _EmplService.GetEmployer(user.UserId, currentUser.CompanyTableId);
                        addReadersDescription += empl.FullName + "; ";

                        DocumentReaderTable reader = new DocumentReaderTable();
                        reader.DocumentTableId = documentId;
                        reader.UserId = user.UserId;
                        SaveDomain(reader);
                    }
                }
            }

            if (addReadersDescription.Length > 0)
            {
                _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = HistoryType.AddReader, Description = addReadersDescription }, currentUser.Id);
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
            var listGroup =  GetPartial(x => x.DocumentTableId == documentId && x.RoleId != null).GroupBy(x => x.RoleId).ToList();
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


        public List<string> AddOrderReader(Guid documentId, List<string> listdata, string currentUserId)
        {
            List<string> newReader = new List<string>();
            string addReadersDescription = String.Empty;
            string removeReadersDescription = String.Empty;
            ApplicationUser currentUser = repoUser.GetById(currentUserId);

            if (listdata != null)
            {
                foreach (var user in listdata)
                {
                    if (Contains(x => x.DocumentTableId == documentId && x.UserId == user) == false)
                    {
                        newReader.Add(user);
                        var empl = _EmplService.GetEmployer(user, currentUser.CompanyTableId);
                        addReadersDescription += empl.FullName + "; ";

                        DocumentReaderTable reader = new DocumentReaderTable();
                        reader.DocumentTableId = documentId;
                        reader.UserId = user;
                        SaveDomain(reader, currentUserId);
                    }
                }
            }

            if (addReadersDescription.Length > 0)
            {
                _HistoryUserService.SaveDomain(new HistoryUserTable { DocumentTableId = documentId, HistoryType = HistoryType.AddReader, Description = addReadersDescription }, currentUser.Id);
            }

            return newReader;
        }
    }
}