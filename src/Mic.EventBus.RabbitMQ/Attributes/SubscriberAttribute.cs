using System;

namespace Mic.EventBus.RabbitMQ.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SubscriberAttribute : Attribute
    {
        /// <summary>
        /// Subscriber 唯一标识
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Subscriber 分组
        /// </summary>
        public string Group { get; private set; }

        /// <summary>
        /// 广播模式
        /// </summary>
        public bool Broadcast { get; set; }

        public SubscriberAttribute(string key, string group = null, bool broadcast = false)
        {
            Key = key;
            Group = group;
            Broadcast = broadcast;

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public string GetFormatKey() => string.IsNullOrWhiteSpace(Group) ? $"{Key}" : $"{Group}_{Key}";

        //public string GetFormatKey() => string.IsNullOrWhiteSpace(Group) ? $"{Key}_{Broadcast}" : $"{Group}_{Key}_{Broadcast}";
    }
}