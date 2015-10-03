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
    public interface IQuestionRequestService
    {
        IEnumerable<QuestionRequestTable> GetAll();
        IEnumerable<QuestionRequestView> GetAllView();
        IEnumerable<QuestionRequestTable> GetPartial(Expression<Func<QuestionRequestTable, bool>> predicate);
        IEnumerable<QuestionRequestView> GetPartialView(Expression<Func<QuestionRequestTable, bool>> predicate);
        IEnumerable<QuestionRequestTable> GetPartialIntercompany(Expression<Func<QuestionRequestTable, bool>> predicate);
        IEnumerable<QuestionRequestView> GetPartialIntercompanyView(Expression<Func<QuestionRequestTable, bool>> predicate);
        QuestionRequestTable FirstOrDefault(Expression<Func<QuestionRequestTable, bool>> predicate);
        QuestionRequestView FirstOrDefaultView(Expression<Func<QuestionRequestTable, bool>> prediacate);
        bool Contains(Expression<Func<QuestionRequestTable, bool>> predicate);
        void Save(QuestionRequestView viewTable);
        void SaveDomain(QuestionRequestTable domainTable);
        void Delete(Guid id);
        QuestionRequestTable Find(Guid id);
        QuestionRequestView FindView(Guid id);
        SelectList GetDropListQuestionRequest(Guid? id);
        SelectList GetDropListQuestionRequestNull(Guid? id);
    }

    public class QuestionRequestService : IQuestionRequestService
    {
        private IRepository<QuestionRequestTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork uow;

        public QuestionRequestService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<QuestionRequestTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }

        public IEnumerable<QuestionRequestTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<QuestionRequestView> GetAllView()
        {
            return Mapper.Map<IEnumerable<QuestionRequestTable>, IEnumerable<QuestionRequestView>>(GetAll());
        }

        public IEnumerable<QuestionRequestTable> GetPartial(Expression<Func<QuestionRequestTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<QuestionRequestView> GetPartialView(Expression<Func<QuestionRequestTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<QuestionRequestTable>, IEnumerable<QuestionRequestView>>(GetPartial(predicate));
        }

        public IEnumerable<QuestionRequestTable> GetPartialIntercompany(Expression<Func<QuestionRequestTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<QuestionRequestView> GetPartialIntercompanyView(Expression<Func<QuestionRequestTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<QuestionRequestTable>, IEnumerable<QuestionRequestView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public QuestionRequestTable FirstOrDefault(Expression<Func<QuestionRequestTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public QuestionRequestView FirstOrDefaultView(Expression<Func<QuestionRequestTable, bool>> prediacate)
        {
            return Mapper.Map<QuestionRequestTable, QuestionRequestView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<QuestionRequestTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(QuestionRequestView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new QuestionRequestTable();
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

        public void SaveDomain(QuestionRequestTable domainTable)
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

        public QuestionRequestTable Find(Guid id)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }

        public QuestionRequestView FindView(Guid id)
        {
            return Mapper.Map<QuestionRequestTable, QuestionRequestView>(Find(id));
        }

        public SelectList GetDropListQuestionRequest(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "Name", id);
        }

        public SelectList GetDropListQuestionRequestNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new QuestionRequestView { Name = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "Name", id);
        }
    }
}