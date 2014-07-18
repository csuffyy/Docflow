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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RapidDoc.Models.Services
{
    public interface IDelegationService
    {
        IEnumerable<DelegationTable> GetAll(string currentUserName = "");
        IEnumerable<DelegationView> GetAllView();
        IEnumerable<DelegationTable> GetPartial(Expression<Func<DelegationTable, bool>> predicate, string currentUserName = "");
        IEnumerable<DelegationView> GetPartialView(Expression<Func<DelegationTable, bool>> predicate);
        IEnumerable<DelegationTable> GetPartialIntercompany(Expression<Func<DelegationTable, bool>> predicate);
        IEnumerable<DelegationView> GetPartialIntercompanyView(Expression<Func<DelegationTable, bool>> predicate);
        DelegationTable FirstOrDefault(Expression<Func<DelegationTable, bool>> predicate);
        DelegationView FirstOrDefaultView(Expression<Func<DelegationTable, bool>> predicate);
        void Save(DelegationView viewTable);
        void SaveDomain(DelegationTable domainTable, string currentUserName = "");
        void Delete(Guid id);
        DelegationTable Find(Guid? id, string currentUserName = "");
        DelegationView FindView(Guid id);
    }

    public class DelegationService : IDelegationService
    {
        private IRepository<DelegationTable> repo;
        private IUnitOfWork _uow;
        private readonly IAccountService _AccountService;

        public DelegationService(IUnitOfWork uow, IAccountService accountService)
        {
            _uow = uow;
            repo = uow.GetRepository<DelegationTable>();
            _AccountService = accountService;
        }

        public IEnumerable<DelegationTable> GetAll(string currentUserName = "")
        {
            string localUserName = getCurrentUserName(currentUserName);
            ApplicationUser user = _AccountService.FirstOrDefault(x => x.UserName == localUserName);
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<DelegationView> GetAllView()
        {
            var items = Mapper.Map<IEnumerable<DelegationTable>, IEnumerable<DelegationView>>(GetAll());
            return items;
        }

        public IEnumerable<DelegationTable> GetPartial(Expression<Func<DelegationTable, bool>> predicate, string currentUserName = "")
        {
            string localUserName = getCurrentUserName(currentUserName);
            ApplicationUser user = _AccountService.FirstOrDefault(x => x.UserName == localUserName);
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<DelegationView> GetPartialView(Expression<Func<DelegationTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<DelegationTable>, IEnumerable<DelegationView>>(GetPartial(predicate));
            return items;
        }

        public IEnumerable<DelegationTable> GetPartialIntercompany(Expression<Func<DelegationTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }

        public IEnumerable<DelegationView> GetPartialIntercompanyView(Expression<Func<DelegationTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<DelegationTable>, IEnumerable<DelegationView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public DelegationTable FirstOrDefault(Expression<Func<DelegationTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public DelegationView FirstOrDefaultView(Expression<Func<DelegationTable, bool>> predicate)
        {
            return Mapper.Map<DelegationTable, DelegationView>(FirstOrDefault(predicate));
        }

        public void Save(DelegationView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new DelegationTable();
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
            else
            {
                var domainTable = Find(viewTable.Id);
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
        }

        public void SaveDomain(DelegationTable domainTable, string currentUserName = "")
        {
            string localUserName = getCurrentUserName(currentUserName);
            ApplicationUser user = _AccountService.FirstOrDefault(x => x.UserName == localUserName);

            if (domainTable.Id == Guid.Empty)
            {
                domainTable.Id = Guid.NewGuid();
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.CompanyTableId = user.CompanyTableId;
                domainTable.ApplicationUserCreatedId = user.Id;
                domainTable.ApplicationUserModifiedId = user.Id;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = user.Id;
                repo.Update(domainTable);
            }
            _uow.Save();
        }

        public void Delete(Guid id)
        {
            repo.Delete(a => a.Id == id);
            _uow.Save();
        }

        public DelegationTable Find(Guid? id, string currentUserName = "")
        {
            string localUserName = getCurrentUserName(currentUserName);
            ApplicationUser user = _AccountService.FirstOrDefault(x => x.UserName == localUserName);
            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }

        public DelegationView FindView(Guid id)
        {
            return Mapper.Map<DelegationTable, DelegationView>(Find(id));
        }

        private string getCurrentUserName(string currentUserName = "")
        {
            if ((HttpContext.Current == null || HttpContext.Current.User.Identity.Name == String.Empty) && currentUserName != string.Empty)
            {
                return currentUserName;
            }
            else
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }
    }
}