using SaiLing.Biz.Dictionary.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Mic.Aop.TestWeb
{
    [BizDictionary]
    public partial class Class2
    {
        [Display(Name = "枚举枚举")]
        [BizDictionary(BizType = BizTypeEnum.Enum)]
        public BizTypeEnum? EnumType2 { get; set; }

        /// <summary>
        /// 单个字典，单选
        /// </summary>
        [Display(Name = "单个字典")]
        [BizDictionary(BizType =  BizTypeEnum.Dictionary, Code = "aaaaa")]
        public string Name11 { get; set; }

        /// <summary>
        /// 单个字典，多选
        /// </summary>
        [Display(Name = "多个字典")]
        [BizDictionary(BizType = BizTypeEnum.Dictionary,IsMultiple = true)]
        public string Name22 { get; set; }

        /// <summary>
        /// 这是行政区划
        /// </summary>
        [Display(Name = "多个行政区划")]
        [BizDictionary(BizType = BizTypeEnum.Region, IsMultiple = true)]
        public string Region { get; set; }

        /// <summary>
        /// 这是部门
        /// </summary>
        [Display(Name = "单个部门")]
        [BizDictionary(BizType = BizTypeEnum.Department)]
        public string Department { get; set; }




        public int Aaa { get; set; }
    }

    [BizDictionary]
    public partial class Class3
    {
        [BizDictionary]
        public string Aaa { get; set; }
    }


    //public static class Class2_Ge_Extensions1
    //{
    //    public static List<Class2> BindBizDictionary(this List<Class2> list)
    //    {
    //        if (list == null || !list.Any()) return list;
    //        var codes = new List<string>();
    //        if (list.Exists(d => !string.IsNullOrWhiteSpace(d.Name11))) { codes.Add("aaaaa"); }
    //        if (list.Exists(d => !string.IsNullOrWhiteSpace(d.Name22))) { codes.Add("Name22"); }
    //        if (!codes.Any()) return list;
    //        var service = ServiceHelper.GetService<IBizDictionaryService>();
    //        var dic = AsyncHelper.RunSync(() => service.GetBizDictionary(codes));
    //        if (!dic.Any()) return list;
    //        foreach (var dto in list)
    //        {
    //            if (!string.IsNullOrWhiteSpace(dto.Name11) && dic.TryGetValue(Name11, out List<DictionaryItem> Name11Options))
    //            {
    //                dto.Name11Text = Name11Options.FirstOrDefault(d => d.Value == dto.Name11)?.Text;
    //            }
    //            if (!string.IsNullOrWhiteSpace(dto.Name22) && dic.TryGetValue(Name22, out List<DictionaryItem> Name22Options))
    //            {
    //                dto.Name22Text = Name22Options.FirstOrDefault(d => d.Value == dto.Name22)?.Text;
    //            }



    //        }
    //        return list;
    //    }

    //}
}
