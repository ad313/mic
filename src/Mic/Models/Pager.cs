namespace Mic.Models
{
    /// <summary>
    /// 分页基类
    /// </summary>
    public class Pager
    {
        /// <summary>页索引，即第几页，从1开始</summary>
        public int Page { get; set; }

        /// <summary>每页显示行数</summary>
        public int PageSize { get; set; }

        /// <summary>排序条件</summary>
        public string Order { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public string Keyword { get; set; }

        public Pager()
        {
            Page = 1;
            PageSize = 10;
        }

        public void Check()
        {
            if (Page <= 0)
                Page = 1;

            if (PageSize <= 0)
                PageSize = 10;
        }
    }
}
