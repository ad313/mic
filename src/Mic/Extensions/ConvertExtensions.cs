using System;
using System.Globalization;

namespace Mic.Extensions
{
    /// <summary>
    /// 数据格式转换扩展
    /// </summary>
    public static partial class ConvertExtensions
    {
        #region private

        #region 常规数据转换

        private delegate bool TryParseDelegate<T>(string s, out T result);

        private static T To<T>(string value, TryParseDelegate<T> parse) => parse(value, out T result) ? result : default(T);

        private static T? ToNull<T>(string value, TryParseDelegate<T> parse) where T : struct => parse(value, out T result) ? (T?)result : null;

        #endregion

        #region 自定义时间格式转换

        private delegate bool TryParseExactDelegate<T>(string s, string format, IFormatProvider provider, DateTimeStyles style, out T result);

        private static T ToExact<T>(string value, string format, IFormatProvider provider, DateTimeStyles style, TryParseExactDelegate<T> parse) => parse(value, format, provider, style, out T result) ? result : default(T);

        private static T? ToExactNull<T>(string value, string format, IFormatProvider provider, DateTimeStyles style, TryParseExactDelegate<T> parse) where T : struct => parse(value, format, provider, style, out T result) ? (T?)result : null;

        #endregion

        #endregion

        #region 数据转换，转换失败时，返回 当前类型的默认值

        public static string GetString(this object value) => value?.ToString();

        public static int GetInt(this object value) => (int)value.GetDecimal();

        public static DateTime GetDateTime(this object value) => To<DateTime>(value.GetString(), DateTime.TryParse);

        public static DateTime GetDateTime(this object value, string format) => ToExact<DateTime>(value.GetString(), format, null, DateTimeStyles.None, DateTime.TryParseExact);

        public static double GetDouble(this object value) => To<double>(value.GetString(), double.TryParse);

        public static decimal GetDecimal(this object value) => To<decimal>(value.GetString(), decimal.TryParse);

        public static Guid GetGuid(this object value) => To<Guid>(value.GetString(), Guid.TryParse);

        #endregion

        #region 数据转换，转换失败时，返回 指定的默认值

        public static int GetInt(this object value, int defaultValue) => value.GetIntOrNull() ?? defaultValue;

        public static DateTime GetDateTime(this object value, DateTime defaultValue) => value.GetDateTimeOrNull() ?? defaultValue;

        public static DateTime GetDateTime(this object value, string format, DateTime defaultValue) => value.GetDateTimeOrNull(format) ?? defaultValue;

        public static double GetDouble(this object value, double defaultValue) => value.GetDoubleOrNull() ?? defaultValue;

        public static decimal GetDecimal(this object value, decimal defaultValue) => value.GetDecimalOrNull() ?? defaultValue;

        public static Guid GetGuid(this object value, Guid defaultValue) => value.GetGuidOrNull() ?? defaultValue;

        #endregion

        #region 数据转换，转换失败时，返回 null

        public static int? GetIntOrNull(this object value) => (int?)value.GetDecimalOrNull();

        public static DateTime? GetDateTimeOrNull(this object value) => ToNull<DateTime>(value.GetString(), DateTime.TryParse);

        public static DateTime? GetDateTimeOrNull(this object value, string format) => ToExactNull<DateTime>(value.GetString(), format, null, DateTimeStyles.None, DateTime.TryParseExact);

        public static double? GetDoubleOrNull(this object value) => ToNull<double>(value.GetString(), double.TryParse);

        public static decimal? GetDecimalOrNull(this object value) => ToNull<decimal>(value.GetString(), decimal.TryParse);

        public static Guid? GetGuidOrNull(this object value) => ToNull<Guid>(value.GetString(), Guid.TryParse);

        #endregion

        #region 时间转字符串

        public static string GetDateTimeString(this DateTime time, string format = "yyyy-MM-dd HH:mm:ss") => time.ToString(format);

        public static string GetDateString(this DateTime time, string format = "yyyy-MM-dd") => time.ToString(format);

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp(this DateTime time)
        {
            return Convert.ToInt64((time.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
        }

        #endregion

        #region 数字保存小数点后 n 位

        public static decimal TrimDecimalPoint(this decimal value, int length = 2) => Math.Round(value, length >= 0 ? length : 0);

        public static double TrimDecimalPoint(this double value, int length = 2) => Math.Round(value, length >= 0 ? length : 0);

        #endregion
    }
}
