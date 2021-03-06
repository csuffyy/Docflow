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
    public interface IDepartmentService
    {
        IEnumerable<DepartmentTable> GetAll();
        IEnumerable<DepartmentView> GetAllView();
        IEnumerable<DepartmentTable> GetPartial(Expression<Func<DepartmentTable, bool>> predicate);
        IEnumerable<DepartmentView> GetPartialView(Expression<Func<DepartmentTable, bool>> predicate);
        IEnumerable<DepartmentTable> GetPartialIntercompany(Expression<Func<DepartmentTable, bool>> predicate);
        IEnumerable<DepartmentView> GetPartialIntercompanyView(Expression<Func<DepartmentTable, bool>> predicate);
        DepartmentTable FirstOrDefault(Expression<Func<DepartmentTable, bool>> predicate);
        DepartmentView FirstOrDefaultView(Expression<Func<DepartmentTable, bool>> predicate);
        bool Contains(Expression<Func<DepartmentTable, bool>> predicate);
        void Save(DepartmentView viewTable);
        void SaveDomain(DepartmentTable domainTable, string currentUserName = "", Guid? companyId = null);
        void Delete(Guid id);
        DepartmentTable Find(Guid id);
        DepartmentView FindView(Guid id);
        SelectList GetDropListDepartmentNull(Guid? id);
        SelectList GetDropListDepartment(Guid? id);

        DepartmentTable getParentDepartment(Guid? id, Guid companyId);
        bool checkParentDepartment(List<string> groupName, string userId);

        object GetJsonDepartmentCompany();
    }

    public class DepartmentService : IDepartmentService
    {
        private IRepository<DepartmentTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork _uow;

        public DepartmentService(IUnitOfWork uow)
        {
            _uow = uow;
            repo = uow.GetRepository<DepartmentTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }
        public IEnumerable<DepartmentTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }
        public IEnumerable<DepartmentView> GetAllView()
        {
            var items = Mapper.Map<IEnumerable<DepartmentTable>, IEnumerable<DepartmentView>>(GetAll());
            return items;
        }
        public IEnumerable<DepartmentTable> GetPartial(Expression<Func<DepartmentTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }
        public IEnumerable<DepartmentView> GetPartialView(Expression<Func<DepartmentTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<DepartmentTable>, IEnumerable<DepartmentView>>(GetPartial(predicate));
            return items;
        }
        public IEnumerable<DepartmentTable> GetPartialIntercompany(Expression<Func<DepartmentTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<DepartmentView> GetPartialIntercompanyView(Expression<Func<DepartmentTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<DepartmentTable>, IEnumerable<DepartmentView>>(GetPartialIntercompany(predicate));
            return items;
        }
        public DepartmentTable FirstOrDefault(Expression<Func<DepartmentTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }
        public DepartmentView FirstOrDefaultView(Expression<Func<DepartmentTable, bool>> predicate)
        {
            return Mapper.Map<DepartmentTable, DepartmentView>(FirstOrDefault(predicate));
        }
        public bool Contains(Expression<Func<DepartmentTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }
        public void Save(DepartmentView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new DepartmentTable();
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
        public void SaveDomain(DepartmentTable domainTable, string currentUserName = "", Guid? companyId = null)
        {
            ApplicationUser user = getCurrentUserName(currentUserName);

            domainTable.DepartmentName = domainTable.DepartmentName.Trim();

            if (domainTable.Id == Guid.Empty)
            {
                domainTable.ApplicationUserCreatedId = user.Id;
                domainTable.ApplicationUserModifiedId = user.Id;
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                if (companyId == null)
                    domainTable.CompanyTableId = user.CompanyTableId;
                else
                    domainTable.CompanyTableId = companyId;
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
        public DepartmentTable Find(Guid id)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }
        public DepartmentView FindView(Guid id)
        {
            return Mapper.Map<DepartmentTable, DepartmentView>(Find(id));
        }
        public SelectList GetDropListDepartmentNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new DepartmentView { DepartmentName = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "DepartmentName", id);
        }
        public SelectList GetDropListDepartment(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "DepartmentName", id);
        }
        private ApplicationUser getCurrentUserName(string currentUserName = "")
        {
            if ((HttpContext.Current == null || HttpContext.Current.User.Identity.Name == String.Empty) && currentUserName != string.Empty)
            {
                return repoUser.Find(x => x.UserName == currentUserName);
            }
            else
            {
                return repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            }
        }

        public DepartmentTable getParentDepartment(Guid? id, Guid companyId)
        {
            DepartmentTable childDepartment = FirstOrDefault(x => x.Id == id && x.CompanyTableId == companyId);
            //if (childDepartment != null && childDepartment.RequiredRoles != null)
                return childDepartment;
            /*else
            {

                return childDepartment != null && childDepartment.ParentDepartmentId != null ? this.getParentDepartment(childDepartment.ParentDepartmentId, companyId) : null;
            }*/
        }


        public bool checkParentDepartment(List<string> groupName, string userId)
        {
            UserManager<ApplicationUser>  userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
           
            return groupName.Select(x => x)
                          .Intersect(userManager.GetRoles(userId))
                          .Any(); ;
        }


        public object GetJsonDepartmentCompany()
        {
            var jsondata = from c in GetAll()
                           select new
                           {
                               value = string.Format("{0},({1})", c.Id, c.DepartmentName),
                               text = string.Format("{0}", c.DepartmentName)
                           };

            return jsondata;
        }
    }
}