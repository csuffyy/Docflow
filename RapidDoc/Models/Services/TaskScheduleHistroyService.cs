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
    public interface ITaskScheduleHistroyService
    {
        IEnumerable<TaskScheduleHistroyTable> GetAll();
        IEnumerable<TaskScheduleHistroyView> GetAllView();
        IEnumerable<TaskScheduleHistroyTable> GetPartial(Expression<Func<TaskScheduleHistroyTable, bool>> predicate);
        IEnumerable<TaskScheduleHistroyView> GetPartialView(Expression<Func<TaskScheduleHistroyTable, bool>> predicate);
        IEnumerable<TaskScheduleHistroyTable> GetPartialIntercompany(Expression<Func<TaskScheduleHistroyTable, bool>> predicate);
        IEnumerable<TaskScheduleHistroyView> GetPartialIntercompanyView(Expression<Func<TaskScheduleHistroyTable, bool>> predicate);
        TaskScheduleHistroyTable FirstOrDefault(Expression<Func<TaskScheduleHistroyTable, bool>> predicate);
        TaskScheduleHistroyView FirstOrDefaultView(Expression<Func<TaskScheduleHistroyTable, bool>> prediacate);
        bool Contains(Expression<Func<TaskScheduleHistroyTable, bool>> predicate);
        void Save(TaskScheduleHistroyView viewTable);
        void SaveDomain(TaskScheduleHistroyTable domainTable, Guid? companyId, string userId = "");
        void Delete(Guid id);
        TaskScheduleHistroyTable Find(Guid id);
        TaskScheduleHistroyView FindView(Guid id);
        IEnumerable<TaskScheduleHistroyView> GetTaskScheduleHistory(Guid id);
    }

    public class TaskScheduleHistroyService : ITaskScheduleHistroyService
    {
        private IRepository<TaskScheduleHistroyTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork uow;

        public TaskScheduleHistroyService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<TaskScheduleHistroyTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }

        public IEnumerable<TaskScheduleHistroyTable> GetAll()
        {
            return repo.All();
        }

        public IEnumerable<TaskScheduleHistroyView> GetAllView()
        {
            return Mapper.Map<IEnumerable<TaskScheduleHistroyTable>, IEnumerable<TaskScheduleHistroyView>>(GetAll());
        }

        public IEnumerable<TaskScheduleHistroyTable> GetPartial(Expression<Func<TaskScheduleHistroyTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }

        public IEnumerable<TaskScheduleHistroyView> GetPartialView(Expression<Func<TaskScheduleHistroyTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<TaskScheduleHistroyTable>, IEnumerable<TaskScheduleHistroyView>>(GetPartial(predicate));
        }

        public IEnumerable<TaskScheduleHistroyTable> GetPartialIntercompany(Expression<Func<TaskScheduleHistroyTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<TaskScheduleHistroyView> GetPartialIntercompanyView(Expression<Func<TaskScheduleHistroyTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<TaskScheduleHistroyTable>, IEnumerable<TaskScheduleHistroyView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public TaskScheduleHistroyTable FirstOrDefault(Expression<Func<TaskScheduleHistroyTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public TaskScheduleHistroyView FirstOrDefaultView(Expression<Func<TaskScheduleHistroyTable, bool>> prediacate)
        {
            return Mapper.Map<TaskScheduleHistroyTable, TaskScheduleHistroyView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<TaskScheduleHistroyTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(TaskScheduleHistroyView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new TaskScheduleHistroyTable();
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable, viewTable.CompanyTableId);
            }
            else
            {
                var domainTable = Find(viewTable.Id ?? Guid.Empty);
                Mapper.Map(viewTable, domainTable);
                SaveDomain(domainTable, viewTable.CompanyTableId);
            }
        }

        public void SaveDomain(TaskScheduleHistroyTable domainTable, Guid? companyId, string userId = "")
        {
            if (String.IsNullOrEmpty(userId))
                userId = HttpContext.Current.User.Identity.GetUserId();

            if (domainTable.Id == Guid.Empty)
            {
                domainTable.Id = Guid.NewGuid();
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userId;
                domainTable.ApplicationUserModifiedId = userId;
                domainTable.CompanyTableId = companyId;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = userId;
                domainTable.CompanyTableId = companyId;
                repo.Update(domainTable);
            }
            uow.Commit();
        }

        public void Delete(Guid id)
        {
            repo.Delete(a => a.Id == id);
            uow.Commit();
        }

        public TaskScheduleHistroyTable Find(Guid id)
        {
            return repo.Find(a => a.Id == id);
        }

        public TaskScheduleHistroyView FindView(Guid id)
        {
            return Mapper.Map<TaskScheduleHistroyTable, TaskScheduleHistroyView>(Find(id));
        }

        public IEnumerable<TaskScheduleHistroyView> GetTaskScheduleHistory(Guid id)
        {
            return GetPartialView(x => x.TaskScheduleId == id);
        }
    }

}