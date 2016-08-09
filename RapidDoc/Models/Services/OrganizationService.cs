using System;
using System.Web;
using System.Linq.Expressions;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNet.Identity;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using RapidDoc.Models.Repository;
using RapidDoc.Models.Infrastructure;
using System.Web.Mvc;
using System.Linq;


namespace RapidDoc.Models.Services
{
    public interface IOrganizationService
    {
        IEnumerable<OrganizationTable> GetAll();
        IQueryable<OrganizationView> GetAllView();
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
        SelectList GetDropListOrganization(Guid? id);
        SelectList GetDropListOrganizationNull(Guid? id);
    }

    public class OrganizationService : IOrganizationService
    {
        private IRepository<OrganizationTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork uow;

        public OrganizationService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<OrganizationTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }

        public IEnumerable<OrganizationTable> GetAll()
        {
            return repo.All();
        }

        public IQueryable<OrganizationView> GetAllView()
        {
            return from item in repo.QueryAll().OrderBy(x => x.OrgName)
                   select new OrganizationView {
                        Id = item.Id,
                        OrgName = item.OrgName,
                        Enable = item.Enable
                   };
        }

        public IEnumerable<OrganizationTable> GetPartial(Expression<Func<OrganizationTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
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

            if (domainTable.Id == Guid.Empty)
            {
                domainTable.Id = Guid.NewGuid();
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
            return repo.Find(a => a.Id == id);
        }

        public OrganizationView FindView(Guid id)
        {
            return Mapper.Map<OrganizationTable, OrganizationView>(Find(id));
        }

        public SelectList GetDropListOrganization(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "OrgName", id);
        }

        public SelectList GetDropListOrganizationNull(Guid? id)
        {
            var items = GetAllView().Where(x => x.Enable == true).ToList();
            items.Insert(0, new OrganizationView { OrgName = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "OrgName", id);
        }
    }
}