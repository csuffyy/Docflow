using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class OrganizationGrid : Grid<OrganizationView>
    {
        public OrganizationGrid(IEnumerable<OrganizationView> items)
            : base(items)
        {
        }
    }

    public class OrganizationAjaxPagingGrid : OrganizationGrid
    {
        public OrganizationAjaxPagingGrid(IEnumerable<OrganizationView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page };
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}