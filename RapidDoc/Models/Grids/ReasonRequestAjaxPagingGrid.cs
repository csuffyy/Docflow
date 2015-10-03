using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class ReasonRequestGrid : Grid<ReasonRequestView>
    {
        public ReasonRequestGrid(IEnumerable<ReasonRequestView> items)
            : base(items)
        {
        }
    }

    public class ReasonRequestAjaxPagingGrid : ReasonRequestGrid
    {
        public ReasonRequestAjaxPagingGrid(IEnumerable<ReasonRequestView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page };
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}