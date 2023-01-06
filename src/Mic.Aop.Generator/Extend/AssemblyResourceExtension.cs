﻿using System.IO;
using System.Reflection;

namespace Mic.Aop.Generator.Extend
{
    public static class AssemblyResourceExtension
    {
        public static Stream GetResourceStream(this Assembly assembly, string name)
        {
            return assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{name}");
        }

        public static string GetResourceString(this Assembly assembly, string name)
        {
            var stream = GetResourceStream(assembly, name);
            return stream.GetString();
        }

        public static string GetString(this Stream stream)
        {
            if (stream == null) return string.Empty;
            using (stream)
            {
                if (stream.Length <= 0) return string.Empty;
                if (stream.Position != 0) stream.Position = 0;

                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                // 设置当前流的位置为流的开始 
                stream.Seek(0, SeekOrigin.Begin);
                stream.Dispose();
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }
    }
}