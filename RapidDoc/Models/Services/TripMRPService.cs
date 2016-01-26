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
    public interface ITripMRPService
    {
        IEnumerable<TripMRPTable> GetAll();
        IEnumerable<TripMRPView> GetAllView();
        IEnumerable<TripMRPTable> GetPartial(Expression<Func<TripMRPTable, bool>> predicate);
        IEnumerable<TripMRPView> GetPartialView(Expression<Func<TripMRPTable, bool>> predicate);
        IEnumerable<TripMRPTable> GetPartialIntercompany(Expression<Func<TripMRPTable, bool>> predicate);
        IEnumerable<TripMRPView> GetPartialIntercompanyView(Expression<Func<TripMRPTable, bool>> predicate);
        TripMRPTable FirstOrDefault(Expression<Func<TripMRPTable, bool>> predicate);
        TripMRPView FirstOrDefaultView(Expression<Func<TripMRPTable, bool>> prediacate);
        bool Contains(Expression<Func<TripMRPTable, bool>> predicate);
        void Save(TripMRPView viewTable);
        void SaveDomain(TripMRPTable domainTable);
        void Delete(Guid id);
        TripMRPTable Find(Guid id);
        TripMRPView FindView(Guid id);
    }

    public class TripMRPService : ITripMRPService
    {
        private IRepository<TripMRPTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork uow;

        public TripMRPService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<TripMRPTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }

        public IEnumerable<TripMRPTable> GetAll()
        {
            return repo.All();
        }

        public IEnumerable<TripMRPView> GetAllView()
        {
            return Mapper.Map<IEnumerable<TripMRPTable>, IEnumerable<TripMRPView>>(GetAll());
        }

        public IEnumerable<TripMRPTable> GetPartial(Expression<Func<TripMRPTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }

        public IEnumerable<TripMRPView> GetPartialView(Expression<Func<TripMRPTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<TripMRPTable>, IEnumerable<TripMRPView>>(GetPartial(predicate));
        }

        public IEnumerable<TripMRPTable> GetPartialIntercompany(Expression<Func<TripMRPTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<TripMRPView> GetPartialIntercompanyView(Expression<Func<TripMRPTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<TripMRPTable>, IEnumerable<TripMRPView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public TripMRPTable FirstOrDefault(Expression<Func<TripMRPTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public TripMRPView FirstOrDefaultView(Expression<Func<TripMRPTable, bool>> prediacate)
        {
            return Mapper.Map<TripMRPTable, TripMRPView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<TripMRPTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(TripMRPView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new TripMRPTable();
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

        public void SaveDomain(TripMRPTable domainTable)
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

        public TripMRPTable Find(Guid id)
        {
            return repo.Find(a => a.Id == id);
        }

        public TripMRPView FindView(Guid id)
        {
            return Mapper.Map<TripMRPTable, TripMRPView>(Find(id));
        }
    }
}