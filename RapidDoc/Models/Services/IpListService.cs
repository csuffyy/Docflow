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
    public interface IIpListService
    {
        IEnumerable<IpListTable> GetAll();
        IEnumerable<IpListView> GetAllView();
        IEnumerable<IpListTable> GetPartial(Expression<Func<IpListTable, bool>> predicate);
        IEnumerable<IpListView> GetPartialView(Expression<Func<IpListTable, bool>> predicate);
        IEnumerable<IpListTable> GetPartialIntercompany(Expression<Func<IpListTable, bool>> predicate);
        IEnumerable<IpListView> GetPartialIntercompanyView(Expression<Func<IpListTable, bool>> predicate);
        IpListTable FirstOrDefault(Expression<Func<IpListTable, bool>> predicate);
        IpListView FirstOrDefaultView(Expression<Func<IpListTable, bool>> prediacate);
        bool Contains(Expression<Func<IpListTable, bool>> predicate);
        void Save(IpListView viewTable);
        void SaveDomain(IpListTable domainTable);
        void Delete(Guid id);
        IpListTable Find(Guid id);
        IpListView FindView(Guid id);
    }

    public class IpListService : IIpListService
    {
        private IRepository<IpListTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork uow;

        public IpListService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<IpListTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }

        public IEnumerable<IpListTable> GetAll()
        {
            return repo.All();
        }

        public IEnumerable<IpListView> GetAllView()
        {
            return Mapper.Map<IEnumerable<IpListTable>, IEnumerable<IpListView>>(GetAll());
        }

        public IEnumerable<IpListTable> GetPartial(Expression<Func<IpListTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }

        public IEnumerable<IpListView> GetPartialView(Expression<Func<IpListTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<IpListTable>, IEnumerable<IpListView>>(GetPartial(predicate));
        }

        public IEnumerable<IpListTable> GetPartialIntercompany(Expression<Func<IpListTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<IpListView> GetPartialIntercompanyView(Expression<Func<IpListTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<IpListTable>, IEnumerable<IpListView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public IpListTable FirstOrDefault(Expression<Func<IpListTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public IpListView FirstOrDefaultView(Expression<Func<IpListTable, bool>> prediacate)
        {
            return Mapper.Map<IpListTable, IpListView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<IpListTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(IpListView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new IpListTable();
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

        public void SaveDomain(IpListTable domainTable)
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

        public IpListTable Find(Guid id)
        {
            return repo.Find(a => a.Id == id);
        }

        public IpListView FindView(Guid id)
        {
            return Mapper.Map<IpListTable, IpListView>(Find(id));
        }

    }
}