using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class TripMRPGrid : Grid<TripMRPView>
    {
        public TripMRPGrid(IEnumerable<TripMRPView> items)
            : base(items)
        {
        }
    }

    public class TripMRPAjaxPagingGrid : TripMRPGrid
    {
        public TripMRPAjaxPagingGrid(IEnumerable<TripMRPView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page };
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}