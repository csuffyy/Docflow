using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class TaskScheduleGrid : Grid<TaskScheduleView>
    {
        public TaskScheduleGrid(IEnumerable<TaskScheduleView> items)
            : base(items)
        {
        }
    }

    public class TaskScheduleAjaxPagingGrid : TaskScheduleGrid
    {
        public TaskScheduleAjaxPagingGrid(IEnumerable<TaskScheduleView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page };
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}