using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class NumberSeriesBookingGrid : Grid<NumberSeriesBookingView>
    {
        public NumberSeriesBookingGrid(IEnumerable<NumberSeriesBookingView> items)
            : base(items)
        {
        }
    }

    public class NumberSeriesBookingAjaxPagingGrid : NumberSeriesBookingGrid
    {
        public NumberSeriesBookingAjaxPagingGrid(IEnumerable<NumberSeriesBookingView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page }; //override  default pager
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}