using System.Collections.Generic;

namespace Mic.Models
{
    /// <summary>
    /// 分页列表模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagerList<T>
    {
        /// <summary>页索引，即第几页，从1开始</summary>
        public int Page { get; set; }

        /// <summary>每页显示行数</summary>
        public int PageSize { get; set; }

        /// <summary>总行数</summary>
        public long TotalCount { get; set; }

        /// <summary>总页数</summary>
        public int PageCount => GetPageCount();

        ///// <summary>排序条件</summary>
        //public string Order { get; set; }

        /// <summary>内容</summary>
        public List<T> Data { get; private set; }

        /// <summary>执行语句</summary>
        public string Sql { get; set; }

        public PagerList()
        {
        }

        public PagerList(Pager pager, List<T> data, long totalCount, string sql)
        {
            Data = data;

            if (pager == null)
                return;

            Page = pager.Page;
            PageSize = pager.PageSize;
            TotalCount = totalCount;
            Sql = sql;
        }


        private int GetPageCount()
        {
            if (TotalCount <= 0 || PageSize <= 0)
                return 0;

            return (int)(TotalCount / PageSize) + ((TotalCount % PageSize) <= 0 ? 0 : 1);
        }
    }
}
