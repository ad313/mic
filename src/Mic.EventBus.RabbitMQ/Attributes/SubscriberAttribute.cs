﻿using System;

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

        public SubscriberAttribute(string key, string group = null)
        {
            Key = key;
            Group = group;

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public string GetFormatKey() => string.IsNullOrWhiteSpace(Group) ? Key : $"{Group}_{Key}";
    }
}
