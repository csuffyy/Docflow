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
    public interface IReasonRequestService
    {
        IEnumerable<ReasonRequestTable> GetAll();
        IEnumerable<ReasonRequestView> GetAllView();
        IEnumerable<ReasonRequestTable> GetPartial(Expression<Func<ReasonRequestTable, bool>> predicate);
        IEnumerable<ReasonRequestView> GetPartialView(Expression<Func<ReasonRequestTable, bool>> predicate);
        IEnumerable<ReasonRequestTable> GetPartialIntercompany(Expression<Func<ReasonRequestTable, bool>> predicate);
        IEnumerable<ReasonRequestView> GetPartialIntercompanyView(Expression<Func<ReasonRequestTable, bool>> predicate);
        ReasonRequestTable FirstOrDefault(Expression<Func<ReasonRequestTable, bool>> predicate);
        ReasonRequestView FirstOrDefaultView(Expression<Func<ReasonRequestTable, bool>> prediacate);
        bool Contains(Expression<Func<ReasonRequestTable, bool>> predicate);
        void Save(ReasonRequestView viewTable);
        void SaveDomain(ReasonRequestTable domainTable);
        void Delete(Guid id);
        ReasonRequestTable Find(Guid id);
        ReasonRequestView FindView(Guid id);
        SelectList GetDropListReasonRequest(Guid? id);
        SelectList GetDropListReasonRequestNull(Guid? id);
    }

    public class ReasonRequestService : IReasonRequestService
    {
        private IRepository<ReasonRequestTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork uow;

        public ReasonRequestService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<ReasonRequestTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }

        public IEnumerable<ReasonRequestTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<ReasonRequestView> GetAllView()
        {
            return Mapper.Map<IEnumerable<ReasonRequestTable>, IEnumerable<ReasonRequestView>>(GetAll());
        }

        public IEnumerable<ReasonRequestTable> GetPartial(Expression<Func<ReasonRequestTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<ReasonRequestView> GetPartialView(Expression<Func<ReasonRequestTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<ReasonRequestTable>, IEnumerable<ReasonRequestView>>(GetPartial(predicate));
        }

        public IEnumerable<ReasonRequestTable> GetPartialIntercompany(Expression<Func<ReasonRequestTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<ReasonRequestView> GetPartialIntercompanyView(Expression<Func<ReasonRequestTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<ReasonRequestTable>, IEnumerable<ReasonRequestView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public ReasonRequestTable FirstOrDefault(Expression<Func<ReasonRequestTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public ReasonRequestView FirstOrDefaultView(Expression<Func<ReasonRequestTable, bool>> prediacate)
        {
            return Mapper.Map<ReasonRequestTable, ReasonRequestView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<ReasonRequestTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(ReasonRequestView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new ReasonRequestTable();
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

        public void SaveDomain(ReasonRequestTable domainTable)
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

        public ReasonRequestTable Find(Guid id)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }

        public ReasonRequestView FindView(Guid id)
        {
            return Mapper.Map<ReasonRequestTable, ReasonRequestView>(Find(id));
        }

        public SelectList GetDropListReasonRequest(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "Name", id);
        }

        public SelectList GetDropListReasonRequestNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new ReasonRequestView { Name = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "Name", id);
        }
    }
}