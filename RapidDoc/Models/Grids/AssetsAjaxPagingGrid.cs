using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class AssetsGrid : Grid<AssetsView>
    {
        public AssetsGrid(IQueryable<AssetsView> items)
            : base(items)
        {
        }
    }

    public class AssetsAjaxPagingGrid : AssetsGrid
    {
        public AssetsAjaxPagingGrid(IQueryable<AssetsView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page };
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}