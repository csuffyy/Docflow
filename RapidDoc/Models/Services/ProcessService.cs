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
    public interface IProcessService
    {
        IEnumerable<ProcessTable> GetAll();
        IEnumerable<ProcessView> GetAllView();
        IEnumerable<ProcessTable> GetPartial(Expression<Func<ProcessTable, bool>> predicate);
        IEnumerable<ProcessView> GetPartialView(Expression<Func<ProcessTable, bool>> predicate);
        IEnumerable<ProcessTable> GetPartialIntercompany(Expression<Func<ProcessTable, bool>> predicate);
        IEnumerable<ProcessView> GetPartialIntercompanyView(Expression<Func<ProcessTable, bool>> predicate);
        ProcessTable FirstOrDefault(Expression<Func<ProcessTable, bool>> predicate);
        ProcessView FirstOrDefaultView(Expression<Func<ProcessTable, bool>> predicate);
        bool Contains(Expression<Func<ProcessTable, bool>> predicate);
        void Save(ProcessView viewTable);
        void SaveDomain(ProcessTable domainTable);
        void Delete(Guid id);
        ProcessTable Find(Guid id, string currentUserId = "");
        ProcessView FindView(Guid id, string currentUserId = "");
        SelectList GetDropListProcessNull(Guid? id);
        SelectList GetDropListProcess(Guid? id);
        SelectList GetProtocolListProcess();
    }

    public class ProcessService : IProcessService
    {
        private IRepository<ProcessTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork _uow;

        public ProcessService(IUnitOfWork uow)
        {
            _uow = uow;
            repo = uow.GetRepository<ProcessTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }
        public IEnumerable<ProcessTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }
        public IEnumerable<ProcessTable> GetPartial(Expression<Func<ProcessTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }
        public IEnumerable<ProcessView> GetPartialView(Expression<Func<ProcessTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<ProcessTable>, IEnumerable<ProcessView>>(GetPartial(predicate));
            return items;
        }
        public IEnumerable<ProcessTable> GetPartialIntercompany(Expression<Func<ProcessTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<ProcessView> GetPartialIntercompanyView(Expression<Func<ProcessTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<ProcessTable>, IEnumerable<ProcessView>>(GetPartialIntercompany(predicate));
            return items;
        }
        public IEnumerable<ProcessView> GetAllView()
        {
            var items = Mapper.Map<IEnumerable<ProcessTable>, IEnumerable<ProcessView>>(GetAll());
            return items;
        }
        public ProcessTable FirstOrDefault(Expression<Func<ProcessTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }
        public ProcessView FirstOrDefaultView(Expression<Func<ProcessTable, bool>> predicate)
        {
            return Mapper.Map<ProcessTable, ProcessView>(FirstOrDefault(predicate));
        }
        public bool Contains(Expression<Func<ProcessTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }
        public void Save(ProcessView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new ProcessTable();
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
            else
            {
                var domainTable = Find(viewTable.Id ?? Guid.Empty);
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable);
            }
        }
        public void SaveDomain(ProcessTable domainTable)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            if (domainTable.Id == Guid.Empty)
            {
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
            _uow.Commit();
        }
        public void Delete(Guid id)
        {
            repo.Delete(a => a.Id == id);
            _uow.Commit();
        }
        public ProcessTable Find(Guid id, string currentUserId = "")
        {
            ApplicationUser user = getCurrentUserId(currentUserId);
            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }
        public ProcessView FindView(Guid id, string currentUserId = "")
        {
            return Mapper.Map<ProcessTable, ProcessView>(Find(id, currentUserId));
        }
        public SelectList GetDropListProcessNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new ProcessView { ProcessName = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "ProcessName", id);
        }
        public SelectList GetDropListProcess(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "ProcessName", id);
        }

        private ApplicationUser getCurrentUserId(string currentUserId = "")
        {
            if (currentUserId != string.Empty)
            {
                return repoUser.GetById(currentUserId);
            }
            else
            {
                return repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            }
        }


        public SelectList GetProtocolListProcess()
        {
            var items = GetAllView().Where(x => x.DocType == DocumentType.Protocol).ToList();
            if (items.Count() > 0)
                return new SelectList(items, "Id", "ProcessName", items.FirstOrDefault().Id);
            else
                return new SelectList(items, "Id", "ProcessName");
        }
    }
}