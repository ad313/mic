using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mic.Helpers
{
    /// <summary>
    /// 数字辅助类
    /// </summary>
    public static class NumberHelper
    {
        /// <summary>
        /// 阿拉伯数字转换为中文数字（0-99999）
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string ToChinese(this int num)
        {
            if (num > 99999)
                throw new Exception("只支持 99999 以下的数字");

            var strNum = num.ToString();
            var result = "";
            if (strNum.Length == 5)
            {
                //万
                result += OneBitNumberToChinese(strNum.Substring(0, 1)) + "万";
                strNum = strNum.Substring(1);
            }
            if (strNum.Length == 4)
            {
                //千
                var secondBitNumber = strNum.Substring(0, 1);
                if (secondBitNumber == "0") result += "零";
                else result += OneBitNumberToChinese(secondBitNumber) + "千";
                strNum = strNum.Substring(1);
            }
            if (strNum.Length == 3)
            {
                //百
                var hundredBitNumber = strNum.Substring(0, 1);
                if (hundredBitNumber == "0" && result.Substring(result.Length - 1) != "零") result += "零";
                else result += OneBitNumberToChinese(hundredBitNumber) + "百";
                strNum = strNum.Substring(1);
            }
            if (strNum.Length == 2)
            {
                //十
                var hundredBitNumber = strNum.Substring(0, 1);
                if (hundredBitNumber == "0" && result.Substring(result.Length - 1) != "零") result += "零";
                else if (hundredBitNumber == "1" && string.IsNullOrEmpty(result)) result += "十";//10->十
                else result += OneBitNumberToChinese(hundredBitNumber) + "十";
                strNum = strNum.Substring(1);
            }
            if (strNum.Length == 1)
            {
                //个
                if (strNum == "0") result += "零";
                else result += OneBitNumberToChinese(strNum);
            }
            //100->一百零零
            if (!string.IsNullOrEmpty(result) && result != "零") result = result.TrimEnd('零');
            return result;
        }
        
        private static string OneBitNumberToChinese(string num)
        {
            var numStr = "123456789";
            var chineseStr = "一二三四五六七八九";
            var result = "";
            int numIndex = numStr.IndexOf(num, StringComparison.Ordinal);
            if (numIndex > -1)
            {
                result = chineseStr.Substring(numIndex, 1);
            }
            return result;
        }
    }
}
