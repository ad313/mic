using System;
using System.IO;
using RazorEngineCore;

namespace Mic.RazorEngineCoreTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var code = File.ReadAllText("RegisterAopClass.cshtml");

            //IRazorEngine razorEngine = new RazorEngine();
            //IRazorEngineCompiledTemplate template = razorEngine.Compile(code);

            //string result = template.Run(new
            //{
            //    AopCodeBuilders = builders,
            //    AopMetaData = mateData,
            //});








            Console.ReadLine();
        }
    }
}
