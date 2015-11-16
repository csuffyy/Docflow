﻿using System;
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
    public interface IProtocolFoldersService
    {
        IEnumerable<ProtocolFoldersTable> GetAll();
        IEnumerable<ProtocolFoldersView> GetAllView();
        IEnumerable<ProtocolFoldersTable> GetPartial(Expression<Func<ProtocolFoldersTable, bool>> predicate);
        IEnumerable<ProtocolFoldersView> GetPartialView(Expression<Func<ProtocolFoldersTable, bool>> predicate);
        IEnumerable<ProtocolFoldersTable> GetPartialIntercompany(Expression<Func<ProtocolFoldersTable, bool>> predicate);
        IEnumerable<ProtocolFoldersView> GetPartialIntercompanyView(Expression<Func<ProtocolFoldersTable, bool>> predicate);
        ProtocolFoldersTable FirstOrDefault(Expression<Func<ProtocolFoldersTable, bool>> predicate);
        ProtocolFoldersView FirstOrDefaultView(Expression<Func<ProtocolFoldersTable, bool>> prediacate);
        bool Contains(Expression<Func<ProtocolFoldersTable, bool>> predicate);
        void Save(ProtocolFoldersView viewTable);
        void SaveDomain(ProtocolFoldersTable domainTable);
        void Delete(Guid id);
        ProtocolFoldersTable Find(Guid id);
        ProtocolFoldersView FindView(Guid id);
        SelectList GetDropListProtocolFolders(Guid? id);
        SelectList GetDropListProtocolFoldersNull(Guid? id);
        SelectList GetDropListProtocolFoldersFullPath(Guid processId, Guid? id);
    }

    public class ProtocolFoldersService: IProtocolFoldersService
    {
        private IRepository<ProtocolFoldersTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IUnitOfWork uow;

        public ProtocolFoldersService(IUnitOfWork _uow)
        {
            uow = _uow;
            repo = uow.GetRepository<ProtocolFoldersTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
        }

        public IEnumerable<ProtocolFoldersTable> GetAll()
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<ProtocolFoldersView> GetAllView()
        {
            return Mapper.Map<IEnumerable<ProtocolFoldersTable>, IEnumerable<ProtocolFoldersView>>(GetAll());
        }

        public IEnumerable<ProtocolFoldersTable> GetPartial(Expression<Func<ProtocolFoldersTable, bool>> predicate)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.FindAll(predicate).Where(x => x.CompanyTableId == user.CompanyTableId);
        }

        public IEnumerable<ProtocolFoldersView> GetPartialView(Expression<Func<ProtocolFoldersTable, bool>> predicate)
        {
            return Mapper.Map<IEnumerable<ProtocolFoldersTable>, IEnumerable<ProtocolFoldersView>>(GetPartial(predicate));
        }

        public IEnumerable<ProtocolFoldersTable> GetPartialIntercompany(Expression<Func<ProtocolFoldersTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<ProtocolFoldersView> GetPartialIntercompanyView(Expression<Func<ProtocolFoldersTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<ProtocolFoldersTable>, IEnumerable<ProtocolFoldersView>>(GetPartialIntercompany(predicate));
            return items;
        }

        public ProtocolFoldersTable FirstOrDefault(Expression<Func<ProtocolFoldersTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }

        public ProtocolFoldersView FirstOrDefaultView(Expression<Func<ProtocolFoldersTable, bool>> prediacate)
        {
            return Mapper.Map<ProtocolFoldersTable, ProtocolFoldersView>(FirstOrDefault(prediacate));
        }

        public bool Contains(Expression<Func<ProtocolFoldersTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }

        public void Save(ProtocolFoldersView viewTable)
        {
            if (viewTable.Id == null)
            {
                var domainTable = new ProtocolFoldersTable();
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

        public void SaveDomain(ProtocolFoldersTable domainTable)
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

        public ProtocolFoldersTable Find(Guid id)
        {
            ApplicationUser user = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            return repo.Find(a => a.Id == id && a.CompanyTableId == user.CompanyTableId);
        }

        public ProtocolFoldersView FindView(Guid id)
        {
            return Mapper.Map<ProtocolFoldersTable, ProtocolFoldersView>(Find(id));
        }

        public SelectList GetDropListProtocolFolders(Guid? id)
        {
            var items = GetAllView().ToList();
            return new SelectList(items, "Id", "ProtocolFolderName", id);
        }

        public SelectList GetDropListProtocolFoldersNull(Guid? id)
        {
            var items = GetAllView().ToList();
            items.Insert(0, new ProtocolFoldersView { ProtocolFolderName = UIElementRes.UIElement.NoValue, Id = null });
            return new SelectList(items, "Id", "ProtocolFolderName", id);
        }


        public SelectList GetDropListProtocolFoldersFullPath(Guid processId, Guid? id)
        {
            List<ProtocolFoldersView> items = this.GetFullPathItems(GetPartialView(x => x.ProcessTableId == processId && x.ProtocolFoldersParentId == null).ToList());
            return new SelectList(items, "Id", "ProtocolFolderName", id);
        }

        public List<ProtocolFoldersView> GetFullPathItems(List<ProtocolFoldersView> listProtoolFolder, string path = "")
        {
            List<ProtocolFoldersView> listDepartmentId = new List<ProtocolFoldersView>();
            List<ProtocolFoldersView> listDepartmentBufId = new List<ProtocolFoldersView>();
            string partPath = "";

            partPath += path;
            foreach (var item in listProtoolFolder)
            {
                
                    listDepartmentId.Add(new ProtocolFoldersView { ProtocolFolderName = partPath + item.ProtocolFolderName, Id = item.Id });
                    listDepartmentBufId = GetFullPathItems(GetAllView().Where(x => x.ProtocolFoldersParentId == item.Id).ToList(),
                        partPath == "" ? item.ProtocolFolderName + "/" : partPath + item.ProtocolFolderName + "/");
                    listDepartmentId.AddRange(listDepartmentBufId);
            }
            return listDepartmentId;

        }
    }
}