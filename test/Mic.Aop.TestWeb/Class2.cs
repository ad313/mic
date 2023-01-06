using System;

namespace Mic.Aop.TestWeb
{
    [BizDictionary]
    public class Class2
    {
        [BizDictionary]
        public string Name { get; set; }
    }

    public class BizDictionaryAttribute : Attribute
    {

    }
}
