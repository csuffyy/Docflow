﻿using AutoMapper;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.Infrastructure;
using RapidDoc.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using RapidDoc.Models.Repository;
using System.Web.Mvc;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace RapidDoc.Models.Services
{
    public interface ISearchService
    {
        IEnumerable<SearchTable> GetPartial(Expression<Func<SearchTable, bool>> predicate);
        IEnumerable<SearchView> GetPartialView(Expression<Func<SearchTable, bool>> predicate);
        SearchTable FirstOrDefault(Expression<Func<SearchTable, bool>> predicate);
        SearchView FirstOrDefaultView(Expression<Func<SearchTable, bool>> predicate);
        bool Contains(Expression<Func<SearchTable, bool>> predicate);
        void SaveDomain(SearchTable domainTable, string currentUserId = "");
        Tuple<int, List<SearchView>> GetDocuments(int blockNumber, int blockSize, SearchFormView model);
        void SaveSearchData(Guid id, string searchString, string currentUserId = "");
        void SaveSearchData(Guid id, dynamic docModel, string actionModelName, string currentUserId = "");
        string PrepareSearchString(dynamic docModel, string actionModelName);
        void Delete(Guid Id);
    }

    public class SearchService : ISearchService
    {
        private IRepository<SearchTable> repo;
        private IRepository<ApplicationUser> repoUser;
        private IRepository<EmplTable> repoEmpl;
        private IUnitOfWork _uow;
        private readonly IDocumentService _DocumentService;
        private readonly ISystemService _SystemService;

        public SearchService(IUnitOfWork uow, IDocumentService documentService,  ISystemService systemService)
        {
            _uow = uow;
            repo = uow.GetRepository<SearchTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            repoEmpl = uow.GetRepository<EmplTable>();
            _DocumentService = documentService;
            _SystemService = systemService;
        }
        public IEnumerable<SearchTable> GetPartial(Expression<Func<SearchTable, bool>> predicate)
        {
            return repo.FindAll(predicate);
        }
        public IEnumerable<SearchView> GetPartialView(Expression<Func<SearchTable, bool>> predicate)
        {
            var items = Mapper.Map<IEnumerable<SearchTable>, IEnumerable<SearchView>>(GetPartial(predicate));
            ApplicationUser currentUser = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());

            foreach (var item in items)
            {
                DocumentTable docuTable = _DocumentService.Find(item.DocumentTableId);
                item.isShow = _DocumentService.isShowDocument(docuTable, currentUser, true);

                ApplicationUser user = repoUser.GetById(item.ApplicationUserCreatedId);
                EmplTable empl = repoEmpl.Find(x => x.ApplicationUserId == user.Id && x.CompanyTableId == user.CompanyTableId);
                if (empl != null)
                    item.CreatedUserName = "(" + empl.AliasCompanyName + ") " + empl.FullName + " " + empl.TitleName + " " + empl.DepartmentName + " "+ _SystemService.ConvertDateTimeToLocal(currentUser, item.CreatedDate);
                else
                    item.CreatedUserName = String.Empty;
            }

            return items;
        }
        public SearchTable FirstOrDefault(Expression<Func<SearchTable, bool>> predicate)
        {
            return repo.Find(predicate);
        }
        public SearchView FirstOrDefaultView(Expression<Func<SearchTable, bool>> predicate)
        {
            var item = Mapper.Map<SearchTable, SearchView>(FirstOrDefault(predicate));
            DocumentTable docuTable = _DocumentService.Find(item.DocumentTableId);
            ApplicationUser currentUser = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());
            item.isShow = _DocumentService.isShowDocument(docuTable, currentUser, true);

            return item;
        }
        public bool Contains(Expression<Func<SearchTable, bool>> predicate)
        {
            return repo.Contains(predicate);
        }
        public void SaveDomain(SearchTable domainTable, string currentUserId = "")
        {
            string userid = getCurrentUserId(currentUserId);
            if (domainTable.Id == Guid.Empty)
            {
                domainTable.CreatedDate = DateTime.UtcNow;
                domainTable.ModifiedDate = domainTable.CreatedDate;
                domainTable.ApplicationUserCreatedId = userid;
                domainTable.ApplicationUserModifiedId = userid;
                repo.Add(domainTable);
            }
            else
            {
                domainTable.ModifiedDate = DateTime.UtcNow;
                domainTable.ApplicationUserModifiedId = userid;
                repo.Update(domainTable);
            }
            _uow.Commit();
        }
        public Tuple<int, List<SearchView>> GetDocuments(int blockNumber, int blockSize, SearchFormView model)
        {
            List<SearchView> result = new List<SearchView>();
            int startIndex = (blockNumber - 1) * blockSize;
            string createdUserId = null;
            string searchString = null;

            if (!String.IsNullOrEmpty(model.SearchText))
            {
                searchString = model.SearchText.Trim();
            }

            if (model.CreatedEmplTableId != null)
            {
                EmplTable emplTable = repoEmpl.GetById(model.CreatedEmplTableId ?? Guid.Empty);
                if (emplTable != null)
                {
                    createdUserId = emplTable.ApplicationUserId;
                }
            }

            ApplicationDbContext contextQuery = _uow.GetDbContext<ApplicationDbContext>();

            var prepareResult = from search in contextQuery.SearchTable
                where (search.CreatedDate >= model.StartDate || model.StartDate == null) &&
                    (search.CreatedDate <= model.EndDate || model.EndDate == null) &&
                    (search.DocumentTable.CompanyTableId == model.CompanyTableId || model.CompanyTableId == null) &&
                    (search.DocumentTable.ProcessTableId == model.ProcessTableId || model.ProcessTableId == null) &&
                    (search.DocumentTable.ApplicationUserCreatedId == createdUserId || createdUserId == null) &&
                    (search.DocumentText.Contains(searchString) || (search.DocumentTable.DocumentNum.Contains(searchString)) || searchString == null || searchString == String.Empty)
                orderby search.CreatedDate descending
                select new SearchView
                {
                    ApplicationUserCreatedId = search.ApplicationUserCreatedId,
                    ApplicationUserModifiedId = search.ApplicationUserModifiedId,
                    CreatedDate = search.CreatedDate,
                    DocumentNum = search.DocumentTable.DocumentNum,
                    DocumentTableId = search.DocumentTableId,
                    DocumentText = search.DocumentText,
                    ProcessName = search.DocumentTable.ProcessTable.ProcessName,
                    ModifiedDate = search.ModifiedDate,
                    Id = search.Id
                };
            int count = prepareResult.Count();
            result = prepareResult.Skip(startIndex).Take(blockSize).ToList();
            List<ApplicationUser> users = repoUser.All().ToList();
            List<EmplTable> empls = repoEmpl.All().ToList();
            ApplicationUser currentUser = users.FirstOrDefault(x => x.Id == HttpContext.Current.User.Identity.GetUserId());

            foreach (var item in result)
            {
                DocumentTable docuTable = _DocumentService.Find(item.DocumentTableId);
                item.isShow = _DocumentService.isShowDocument(docuTable, currentUser, true);

                ApplicationUser user = users.FirstOrDefault(x => x.Id == item.ApplicationUserCreatedId);
                EmplTable empl = empls.FirstOrDefault(x => x.ApplicationUserId == user.Id && x.CompanyTableId == user.CompanyTableId);
                if (empl != null)
                    item.CreatedUserName = "(" + empl.AliasCompanyName + ") " + empl.FullName + " " + empl.TitleName + " " + empl.DepartmentName + " " + _SystemService.ConvertDateTimeToLocal(currentUser, item.CreatedDate);
                else
                    item.CreatedUserName = String.Empty;
            }

            return new Tuple<int, List<SearchView>>(count, result);
        }

        public string PrepareSearchString(dynamic docModel, string actionModelName)
        {
            Type type = Type.GetType("RapidDoc.Models.ViewModels." + actionModelName + "_View");
            var properties = type.GetProperties();
            string allStringData = String.Empty;
            string regex = @"(<.+?>|&nbsp;)";
            string regexGuid = @"([a-z0-9]{8}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{4}[-][a-z0-9]{12})";

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType.IsEnum)
                {
                    string enumValue = property.GetValue(docModel, null).ToString();
                    MemberInfo member = property.PropertyType.GetMember(enumValue)[0];

                    var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
                    string outString = String.Empty;
                    if (attrs.Any())
                    {
                        var displayAttr = ((DisplayAttribute)attrs[0]);

                        outString = displayAttr.Name;

                        if (displayAttr.ResourceType != null)
                        {
                            outString = displayAttr.GetName();
                        }
                    }
                    else
                    {
                        outString = enumValue;
                    }
                    if (allStringData.IndexOf(outString) < 0)
                        allStringData = allStringData + outString + "|";
                }
                else if (property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(docModel, null);

                    if (!String.IsNullOrEmpty(value) && !String.IsNullOrWhiteSpace(value))
                    {
                        string stringWithoutTags = Regex.Replace(value, regex, "").Trim();

                        if (!String.IsNullOrEmpty(stringWithoutTags))
                        {
                            List<string> guidList = new List<string>();
                            guidList = Regex.Matches(stringWithoutTags, regexGuid)
                                .Cast<Match>()
                                .Select(m => m.Groups[0].Value)
                                .ToList();

                            foreach (string guid in guidList)
                            {
                                stringWithoutTags = stringWithoutTags.Replace(guid + ",", "");
                                stringWithoutTags = stringWithoutTags.Replace(guid, "");
                            }

                            stringWithoutTags = StripTagsCharArray(stringWithoutTags);
                            stringWithoutTags = stringWithoutTags.Replace("\r", "").Replace("\n", "").Trim();
                            if (!String.IsNullOrEmpty(stringWithoutTags))
                            {
                                if (allStringData.IndexOf(stringWithoutTags) < 0)
                                    allStringData = allStringData + stringWithoutTags + "|";
                            }
                        }
                    }
                }
            }

            return allStringData;
        }

        public string StripTagsCharArray(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;
            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true; continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public void SaveSearchData(Guid id, string searchString, string currentUserId = "")
        {
            if (String.IsNullOrEmpty(searchString))
                return;

            DocumentTable document = _DocumentService.Find(id);

            if (document == null)
                return;

            document.DocumentText = searchString;
            _DocumentService.SaveDocumentText(document);

            if (!Contains(x => x.DocumentTableId == document.Id))
                SaveDomain(new SearchTable { DocumentText = searchString, DocumentTableId = document.Id }, currentUserId);
            else
            {
                SearchTable searchTable = FirstOrDefault(x => x.DocumentTableId == document.Id);
                searchTable.DocumentText = searchString;
                SaveDomain(searchTable, currentUserId);
            }
        }

        public void SaveSearchData(Guid id, dynamic docModel, string actionModelName, string currentUserId = "")
        {
            string searchString = PrepareSearchString(docModel, actionModelName);
            if (!String.IsNullOrEmpty(searchString))
                SaveSearchData(id, searchString, currentUserId);
        }
        public void Delete(Guid Id)
        {
            repo.Delete(x => x.Id == Id);
            _uow.Commit();
        }

        private string getCurrentUserId(string currentUserId = "")
        {
            if (currentUserId != string.Empty)
            {
                return currentUserId;
            }
            else
            {
                return HttpContext.Current.User.Identity.GetUserId();
            }
        }
    }
}