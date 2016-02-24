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
using Microsoft.AspNet.Identity;

namespace RapidDoc.Models.Services
{
    public interface IPortalParametersService
    {
        IEnumerable<PortalParametersTable> GetAll();
        IEnumerable<PortalParametersView> GetAllView();
        IEnumerable<PortalParametersTable> GetPartial(Expression<Func<PortalParametersTable, bool>> predicate);
        IEnumerable<PortalParametersView> GetPartialView(Expression<Func<PortalParametersTable, bool>> predicate);
        IEnumerable<PortalParametersTable> GetPartialIntercompany(Expression<Func<PortalParametersTable, bool>> predicate);
        IEnumerable<PortalParametersView> GetPartialIntercompanyView(Expression<Func<PortalParametersTable, bool>> predicate);
        PortalParametersTable FirstOrDefault(Expression<Func<PortalParametersTable, bool>> predicate);
        PortalParametersView FirstOrDefaultView(Expression<Func<PortalParametersTable, bool>> prediacate);
        bool Contains(Expression<Func<PortalParametersTable, bool>> predicate);
        void Save(PortalParametersView viewTable);
        void SaveDomain(PortalParametersTable domainTable);
        PortalParametersTable Find(Guid id);
        PortalParametersView FindView(Guid id);
    }

    public class PortalParametersService : IPortalParametersService
    {
        private IRepository<PortalParametersTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IRepository<DepartmentTable> repoDepartment;
        private IUnitOfWork uow;

        public PortalParametersService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<PortalParametersTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            repoDepartment = uow.GetRepository<DepartmentTable>();
        }

        public IEnumerable<PortalParametersTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<PortalParametersView> GetAllView()
        {
            return Mapper.Map<IEnumerable<PortalParametersTable>, IEnumerable<PortalParametersView>>(GetAll());
        }

        public IEnumerable<PortalParametersTable> GetPartial(Expression<Func<PortalParametersTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<PortalParametersView> GetPartialView(Expression<Func<PortalParametersTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<PortalParametersTable>, IEnumerable<PortalParametersView>>(GetPartial(predicate));
        }

        public IEnumerable<PortalParametersTable> GetPartialIntercompany(Expression<Func<PortalParametersTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<PortalParametersView> GetPartialIntercompanyView(Expression<Func<PortalParametersTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<PortalParametersTable>, IEnumerable<PortalParametersView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public PortalParametersTable FirstOrDefault(Expression<Func<PortalParametersTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public PortalParametersView FirstOrDefaultView(Expression<Func<PortalParametersTable, bool>> prediacate)
        {
            return Mapper.Map<PortalParametersTable, PortalParametersView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<PortalParametersTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(PortalParametersView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new PortalParametersTable();
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

        public void SaveDomain(PortalParametersTable domainTable)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser user = repoUser.GetById(userId);
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.Id = Guid.NewGuid();
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userId;
                domainTable.ApplicationUserModifiedId = userId;
                domainTable.CompanyTableId = user.CompanyTableId;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = userId;
                repo.Update(domainTable);
            }
            uow.Commit();
        }

        public PortalParametersTable Find(Guid id)
        {
            return repo.Find(a => a.Id == id);
        }

        public PortalParametersView FindView(Guid id)
        {
            return Mapper.Map<PortalParametersTable, PortalParametersView>(Find(id));
        }      
    }
}