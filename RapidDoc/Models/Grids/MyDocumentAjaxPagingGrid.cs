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

namespace RapidDoc.Models.Grids
{
    public class MyDocumentGrid : Grid<DocumentView>
    {
        private IEnumerable<DocumentView> _displayingItems;

        private readonly IReviewDocLogService _ReviewDocLogService;
        private readonly IDocumentService _DocumentService;
        private readonly IAccountService _AccountService;
        private readonly ISearchService _SearchService;
        private readonly IEmplService _EmplService;

        public MyDocumentGrid(IQueryable<DocumentView> items, IReviewDocLogService reviewDocLogService, IDocumentService documentService, IAccountService accountService, ISearchService searchService, IEmplService emplService)
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
            EmplTable empl = _EmplService.GetEmployer(user.Id, user.CompanyTableId);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
 
            foreach (var displayedItem in _displayingItems)
            {
                displayedItem.FullName = empl.ShortFullName;
                displayedItem.TitleName = empl.TitleName;
                displayedItem.DepartmentName = empl.DepartmentName;
                displayedItem.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(displayedItem.CreatedDate), timeZoneInfo);

                displayedItem.isNotReview = true;
                displayedItem.SLAStatus = _DocumentService.SLAStatus(displayedItem.Id ?? Guid.Empty, "", user);

                if (!String.IsNullOrEmpty(displayedItem.DocumentText) && displayedItem.DocumentText.Length > 80)
                {
                    displayedItem.DocumentText = displayedItem.DocumentText.Substring(0, 80) + "...";
                }
            }

            return _displayingItems;
        }
    }

    public class MyDocumentAjaxPagingGrid : MyDocumentGrid
    {
        public MyDocumentAjaxPagingGrid(IQueryable<DocumentView> items, int page, bool renderOnlyRows, IReviewDocLogService reviewDocLogService, IDocumentService documentService, IAccountService accountService, ISearchService searchService, IEmplService emplService)
            : base(items, reviewDocLogService, documentService, accountService, searchService, emplService)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page }; //override  default pager
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}