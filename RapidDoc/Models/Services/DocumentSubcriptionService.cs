using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.Repository;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Transactions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data;

namespace RapidDoc.Models.Services
{
    public interface IDocumentSubcriptionService
    {
        IEnumerable<DocumentSubcriptionListTable> GetAll();
        IEnumerable<DocumentSubcriptionListTable> GetPartial(Expression<Func<DocumentSubcriptionListTable, bool>> predicate);
        DocumentSubcriptionListTable FirstOrDefault(Expression<Func<DocumentSubcriptionListTable, bool>> predicate);
        bool Contains(Expression<Func<DocumentSubcriptionListTable, bool>> predicate);
        void SaveSubscriber(Guid documentId, string[] listdata, string currentUserId);
        void SaveSubscriberMapper(Guid documentId, string[] listdata, string currentUserId);
        void AddSubscriber(Guid documentId, List<IdentityUserRole> listdata);
        void SaveDomain(DocumentSubcriptionListTable domainTable, string currentUserId);
        void Delete(Guid documentId);
        void Delete(Expression<Func<DocumentSubcriptionListTable, bool>> predicate);
        DocumentSubcriptionListTable Find(Guid id);
    }

    public class DocumentSubcriptionService : IDocumentSubcriptionService
    {
        private IRepository<DocumentSubcriptionListTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork _uow;
        private readonly IEmplService _EmplService;
        private readonly IHistoryUserService _HistoryUserService;
        protected RoleManager<ApplicationRole> RoleManager { get; private set; }

        public DocumentSubcriptionService(IUnitOfWork uow, IEmplService emplService, IHistoryUserService historyUserService)
        {
            _uow = uow;
            repo = uow.GetRepository<DocumentSubcriptionListTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            RoleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(_uow.GetDbContext<ApplicationDbContext>()));
            _EmplService = emplService;
            _HistoryUserService = historyUserService;
        }

        public IEnumerable<DocumentSubcriptionListTable> GetAll()
        {
            return repo.All();
        }

        public IEnumerable<DocumentSubcriptionListTable> GetPartial(Expression<Func<DocumentSubcriptionListTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }

        public DocumentSubcriptionListTable FirstOrDefault(Expression<Func<DocumentSubcriptionListTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public bool Contains(Expression<Func<DocumentSubcriptionListTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void SaveSubscriber(Guid documentId, string[] listdata, string currentUserId)
        {
            if (listdata != null)
            {
                foreach (string userId in listdata)
                {                      
                    DocumentSubcriptionListTable reader = new DocumentSubcriptionListTable();
                    reader.DocumentTableId = documentId;
                    reader.UserId = userId;
                    SaveDomain(reader, currentUserId);
                }
            }
         }

        public void SaveSubscriberMapper(Guid documentId, string[] listdata, string currentUserId)
        {
            DateTime createdDate = DateTime.UtcNow;
            using (var bcp = new System.Data.SqlClient.SqlBulkCopy(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                bcp.BatchSize = listdata.Count();
                bcp.DestinationTableName = "[dbo].[DocumentSubcriptionListTable]";
                DataTable table = new DataTable();
                table.Columns.Add("Id", typeof(Guid));
                table.Columns.Add("DocumentTableId", typeof(Guid));
                table.Columns.Add("UserId", typeof(string));               
                table.Columns.Add("ApplicationUserCreatedId", typeof(string));
                table.Columns.Add("ApplicationUserModifiedId", typeof(string));
                table.Columns.Add("TimeStamp", typeof(Byte[]));
                table.Columns.Add("CreatedDate", typeof(DateTime));
                table.Columns.Add("ModifiedDate", typeof(DateTime));

                int num = 0;
                foreach (string userId in listdata)
                {
                    num++;
                    DataRow row = table.NewRow();
                    row["Id"] = Guid.NewGuid();
                    row["DocumentTableId"] = documentId;
                    row["UserId"] = userId;                                      
                    row["ApplicationUserCreatedId"] = currentUserId;
                    row["ApplicationUserModifiedId"] = currentUserId;
                    row["TimeStamp"] = DBNull.Value;
                    row["CreatedDate"] = createdDate;
                    row["ModifiedDate"] = createdDate;

                    table.Rows.Add(row);
                }

                bcp.WriteToServer(table);
                _uow.Commit();
            }
        }

        public void AddSubscriber(Guid documentId, List<IdentityUserRole> listdata)
        {
            if (listdata != null)
            {
                foreach (var user in listdata)
                {
                    DocumentSubcriptionListTable reader = new DocumentSubcriptionListTable();
                    reader.DocumentTableId = documentId;
                    reader.UserId = user.UserId;
                    SaveDomain(reader, HttpContext.Current.User.Identity.GetUserId());
                }
            }
        }

        public void SaveDomain(DocumentSubcriptionListTable domainTable, string currentUserId)
        {
            domainTable.CreatedDate = DateTime.UtcNow;
            domainTable.ModifiedDate = domainTable.CreatedDate;
            domainTable.ApplicationUserCreatedId = currentUserId;
            domainTable.ApplicationUserModifiedId = currentUserId;

            repo.Add(domainTable);
            _uow.Commit();
        }

        public void Delete(Guid documentId)
        {
            repo.Delete(x => x.DocumentTableId == documentId);
            _uow.Commit();
        }

        public void Delete(Expression<Func<DocumentSubcriptionListTable, bool>> predicate)
        {
            repo.Delete(predicate);
            _uow.Commit();
        }

        public DocumentSubcriptionListTable Find(Guid id)
        {
            return repo.GetById(id);
        }
    }
}