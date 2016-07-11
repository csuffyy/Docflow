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
    public interface IProjectService
    {
        IEnumerable<ProjectTable> GetAll();
        IEnumerable<ProjectView> GetAllView();
        IEnumerable<ProjectTable> GetPartial(Expression<Func<ProjectTable, bool>> predicate);
        IEnumerable<ProjectView> GetPartialView(Expression<Func<ProjectTable, bool>> predicate);
        IEnumerable<ProjectTable> GetPartialIntercompany(Expression<Func<ProjectTable, bool>> predicate);
        IEnumerable<ProjectView> GetPartialIntercompanyView(Expression<Func<ProjectTable, bool>> predicate);
        ProjectTable FirstOrDefault(Expression<Func<ProjectTable, bool>> predicate);
        ProjectView FirstOrDefaultView(Expression<Func<ProjectTable, bool>> prediacate);
        bool Contains(Expression<Func<ProjectTable, bool>> predicate);
        void Save(ProjectView viewTable);
        void SaveDomain(ProjectTable domainTable);
        void Delete(Guid id);
        ProjectTable Find(Guid id);
        ProjectView FindView(Guid id);
        SelectList GetDropListProject(Guid? id);
        SelectList GetDropListProjectNull(Guid? id);
    }

    public class ProjectService : IProjectService
    {
        private IRepository<ProjectTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork uow;

        public ProjectService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<ProjectTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }

        public IEnumerable<ProjectTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<ProjectView> GetAllView()
        {
            return Mapper.Map<IEnumerable<ProjectTable>, IEnumerable<ProjectView>>(GetAll());
        }

        public IEnumerable<ProjectTable> GetPartial(Expression<Func<ProjectTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<ProjectView> GetPartialView(Expression<Func<ProjectTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<ProjectTable>, IEnumerable<ProjectView>>(GetPartial(predicate));
        }

        public IEnumerable<ProjectTable> GetPartialIntercompany(Expression<Func<ProjectTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }

        public IEnumerable<ProjectView> GetPartialIntercompanyView(Expression<Func<ProjectTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<ProjectTable>, IEnumerable<ProjectView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public ProjectTable FirstOrDefault(Expression<Func<ProjectTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public ProjectView FirstOrDefaultView(Expression<Func<ProjectTable, bool>> prediacate)
        {
            return Mapper.Map<ProjectTable, ProjectView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<ProjectTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(ProjectView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new ProjectTable();
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

        public void SaveDomain(ProjectTable domainTable)
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

        public ProjectTable Find(Guid id)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }

        public ProjectView FindView(Guid id)
        {
            return Mapper.Map<ProjectTable, ProjectView>(Find(id));
        }

        public SelectList GetDropListProject(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "ProjectName", id);
        }

        public SelectList GetDropListProjectNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new ProjectView { ProjectName = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "ProjectName", id);
        }
    }
}