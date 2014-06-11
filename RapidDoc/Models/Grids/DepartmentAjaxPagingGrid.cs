﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class DepartmentGrid : Grid<DepartmentView>
    {
        public DepartmentGrid(IEnumerable<DepartmentView> items)
            : base(items)
        {
        }
    }

    public class DepartmentAjaxPagingGrid : DepartmentGrid
    {
        public DepartmentAjaxPagingGrid(IEnumerable<DepartmentView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page }; //override  default pager
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}