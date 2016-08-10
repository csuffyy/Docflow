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
    public interface IAssetsService
    {
        IEnumerable<AssetsTable> GetAll();
        IQueryable<AssetsView> GetAllView(string currentUserId = "");
        IEnumerable<AssetsTable> GetPartial(Expression<Func<AssetsTable, bool>> predicate);
        IEnumerable<AssetsView> GetPartialView(Expression<Func<AssetsTable, bool>> predicate);
        IEnumerable<AssetsTable> GetPartialIntercompany(Expression<Func<AssetsTable, bool>> predicate);
        IEnumerable<AssetsView> GetPartialIntercompanyView(Expression<Func<AssetsTable, bool>> predicate);
        AssetsTable FirstOrDefault(Expression<Func<AssetsTable, bool>> predicate);
        AssetsView FirstOrDefaultView(Expression<Func<AssetsTable, bool>> prediacate);
        bool Contains(Expression<Func<AssetsTable, bool>> predicate);
        void Save(AssetsView viewTable);
        void SaveDomain(AssetsTable domainTable);
        void Delete(Guid id);
        AssetsTable Find(Guid id);
        AssetsView FindView(Guid id);
        SelectList GetDropListAssets(Guid? id);
        SelectList GetDropListAssetsNull(Guid? id);
    }

    public class AssetsService : IAssetsService
    {
        private IRepository<AssetsTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork uow;

        public AssetsService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<AssetsTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }

        public IEnumerable<AssetsTable> GetAll()
        {
            return repo.All();
        }

        public IQueryable<AssetsView> GetAllView(string currentUserId = "")
        {
            ApplicationUser user = getCurrentUserId(currentUserId);
            return from item in repo.QueryAll().Where(x => x.CompanyTableId == user.CompanyTableId).OrderBy(x => x.Name)
                   select new AssetsView {
                        Id = item.Id,
                        AssetNumber = item.AssetNumber,
                        Name = item.Name,
                        Location = item.Location,
                        Description = item.Description
                   };
        }

        public IEnumerable<AssetsTable> GetPartial(Expression<Func<AssetsTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }

        public IEnumerable<AssetsView> GetPartialView(Expression<Func<AssetsTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<AssetsTable>, IEnumerable<AssetsView>>(GetPartial(predicate));
        }

        public IEnumerable<AssetsTable> GetPartialIntercompany(Expression<Func<AssetsTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<AssetsView> GetPartialIntercompanyView(Expression<Func<AssetsTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<AssetsTable>, IEnumerable<AssetsView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public AssetsTable FirstOrDefault(Expression<Func<AssetsTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public AssetsView FirstOrDefaultView(Expression<Func<AssetsTable, bool>> prediacate)
        {
            return Mapper.Map<AssetsTable, AssetsView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<AssetsTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(AssetsView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new AssetsTable();
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

        public void SaveDomain(AssetsTable domainTable)
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

        public AssetsTable Find(Guid id)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.Find(a => a.Id == id);
        }

        public AssetsView FindView(Guid id)
        {
            return Mapper.Map<AssetsTable, AssetsView>(Find(id));
        }

        public SelectList GetDropListAssets(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "OrgName", id);
        }

        public SelectList GetDropListAssetsNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new AssetsView { AssetNumber = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "OrgName", id);
        }

        private ApplicationUser getCurrentUserId(string currentUserId = "")
        {
            if (currentUserId != string.Empty)
            {
                return repoUser.GetById(currentUserId);
            }
            else
            {
                return repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            }
        }
    }
}