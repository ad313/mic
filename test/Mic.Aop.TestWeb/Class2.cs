using System.ComponentModel;
using SaiLing.Biz.Dictionary.Extensions;
using System.ComponentModel.DataAnnotations;
using SaiLing.WaterPlantAttendance.Services.Common.ClassToProto;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Mic.Aop.TestWeb
{
    [BizDictionary]
    [ClassToProto]
    public partial class Class2
    {
        [Display(Name = "枚举枚举")]
        [BizDictionary(BizType = BizTypeEnum.Enum)]
        public BizTypeEnum? EnumType211 { get; set; }

        /// <summary>
        /// 单个字典，单选
        /// </summary>
        [Display(Name = "单个字典")]
        [BizDictionary(BizType =  BizTypeEnum.Dictionary, Code = "aaaaa")]
        public string Name1111 { get; set; }

        /// <summary>
        /// 单个字典，多选
        /// </summary>
        [Display(Name = "多个字典")]
        [BizDictionary(BizType = BizTypeEnum.Dictionary, IsMultiple = true)]
        public string Name1122 { get; set; }

        /// <summary>
        /// 这是行政区划
        /// </summary>
        [Display(Name = "单个行政区划")]
        [BizDictionary(BizType = BizTypeEnum.Region)]
        public string Region11 { get; set; }

        /// <summary>
        /// 这是行政区划
        /// </summary>
        [Display(Name = "多个行政区划")]
        [BizDictionary(BizType = BizTypeEnum.Region, IsMultiple = true)]
        public string Region112 { get; set; }

        /// <summary>
        /// 这是部门
        /// </summary>
        [Display(Name = "单个部门")]
        [BizDictionary(BizType = BizTypeEnum.Department)]
        public string Department11 { get; set; }

        /// <summary>
        /// 这是部门
        /// </summary>
        [Display(Name = "多个部门")]
        [BizDictionary(BizType = BizTypeEnum.Department, IsMultiple = true)]
        public string Department112 { get; set; }


        /// <summary>
        /// 这是用户
        /// </summary>
        [Display(Name = "单个用户")]
        [BizDictionary(BizType = BizTypeEnum.User)]
        public string User1 { get; set; }

        /// <summary>
        /// 这是用户
        /// </summary>
        [Display(Name = "多个用户")]
        [BizDictionary(BizType = BizTypeEnum.User, IsMultiple = true)]
        public string User2 { get; set; }



        public int Aaa { get; set; }
    }

    [BizDictionary]
    public partial class Class3
    {
        [BizDictionary]
        public string Aaa { get; set; }
    }


    
    public partial class Class4
    {
        /// <summary>
        /// aaa
        /// </summary>
        public string Aaa { get; set; }
    }

    /// <summary>
    /// 考勤统计 查询参数
    /// </summary>
    [ClassToProto]
    public class ClockStatisticsQuery 
    {
        /// <summary>
        /// 112323欧尼
        /// </summary>
        public  int Page { get; set; }

        public  int PageSize { get; set; }

        /// <summary>
        /// 考勤组
        /// </summary>  
        [DisplayName("考勤组")]
        public string? AttendanceGroupId { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>  
        [DisplayName("开始日期")]
        public DateTime? BeginDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>  
        [DisplayName("结束日期")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 用户编号集合
        /// </summary>  
        [DisplayName("用户编号集合")]
        public string? UserIds { get; set; }

        public List<string> UserIdList => string.IsNullOrWhiteSpace(UserIds) ? null : UserIds.Split(',').ToList();
    }
}
