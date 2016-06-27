using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class ProjectGrid : Grid<ProjectView>
    {
        public ProjectGrid(IEnumerable<ProjectView> items)
            : base(items)
        {
        }
    }

    public class ProjectAjaxPagingGrid : ProjectGrid
    {
        public ProjectAjaxPagingGrid(IEnumerable<ProjectView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page };
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}