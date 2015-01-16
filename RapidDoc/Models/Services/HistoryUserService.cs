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

namespace RapidDoc.Models.Services
{
    public interface IHistoryUserService
    {
        IEnumerable<HistoryUserTable> GetAll();
        IEnumerable<HistoryUserView> GetAllView();
        IEnumerable<HistoryUserTable> GetPartial(Expression<Func<HistoryUserTable, bool>> predicate);
        IEnumerable<HistoryUserView> GetPartialView(Expression<Func<HistoryUserTable, bool>> predicate);
        HistoryUserTable FirstOrDefault(Expression<Func<HistoryUserTable, bool>> predicate);
        HistoryUserView FirstOrDefaultView(Expression<Func<HistoryUserTable, bool>> predicate);
        void SaveDomain(HistoryUserTable domainTable, string userId);
        HistoryUserTable Find(Guid id);
        HistoryUserView FindView(Guid id);
    }

    public class HistoryUserService : IHistoryUserService
    {
        private IRepository<HistoryUserTable> repo;
        private IUnitOfWork _uow;
        private readonly IAccountService _AccountService;
        private readonly IEmplService _EmplService;

        public HistoryUserService(IUnitOfWork uow, IAccountService accountService, IEmplService emplService)
        {
            _uow = uow;
            repo = uow.GetRepository<HistoryUserTable>();
            _AccountService = accountService;
            _EmplService = emplService;
        }
        public IEnumerable<HistoryUserTable> GetAll()
        {
            return repo.All();
        }
        public IEnumerable<HistoryUserView> GetAllView()
        {
            var items = Mapper.Map<IEnumerable<HistoryUserTable>, IEnumerable<HistoryUserView>>(GetAll());
            ApplicationUser currentUser = _AccountService.Find(HttpContext.Current.User.Identity.GetUserId());

            foreach(var item in items)
            {
                EmplTable empl = _EmplService.GetEmployer(item.ApplicationUserCreatedId, currentUser.CompanyTableId);
                if (empl != null)
                {
                    item.CreatedEmplName = empl.FullName;
                    item.CreatedEmplTitle = empl.TitleName;
                    item.CreatedEmplDepartment = empl.DepartmentName;
                }
                else
                {
                    item.CreatedEmplName = currentUser.UserName;
                    item.CreatedEmplTitle = String.Empty;
                    item.CreatedEmplDepartment = String.Empty;
                }
            }

            return items;
        }
        public IEnumerable<HistoryUserTable> GetPartial(Expression<Func<HistoryUserTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<HistoryUserView> GetPartialView(Expression<Func<HistoryUserTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<HistoryUserTable>, IEnumerable<HistoryUserView>>(GetPartial(predicate));

            ApplicationUser currentUser = _AccountService.Find(HttpContext.Current.User.Identity.GetUserId());
            foreach (var item in items)
            {
                EmplTable empl = _EmplService.GetEmployer(item.ApplicationUserCreatedId, currentUser.CompanyTableId);
                if (empl != null)
                {
                    item.CreatedEmplName = empl.FullName;
                    item.CreatedEmplTitle = empl.TitleName;
                    item.CreatedEmplDepartment = empl.DepartmentName;
                }
                else
                {
                    item.CreatedEmplName = currentUser.UserName;
                    item.CreatedEmplTitle = String.Empty;
                    item.CreatedEmplDepartment = String.Empty;
                }
            }

            return items;
        }        
        public HistoryUserTable FirstOrDefault(Expression<Func<HistoryUserTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }
        public HistoryUserView FirstOrDefaultView(Expression<Func<HistoryUserTable, bool>> predicate)
        {
            var item = Mapper.Map<HistoryUserTable, HistoryUserView>(FirstOrDefault(predicate));
            ApplicationUser currentUser = _AccountService.Find(HttpContext.Current.User.Identity.GetUserId());
            EmplTable empl = _EmplService.GetEmployer(item.ApplicationUserCreatedId, currentUser.CompanyTableId);
            if (empl != null)
            {
                item.CreatedEmplName = empl.FullName;
                item.CreatedEmplTitle = empl.TitleName;
                item.CreatedEmplDepartment = empl.DepartmentName;
            }
            else
            {
                item.CreatedEmplName = currentUser.UserName;
                item.CreatedEmplTitle = String.Empty;
                item.CreatedEmplDepartment = String.Empty;
            }
            return item;
        }
        public void SaveDomain(HistoryUserTable domainTable, string userId)
        {
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userId;
                domainTable.ApplicationUserModifiedId = userId;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = userId;
                repo.Update(domainTable);
            }
            _uow.Save();
        }
        public void Delete(Guid id)
        {
            repo.Delete(a => a.Id == id);
            _uow.Save();
        }
        public HistoryUserTable Find(Guid id)
        {
            return repo.GetById(id);
        }
        public HistoryUserView FindView(Guid id)
        {
            var item = Mapper.Map<HistoryUserTable, HistoryUserView>(Find(id));
            ApplicationUser currentUser = _AccountService.Find(HttpContext.Current.User.Identity.GetUserId());
            EmplTable empl = _EmplService.GetEmployer(item.ApplicationUserCreatedId, currentUser.CompanyTableId);
            if (empl != null)
            {
                item.CreatedEmplName = empl.FullName;
                item.CreatedEmplTitle = empl.TitleName;
                item.CreatedEmplDepartment = empl.DepartmentName;
            }
            else
            {
                item.CreatedEmplName = currentUser.UserName;
                item.CreatedEmplTitle = String.Empty;
                item.CreatedEmplDepartment = String.Empty;
            }

            return item;
        }
    }
}