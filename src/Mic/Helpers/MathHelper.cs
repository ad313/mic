using System;
using System.Linq;

namespace Mic.Helpers
{
    /// <summary>
    /// 数学算法辅助类
    /// </summary>
    public class MathHelper
    {
        /// <summary>
        /// 箱型图数据过滤
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static BoxFilterResult BoxFilter(decimal[] data)
        {
            return new BoxFilterResult(data);
        }
    }

    /// <summary>
    /// 详细图过滤数据结果模型
    /// </summary>
    public class BoxFilterResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>
        private decimal[] Source { get; }

        ///// <summary>
        ///// 正常的数据
        ///// </summary>
        //public decimal[] Data { get; }
        ///// <summary>
        ///// 异常的数据
        ///// </summary>
        //public decimal[] Error { get; }

        /// <summary>
        /// 下四分位数位置
        /// </summary>
        public decimal Q1Position { get; }
        /// <summary>
        /// 下四分位数
        /// </summary>
        public decimal Q1 { get; }
        /// <summary>
        /// 下四分位数详情
        /// </summary>
        public string Q1String { get; }

        /// <summary>
        /// 中位数位置
        /// </summary>
        public decimal Q2Position { get; }
        /// <summary>
        /// 中位数
        /// </summary>
        public decimal Q2 { get; }
        /// <summary>
        /// 中位数详情
        /// </summary>
        public string Q2String { get; }


        /// <summary>
        /// 上四分位数位置
        /// </summary>
        public decimal Q3Position { get; }
        /// <summary>
        /// 上四分位数
        /// </summary>
        public decimal Q3 { get; }
        /// <summary>
        /// 上四分位数详情
        /// </summary>
        public string Q3String { get; }

        /// <summary>
        /// 上限
        /// </summary>
        public decimal Max { get; }

        /// <summary>
        /// 下限
        /// </summary>
        public decimal Min { get; }

        /// <summary>
        /// 四分位距
        /// </summary>
        public decimal Iqr { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="source"></param>
        public BoxFilterResult(decimal[] source)
        {
            if (source == null || source.Length <= 0)
                throw new Exception("数据源 Source 不能为空");

            Source = source.OrderBy(d => d).ToArray();

            var n = Source.Length;
            if (n < 4)
            {
                Success = false;
                ErrorMessage = "待检测数据不能小于4条";
                return;
            }

            try
            {
                Q1Position = (n + 1) / 4m;
                Q1 = 0.25m * Source[(int)Q1Position - 1] + 0.75m * Source[(int)Q1Position];
                Q1String = $"0.25*{Source[(int)Q1Position - 1]} + 0.75*{Source[(int)Q1Position]} = {Q1}";


                Q2Position = 2m * (n + 1) / 4;
                //偶数个
                if (Source.Length % 2 == 0)
                {
                    Q2 = 0.5m * Source[(int)Q2Position - 1] + 0.5m * Source[(int)Q2Position];
                    Q2String = $"0.5*{Source[(int)Q2Position - 1]} + 0.5*{Source[(int)Q2Position]} = {Q2}";
                }
                else
                {
                    Q2 = Source[(int)Q2Position - 1];
                    Q2String = $"{Q2}";
                }

                Q3Position = 3m * (n + 1) / 4;
                Q3 = 0.75m * Source[(int)Q3Position - 1] + 0.25m * Source[(int)Q3Position];
                Q3String = $"0.75*{Source[(int)Q3Position - 1]} + 0.25*{Source[(int)Q3Position]} = {Q3}";

                Iqr = Q3 - Q1;
                Max = Q3 + 1.5m * Iqr;
                Min = Q1 - 1.5m * Iqr;

                //Data = Source.Where(d => d >= Min && d <= Max).ToArray();
                //Error = Source.Where(d => d > Max || d < Min).ToArray();

                Success = true;
            }
            catch (Exception e)
            {
                Success = false;
                ErrorMessage = $"计算失败：{e.Message}";
            }
        }
    }
}