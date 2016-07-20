using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using RapidDoc.Models.Repository;
using System.Web.Mvc;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity;

namespace RapidDoc.Models.Services
{
    public interface ICompanyService
    {
        IEnumerable<CompanyTable> GetAll();
        IEnumerable<CompanyView> GetAllView();
        IEnumerable<CompanyTable> GetPartial(Expression<Func<CompanyTable, bool>> predicate);
        IEnumerable<CompanyView> GetPartialView(Expression<Func<CompanyTable, bool>> predicate);
        CompanyTable FirstOrDefault(Expression<Func<CompanyTable, bool>> predicate);
        CompanyView FirstOrDefaultView(Expression<Func<CompanyTable, bool>> predicate);
        void Save(CompanyView viewTable);
        void SaveDomain(CompanyTable domainTable);
        void Delete(Guid id);
        CompanyTable Find(Guid id);
        CompanyView FindView(Guid id);
        SelectList GetDropListCompanyNull(Guid? id);
        SelectList GetDropListCompany(Guid? id);

        IEnumerable<RenameCompanyTable> GetAllRename();
        IEnumerable<RenameCompanyView> GetAllViewRename();
        IEnumerable<RenameCompanyTable> GetPartialRename(Expression<Func<RenameCompanyTable, bool>> predicate);
        IEnumerable<RenameCompanyView> GetPartialViewRename(Expression<Func<RenameCompanyTable, bool>> predicate);
        RenameCompanyTable FirstOrDefaultRename(Expression<Func<RenameCompanyTable, bool>> predicate);
        RenameCompanyView FirstOrDefaultViewRename(Expression<Func<RenameCompanyTable, bool>> predicate);
        void SaveRename(RenameCompanyView viewTable);
        void SaveDomainRename(RenameCompanyTable domainTable);
        void DeleteRename(Guid id);
        RenameCompanyTable FindRename(Guid id);
        RenameCompanyView FindViewRename(Guid id);
        string GetCompanyName(Guid companyId, DateTime? date);
    }

    public class CompanyService : ICompanyService
    {
        private IRepository<CompanyTable> repo;
        private IRepository<RenameCompanyTable> repoRename;
        private IUnitOfWork _uow;

        public CompanyService(IUnitOfWork uow)
        {
            _uow = uow;
            repo = uow.GetRepository<CompanyTable>();
            repoRename = uow.GetRepository<RenameCompanyTable>();
        }
        public IEnumerable<CompanyTable> GetAll()
        {
            return repo.All();
        }
        public IEnumerable<RenameCompanyTable> GetAllRename()
        {
            return repoRename.All();
        }
        public IEnumerable<CompanyView> GetAllView()
        {
            var items = Mapper.Map<IEnumerable<CompanyTable>, IEnumerable<CompanyView>>(GetAll());
            return items;
        }
        public IEnumerable<RenameCompanyView> GetAllViewRename()
        {
            var items = Mapper.Map<IEnumerable<RenameCompanyTable>, IEnumerable<RenameCompanyView>>(GetAllRename());
            return items;
        }
        public IEnumerable<CompanyTable> GetPartial(Expression<Func<CompanyTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<RenameCompanyTable> GetPartialRename(Expression<Func<RenameCompanyTable, bool>> predicate)
        {
            return repoRename.FindAll(predicate);
        }
        public IEnumerable<CompanyView> GetPartialView(Expression<Func<CompanyTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<CompanyTable>, IEnumerable<CompanyView>>(GetPartial(predicate));
            return items;
        }
        public IEnumerable<RenameCompanyView> GetPartialViewRename(Expression<Func<RenameCompanyTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<RenameCompanyTable>, IEnumerable<RenameCompanyView>>(GetPartialRename(predicate));
            return items;
        }
        public CompanyTable FirstOrDefault(Expression<Func<CompanyTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }
        public RenameCompanyTable FirstOrDefaultRename(Expression<Func<RenameCompanyTable, bool>> predicate)
        {
            return repoRename.Find(predicate);
        }
        public CompanyView FirstOrDefaultView(Expression<Func<CompanyTable, bool>> predicate)
        {
            return Mapper.Map<CompanyTable, CompanyView>(FirstOrDefault(predicate));
        }
        public RenameCompanyView FirstOrDefaultViewRename(Expression<Func<RenameCompanyTable, bool>> predicate)
        {
            return Mapper.Map<RenameCompanyTable, RenameCompanyView>(FirstOrDefaultRename(predicate));
        }
        public void Save(CompanyView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new CompanyTable();
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
        public void SaveRename(RenameCompanyView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new RenameCompanyTable();
                Mapper.Map(viewTable, domainTable);
                SaveDomainRename(domainTable);
            }
            else
            {
                var domainTable = FindRename(viewTable.Id ?? Guid.Empty);
                Mapper.Map(viewTable, domainTable);
                SaveDomainRename(domainTable);
            }
        }
        public void SaveDomain(CompanyTable domainTable)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                repo.Update(domainTable);
            }
            _uow.Commit();
        }
        public void SaveDomainRename(RenameCompanyTable domainTable)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                repoRename.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                repoRename.Update(domainTable);
            }
            _uow.Commit();
        }
        public void Delete(Guid id)
        {
            repo.Delete(a => a.Id == id);
            _uow.Commit();
        }
        public void DeleteRename(Guid id)
        {
            repoRename.Delete(a => a.Id == id);
            _uow.Commit();
        }
        public CompanyTable Find(Guid id)
        {
            return repo.GetById(id);
        }
        public RenameCompanyTable FindRename(Guid id)
        {
            return repoRename.GetById(id);
        }
        public CompanyView FindView(Guid id)
        {
            return Mapper.Map<CompanyTable, CompanyView>(Find(id));
        }
        public RenameCompanyView FindViewRename(Guid id)
        {
            return Mapper.Map<RenameCompanyTable, RenameCompanyView>(FindRename(id));
        }
        public SelectList GetDropListCompanyNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new CompanyView { CompanyName = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "CompanyName", id);
        }
        public SelectList GetDropListCompany(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "CompanyName", id);
        }

        public string GetCompanyName(Guid companyId, DateTime? date)
        {
            var model = repoRename.Find(x => x.CompanyTableId == companyId && x.DateFrom <= date && (x.DateTo >= date || x.DateTo == null));
            if (model != null)
                return model.FullCompanyName;

            return String.Empty;
        }
    }
}