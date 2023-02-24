using System;
using System.ComponentModel.DataAnnotations;

namespace Mic.Aop.TestWeb
{
    [BizDictionary]
    public partial class Class2
    {
        /// <summary>
        /// Name啊啊啊啊
        /// </summary>
        [BizDictionary]
        [Display(Name = "Name啊啊啊啊")]
        public string Name { get; set; }

        public int Aaa { get; set; }
    }

    [BizDictionary]
    public partial class Class3
    {
        [BizDictionary]
        public string Aaa { get; set; }
    }

    public class BizDictionaryAttribute : Attribute
    {
        
    }
}
