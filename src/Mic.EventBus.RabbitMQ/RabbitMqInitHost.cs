﻿using Mic.EventBus.RabbitMQ.Attributes;
using Mic.EventBus.RabbitMQ.Core.Abstractions;
using Mic.EventBus.RabbitMQ.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mic.EventBus.RabbitMQ
{
    public class RabbitMqInitHost : BackgroundService
    {
        private readonly IEventBusProvider _eventBusProvider;
        private readonly ISerializerProvider _serializerProvider;
        private readonly ILogger<RabbitMqInitHost> _logger;

        public RabbitMqInitHost(IEventBusProvider eventBusProvider, ISerializerProvider serializerProvider,ILogger<RabbitMqInitHost> logger)
        {
            _eventBusProvider = eventBusProvider;
            _serializerProvider = serializerProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var method in DependencyRegister.RpcServerMethodList)
            {
                var tag = method.GetCustomAttribute<RpcServerAttribute>();
                _eventBusProvider.RpcServer<JsonElement[]>(tag.GetFormatKey(), async data =>
                {
                    try
                    {
                        using var scope = _eventBusProvider.ServiceProvider.CreateScope();
                        var provider = scope.ServiceProvider;
                        var instance = GetInstance(provider, method.DeclaringType);
                        if (instance == null)
                            throw new Exception($"faild to get {method.DeclaringType?.FullName} instance");

                        var pars = new List<dynamic>();
                        var methodPars = method.GetParameters();
                        for (var i = 0; i < methodPars.Length; i++)
                        {
                            pars.Add(_serializerProvider.Deserialize(data?.Length >= i + 1 ? data[i].GetRawText() : null, methodPars[i].ParameterType));
                        }

                        var result = method.Invoke(instance, pars.ToArray());

                        var isAsync = method.ReturnType.Name == "Task" || method.ReturnType.BaseType?.Name == "Task";
                        if (isAsync)
                        {
                            var task = result as Task ?? throw new ArgumentException(nameof(result));
                            await task.ConfigureAwait(false);
                            result = task.GetType().GetProperty("Result")?.GetValue(task, null);
                        }

                        return new RpcResult(result is string
                            ? result.ToString()
                            : _serializerProvider.Serialize(result));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"RpcServer Server error：{e.Message}", e);
                        return new RpcResult(null, e);
                    }
                    finally
                    {
                        await Task.CompletedTask;
                    }
                });
            }

            foreach (var method in DependencyRegister.SubscriberMethodList)
            {
                var tag = method.GetCustomAttribute<SubscriberAttribute>();

                _eventBusProvider.Subscribe<object>(tag.GetFormatKey(), async data =>
                {
                    using var scope = _eventBusProvider.ServiceProvider.CreateScope();
                    var provider = scope.ServiceProvider;
                    var instance = GetInstance(provider, method.DeclaringType);
                    if (instance == null)
                        throw new Exception($"faild to get {method.DeclaringType?.FullName} instance");

                    var pars = new List<dynamic>();
                    var methodPar = method.GetParameters().FirstOrDefault();
                    if (methodPar != null && data.Data != null)
                    {
                        if (methodPar.ParameterType == typeof(string))
                        {
                            pars.Add(data.Data.ToString());
                        }
                        else
                        {
                            pars.Add(_serializerProvider.Deserialize(data.Data.ToString(), methodPar.ParameterType));
                        }
                    }

                    var result = method.Invoke(instance, pars.ToArray());

                    var isAsync = method.ReturnType.Name == "Task" || method.ReturnType.BaseType?.Name == "Task";
                    if (isAsync)
                    {
                        var task = result as Task ?? throw new ArgumentException(nameof(result));
                        await task.ConfigureAwait(false);
                    }
                });
            }

            await Task.CompletedTask;
        }

        private object GetInstance(IServiceProvider serviceProvider, Type type)
        {
            return serviceProvider.GetService(type) ?? ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type);
        }
    }
}