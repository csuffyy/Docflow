using System;
using System.Web;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNet.Identity;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using RapidDoc.Models.Repository;
using RapidDoc.Models.Infrastructure;
using System.Web.Mvc;


namespace RapidDoc.Models.Services
{
    public interface IOrganizationService
    {
        IEnumerable<OrganizationTable> GetAll();
        IEnumerable<OrganizationView> GetAllView();
        IEnumerable<OrganizationTable> GetPartial(Expression<Func<OrganizationTable, bool>> predicate);
        IEnumerable<OrganizationView> GetPartialView(Expression<Func<OrganizationTable, bool>> predicate);
        IEnumerable<OrganizationTable> GetPartialIntercompany(Expression<Func<OrganizationTable, bool>> predicate);
        IEnumerable<OrganizationView> GetPartialIntercompanyView(Expression<Func<OrganizationTable, bool>> predicate);
        OrganizationTable FirstOrDefault(Expression<Func<OrganizationTable, bool>> predicate);
        OrganizationView FirstOrDefaultView(Expression<Func<OrganizationTable, bool>> prediacate);
        bool Contains(Expression<Func<OrganizationTable, bool>> predicate);
        void Save(OrganizationView viewTable);
        void SaveDomain(OrganizationTable domainTable);
        void Delete(Guid id);
        OrganizationTable Find(Guid id);
        OrganizationView FindView(Guid id);
    }

    public class OrganizationService : IOrganizationService
    {
        private IRepository<OrganizationTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IRepository<DepartmentTable> repoDepartment;
        private IUnitOfWork uow;

        public OrganizationService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<OrganizationTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            repoDepartment = uow.GetRepository<DepartmentTable>();
        }

        public IEnumerable<OrganizationTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<OrganizationView> GetAllView()
        {
            return Mapper.Map<IEnumerable<OrganizationTable>, IEnumerable<OrganizationView>>(GetAll());
        }

        public IEnumerable<OrganizationTable> GetPartial(Expression<Func<OrganizationTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<OrganizationView> GetPartialView(Expression<Func<OrganizationTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<OrganizationTable>, IEnumerable<OrganizationView>>(GetPartial(predicate));
        }

        public IEnumerable<OrganizationTable> GetPartialIntercompany(Expression<Func<OrganizationTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<OrganizationView> GetPartialIntercompanyView(Expression<Func<OrganizationTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<OrganizationTable>, IEnumerable<OrganizationView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public OrganizationTable FirstOrDefault(Expression<Func<OrganizationTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public OrganizationView FirstOrDefaultView(Expression<Func<OrganizationTable, bool>> prediacate)
        {
            return Mapper.Map<OrganizationTable, OrganizationView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<OrganizationTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(OrganizationView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new OrganizationTable();
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

        public void SaveDomain(OrganizationTable domainTable)
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

        public void Delete(Guid id)
        {
            repo.Delete(a => a.Id == id);
            uow.Commit();
        }

        public OrganizationTable Find(Guid id)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }

        public OrganizationView FindView(Guid id)
        {
            return Mapper.Map<OrganizationTable, OrganizationView>(Find(id));
        }


    }
}