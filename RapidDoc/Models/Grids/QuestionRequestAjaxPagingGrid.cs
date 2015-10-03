using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RapidDoc.Models.DomainModels;
using RapidDoc.Models.ViewModels;
using GridMvc;

namespace RapidDoc.Models.Grids
{
    public class QuestionRequestGrid : Grid<QuestionRequestView>
    {
        public QuestionRequestGrid(IEnumerable<QuestionRequestView> items)
            : base(items)
        {
        }
    }

    public class QuestionRequestAjaxPagingGrid : QuestionRequestGrid
    {
        public QuestionRequestAjaxPagingGrid(IEnumerable<QuestionRequestView> items, int page, bool renderOnlyRows)
            : base(items)
        {
            Pager = new AjaxGridPager(this) { CurrentPage = page };
            RenderOptions.RenderRowsOnly = renderOnlyRows;
            EnablePaging = true;
        }
    }
}