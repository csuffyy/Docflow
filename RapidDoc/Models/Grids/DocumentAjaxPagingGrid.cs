﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;
using System.Web.Mvc;
using RapidDoc.Models.Services;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace RapidDoc.Models.Grids
{
    public class DocumentGrid : Grid<DocumentView>
    {
        private IEnumerable<DocumentView> _displayingItems;

        private readonly IReviewDocLogService _ReviewDocLogService;
        private readonly IDocumentService _DocumentService;
        private readonly IAccountService _AccountService;
        private readonly ISearchService _SearchService;
        private readonly IEmplService _EmplService;

        public DocumentGrid(IQueryable<DocumentView> items, IReviewDocLogService reviewDocLogService, IDocumentService documentService, IAccountService accountService, ISearchService searchService, IEmplService emplService)
            : base(items)
        {
            _ReviewDocLogService = reviewDocLogService;
            _DocumentService = documentService;
            _AccountService = accountService;
            _SearchService = searchService;
            _EmplService = emplService;
        }

        protected override IEnumerable<DocumentView> GetItemsToDisplay()
        {
            if (_displayingItems != null)
                return _displayingItems;

            _displayingItems = base.GetItemsToDisplay().ToList();
            ApplicationUser user = _AccountService.Find(HttpContext.Current.User.Identity.GetUserId());

            foreach (var displayedItem in _displayingItems)
            {
                displayedItem.isNotReview = _ReviewDocLogService.isNotReviewDocCurrentUser(displayedItem.Id ?? Guid.Empty, "", user);
                displayedItem.SLAStatus = _DocumentService.SLAStatus(displayedItem.Id ?? Guid.Empty, "", user);

                EmplTable empl = _EmplService.GetEmployer(displayedItem.ApplicationUserCreatedId, displayedItem.CompanyTableId);
                displayedItem.FullName = empl.FullName;
                displayedItem.TitleName = empl.TitleName;
                displayedItem.DepartmentName = empl.DepartmentName;

                if (!String.IsNullOrEmpty(displayedItem.DocumentText) && displayedItem.DocumentText.Length > 80)
                {
                    displayedItem.DocumentText = displayedItem.DocumentText.Substring(0, 80) + "...";
                }
            }

            return _displayingItems;
        }
    }

    public class DocumentAjaxPagingGrid : DocumentGrid
    {
        public DocumentAjaxPagingGrid(IQueryable<DocumentView> items, int page, bool renderOnlyRows, IReviewDocLogService reviewDocLogService, IDocumentService documentService, IAccountService accountService, ISearchService searchService, IEmplService emplService)
            : base(items, reviewDocLogService, documentService, accountService, searchService, emplService)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page }; //override  default pager
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}