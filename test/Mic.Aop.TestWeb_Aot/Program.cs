using System.Text.Json.Serialization;

namespace Mic.Aop.TestWeb_Aot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });

            builder.Services.AddTransient<LogAttribute>();

            var app = builder.Build();

            //var sampleTodos = new DataService_g(app.Services.GetService<IServiceProvider>()).GetData();
            var sampleTodos = new ToDo2[] {
                new(1, "Walk the dog"),
                new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
                new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
                new(4, "Clean the bathroom"),
                new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
            };

            var todosApi = app.MapGroup("/todos");
            todosApi.MapGet("/", () => sampleTodos);
            todosApi.MapGet("/{id}", (int id) =>
                sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                    ? Results.Ok(todo)
                    : Results.NotFound());

            app.Run();
        }
    }

    public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

    [JsonSerializable(typeof(ToDo2[]))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {

    }

    public partial class ToDo2
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public DateOnly? DueBy { get; set; }

        public bool IsComplete { get; set; }

        public ToDo2(int id, string? title, DateOnly? dueBy = null, bool isComplete = false)
        {
            Id = id;
            Title = title;
            DueBy = dueBy;
            IsComplete = isComplete;
        }
    }

    public class DataService
    {
        [Log(AopTag = true)]
        public virtual ToDo2[] GetData()
        {
            return new ToDo2[] {
                new(1, "Walk the dog"),
                new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
                new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
                new(4, "Clean the bathroom"),
                new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
            };
        }
    }

    ///// <summary>
    ///// 继承 DataService 实现方法拦截
    ///// </summary>
    //public sealed class DataService_g2 : DataService
    //{
    //    private readonly IServiceProvider _serviceProvider0;
    //    public DataService_g2(IServiceProvider serviceProvider0)
    //    {
    //        _serviceProvider0 = serviceProvider0;
    //    }

    //    public override ToDo2[] GetData()
    //    {
    //        var aopContext = new AopContext(_serviceProvider0,
    //            new Dictionary<string, dynamic>() { },
    //            false,
    //            true,
    //            null,
    //            typeof(ToDo2[]));

    //        var aopInterceptor0 = _serviceProvider0.GetRequiredService<LogAttribute>();
    //        aopInterceptor0.AopTag = true;

    //        if (aopInterceptor0.HasBefore) aopContext = aopInterceptor0.Before(aopContext);
    //        if (aopInterceptor0.HasAopNext)
    //        {
    //            if (aopInterceptor0.HasActualNext)
    //            {
    //                aopContext.ActualMethod = () => base.GetData();
    //            }

    //            aopContext = aopInterceptor0.Next(aopContext);
    //        }
    //        else
    //        {
    //            if (aopInterceptor0.HasActualNext)
    //            {
    //                aopContext.ReturnValue = base.GetData();
    //            }
    //        }

    //        if (aopInterceptor0.HasAfter) aopContext = aopInterceptor0.After(aopContext);
    //        return aopContext.ReturnValue;
    //    }
    //}
}
