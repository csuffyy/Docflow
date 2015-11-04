using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class ProtocolFoldersGrid : Grid<ProtocolFoldersView>
    {
        public ProtocolFoldersGrid(IEnumerable<ProtocolFoldersView> items)
            : base(items)
        {
        }
    }

    public class ProtocolFoldersAjaxPagingGrid : ProtocolFoldersGrid
    {
        public ProtocolFoldersAjaxPagingGrid(IEnumerable<ProtocolFoldersView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page };
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}