using AutoMapper;
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
using Microsoft.AspNet.Identity.EntityFramework;

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
        private readonly IGroupProcessService _GroupProcessService;

        protected UserManager<ApplicationUser> UserManager { get; private set; }

        public SearchService(IUnitOfWork uow, IDocumentService documentService, ISystemService systemService, IGroupProcessService groupProcessService)
        {
            _uow = uow;
            repo = uow.GetRepository<SearchTable>();
            repoUser = uow.GetRepository<ApplicationUser>();
            repoEmpl = uow.GetRepository<EmplTable>();
            _DocumentService = documentService;
            _SystemService = systemService;
            _GroupProcessService = groupProcessService;

            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_uow.GetDbContext<ApplicationDbContext>()));
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
            DateTime currentDate = DateTime.UtcNow;
            ApplicationUser currentUser = repoUser.GetById(HttpContext.Current.User.Identity.GetUserId());

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

            List<Guid> documentAccessList = new List<Guid>();
            if (UserManager.IsInRole(currentUser.Id, "Administrator"))
                documentAccessList.AddRange(from document in contextQuery.DocumentTable select document.Id);
            else
            {
                var delegations = (from delegation in contextQuery.DelegationTable.AsNoTracking()
                                   join emplTo in contextQuery.EmplTable.AsNoTracking() on delegation.EmplTableToId equals emplTo.Id
                                   where delegation.DateFrom <= currentDate && delegation.DateTo >= currentDate && delegation.isArchive == false
                                   && delegation.CompanyTableId == currentUser.CompanyTableId && emplTo.ApplicationUserId == currentUser.Id
                                   select delegation).ToList();

                List<Guid> childGroup = new List<Guid>();

                foreach (var item in delegations.Where(x => x.GroupProcessTableId != null))
                {
                    childGroup.AddRange(_GroupProcessService.GetGroupChildren(item.GroupProcessTableId));
                    childGroup.Add((Guid)item.GroupProcessTableId);
                }

                var childGroupArray = childGroup.Distinct().ToArray();

                documentAccessList.AddRange(from document in contextQuery.DocumentTable.AsNoTracking()
                                            where document.ApplicationUserCreatedId == currentUser.Id
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable.AsNoTracking()
                                            join tracker in contextQuery.WFTrackerTable.AsNoTracking() on document.Id equals tracker.DocumentTableId
                                            where tracker.Users.Any(x => x.UserId == currentUser.Id) || tracker.SignUserId == currentUser.Id
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable.AsNoTracking()
                                            join modification in contextQuery.ModificationUsersTable.AsNoTracking() on document.Id equals modification.DocumentTableId
                                            where modification.UserId == currentUser.Id && document.DocumentState == DocumentState.Created
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable.AsNoTracking()
                                            join process in contextQuery.ProcessTable.AsNoTracking() on document.ProcessTableId equals process.Id
                                            join role in contextQuery.Roles on process.StartReaderRoleId equals role.Id
                                            where process.StartReaderRoleId != null && role.Users.Any(x => x.UserId == currentUser.Id)
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable.AsNoTracking()
                                            join reader in contextQuery.DocumentReaderTable.AsNoTracking() on document.Id equals reader.DocumentTableId
                                            where document.DocumentState != DocumentState.Created && reader.UserId == currentUser.Id
                                            select document.Id);

                documentAccessList.AddRange(from document in contextQuery.DocumentTable.AsNoTracking()
                                            join reader in contextQuery.DocumentReaderTable.AsNoTracking() on document.Id equals reader.DocumentTableId
                                            join role in contextQuery.Roles on reader.RoleId equals role.Id
                                            where document.DocumentState != DocumentState.Created && reader.RoleId != null && role.Users.Any(x => x.UserId == currentUser.Id)
                                            select document.Id);

                if (delegations.Count() > 0)
                {
                    documentAccessList.AddRange(from document in contextQuery.DocumentTable.AsNoTracking()
                                                where (contextQuery.DelegationTable.Any(d => d.EmplTableTo.ApplicationUserId == currentUser.Id && d.DateFrom <= currentDate && d.DateTo >= currentDate && d.isArchive == false
                                                && d.CompanyTableId == currentUser.CompanyTableId
                                                && (d.GroupProcessTableId == null || (d.GroupProcessTableId != null && childGroupArray.Any(x => x == document.ProcessTable.GroupProcessTableId)))
                                                && (d.ProcessTableId == null || d.ProcessTableId == document.ProcessTableId)
                                                && contextQuery.WFTrackerTable.Any(w => w.DocumentTableId == document.Id && w.TrackerType == TrackerType.Waiting && w.Users.Any(b => b.UserId == d.EmplTableFrom.ApplicationUserId))))
                                                select document.Id);
                }

                documentAccessList.Distinct();
            }

            var documentAccessListArray = documentAccessList.ToArray();
            List<SearchView> prepareResult = new List<SearchView>();

            if (UserManager.IsInRole(currentUser.Id, "Administrator"))
            {
                prepareResult.AddRange((from search in contextQuery.SearchTable
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
                                         Id = search.Id,
                                         AliasCompanyName = search.DocumentTable.CompanyTable.AliasCompanyName,
                                         CompanyTableId = search.DocumentTable.CompanyTableId
                                     }).Take(200));
            }
            else
            {
                prepareResult.AddRange((from search in contextQuery.SearchTable
                                     where (search.CreatedDate >= model.StartDate || model.StartDate == null) &&
                                         (search.CreatedDate <= model.EndDate || model.EndDate == null) &&
                                         (search.DocumentTable.CompanyTableId == model.CompanyTableId || model.CompanyTableId == null) &&
                                         (search.DocumentTable.ProcessTableId == model.ProcessTableId || model.ProcessTableId == null) &&
                                         (search.DocumentTable.ApplicationUserCreatedId == createdUserId || createdUserId == null) &&
                                         documentAccessListArray.Contains(search.DocumentTable.Id) &&
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
                                         Id = search.Id,
                                         AliasCompanyName = search.DocumentTable.CompanyTable.AliasCompanyName,
                                         CompanyTableId = search.DocumentTable.CompanyTableId
                                     }).Take(200));
            }

            int count = prepareResult.Count();
            result = prepareResult.Skip(startIndex).Take(blockSize).ToList();
            List<ApplicationUser> users = repoUser.All().ToList();
            List<EmplTable> empls = repoEmpl.All().ToList();

            foreach (var item in result)
            {
                item.isShow = true;
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

            document.DocumentText = GetDocumentText(document, searchString);
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

        private string GetDocumentText(DocumentTable documentTable, string defaultText)
        {
            string ret = defaultText;

            if (documentTable.DocType == DocumentType.OfficeMemo || documentTable.DocType == DocumentType.Protocol || documentTable.DocType == DocumentType.Order
                || documentTable.DocType == DocumentType.AppealDoc || documentTable.DocType == DocumentType.OutcomingDoc || documentTable.DocType == DocumentType.IncomingDoc)
            {
                var refDocument = _DocumentService.GetDocument(documentTable.RefDocumentId, documentTable.ProcessTable.TableName);

                if (refDocument != null)
                {
                    if (documentTable.DocType == DocumentType.OfficeMemo)
                    {
                        ret = refDocument._DocumentTitle;
                    }
                    else if (documentTable.DocType == DocumentType.Protocol)
                    {
                        ret = refDocument.Subject + '\n' + refDocument.Agenda;
                    }
                    if (documentTable.DocType == DocumentType.OutcomingDoc)
                    {
                        ret = refDocument.DocumentSubject;
                    }
                    if (documentTable.DocType == DocumentType.IncomingDoc)
                    {
                        ret = refDocument.DocumentSubject;
                    }
                    if (documentTable.DocType == DocumentType.AppealDoc)
                    {
                        ret = refDocument.Subject;
                    }
                    else if (documentTable.DocType == DocumentType.Order)
                    {
                        if (refDocument.GetType() == (new USR_ORD_BusinessTrip_Table()).GetType())
                            ret = refDocument.GoalTrip;
                        else
                        {
                            if (refDocument.GetType().GetProperty("GoalTrip") != null && String.IsNullOrEmpty(refDocument.Subject))
                                ret = refDocument.GoalTrip;
                            else
                                ret = refDocument.Subject;
                        }                       
                    }
                }
            }

            return ret;
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