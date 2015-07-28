using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class CountryGrid : Grid<CountryView>
    {
        public CountryGrid(IEnumerable<CountryView> items)
            : base(items)
        {
        }
    }

    public class CountryAjaxPagingGrid : CountryGrid
    {
        public CountryAjaxPagingGrid(IEnumerable<CountryView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page };
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}