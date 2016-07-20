using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class RenameCompanyGrid : Grid<RenameCompanyView>
    {
        public RenameCompanyGrid(IEnumerable<RenameCompanyView> items)
            : base(items)
        {
        }
    }

    public class RenameCompanyAjaxPagingGrid : RenameCompanyGrid
    {
        public RenameCompanyAjaxPagingGrid(IEnumerable<RenameCompanyView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page }; //override  default pager
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}