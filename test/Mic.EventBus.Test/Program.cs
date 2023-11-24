using Mic.EventBus.RabbitMQ.Core.Abstractions;

namespace Mic.EventBus.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            //builder.Services.ConfigureHttpJsonOptions(options =>
            //{
            //    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            //});

            //builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddScoped<IService1, Service1>();

            builder.Services.AddEventBusUseRabbitMq(builder.Configuration.GetSection("RabbitMQConfig").Get<RabbitMQ.RabbitMqConfig>());

            var app = builder.Build();



            app.Lifetime.ApplicationStarted.Register(async () =>
            {
                var bus = app.Services.GetService<IEventBusProvider>();
                //await bus.PublishAsync($"test_test1", new EventMessageModel<int>(1));
                //await bus.PublishAsync($"test_test2", new EventMessageModel<int>(2), true);

                //bus.Subscribe<int>("test_test1", async data =>
                //{
                //    var service1 = data.ScopeServiceProvider.GetRequiredService<IService1>();
                //    var d = await service1.Test1(1);
                //});

                //var data = await bus.RpcClientAsync<string>("Service1/TestRpcServer2", new object[] { 1, "str", 123.22, DateTime.Now, new EventMessageModel<int>(1) }, 300);

                await Task.Delay(10000);
            });

            var sampleTodos = new Todo[] {
                new(1, "Walk the dog"),
                new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
                new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
                new(4, "Clean the bathroom"),
                new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
            };

            var todosApi = app.MapGroup("/todos");
            todosApi.MapGet("/", async (IEventBusProvider bus) =>
            {
                await bus.PublishAsync($"test_test1", new EventMessageModel<int>(1));

                //var data = await bus.RpcClientAsync<string>("Service1/TestRpcServer", new object[] { 1 }, 300);
                //var data = await bus.RpcClientAsync<string>("Service1/TestRpcServer2", new object[] { 1, "str", 123.22, DateTime.Now , new EventMessageModel<int>(1) }, 300);


                return sampleTodos;
            });
            todosApi.MapGet("/{id}", (int id) =>
                sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                    ? Results.Ok(todo)
                    : Results.NotFound());

            app.Run();
        }
    }

    public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

    //[JsonSerializable(typeof(Todo[]))]
    //internal partial class AppJsonSerializerContext : JsonSerializerContext
    //{

    //}
}