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
    public interface ICountryService
    {
        IEnumerable<CountryTable> GetAll();
        IEnumerable<CountryView> GetAllView();
        IEnumerable<CountryTable> GetPartial(Expression<Func<CountryTable, bool>> predicate);
        IEnumerable<CountryView> GetPartialView(Expression<Func<CountryTable, bool>> predicate);
        IEnumerable<CountryTable> GetPartialIntercompany(Expression<Func<CountryTable, bool>> predicate);
        IEnumerable<CountryView> GetPartialIntercompanyView(Expression<Func<CountryTable, bool>> predicate);
        CountryTable FirstOrDefault(Expression<Func<CountryTable, bool>> predicate);
        CountryView FirstOrDefaultView(Expression<Func<CountryTable, bool>> prediacate);
        bool Contains(Expression<Func<CountryTable, bool>> predicate);
        void Save(CountryView viewTable);
        void SaveDomain(CountryTable domainTable);
        void Delete(Guid id);
        CountryTable Find(Guid id);
        CountryView FindView(Guid id);
        SelectList GetDropListCountry(Guid? id);
        SelectList GetDropListCountryNull(Guid? id);
    }

    public class CountryService : ICountryService
    {
        private IRepository<CountryTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IRepository<DepartmentTable> repoDepartment;
        private IUnitOfWork uow;

        public CountryService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<CountryTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            repoDepartment = uow.GetRepository<DepartmentTable>();
        }

        public IEnumerable<CountryTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<CountryView> GetAllView()
        {
            return Mapper.Map<IEnumerable<CountryTable>, IEnumerable<CountryView>>(GetAll());
        }

        public IEnumerable<CountryTable> GetPartial(Expression<Func<CountryTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<CountryView> GetPartialView(Expression<Func<CountryTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<CountryTable>, IEnumerable<CountryView>>(GetPartial(predicate));
        }

        public IEnumerable<CountryTable> GetPartialIntercompany(Expression<Func<CountryTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<CountryView> GetPartialIntercompanyView(Expression<Func<CountryTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<CountryTable>, IEnumerable<CountryView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public CountryTable FirstOrDefault(Expression<Func<CountryTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public CountryView FirstOrDefaultView(Expression<Func<CountryTable, bool>> prediacate)
        {
            return Mapper.Map<CountryTable, CountryView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<CountryTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(CountryView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new CountryTable();
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

        public void SaveDomain(CountryTable domainTable)
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

        public CountryTable Find(Guid id)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }

        public CountryView FindView(Guid id)
        {
            return Mapper.Map<CountryTable, CountryView>(Find(id));
        }

        public SelectList GetDropListCountry(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "CityName", id);
        }

        public SelectList GetDropListCountryNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new CountryView { CityName = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "CityName", id);
        }
        
    }
}