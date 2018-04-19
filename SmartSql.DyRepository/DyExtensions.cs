﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SmartSql.Abstractions;
using SmartSql.DyRepository;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DyExtensions
    {
        public static void AddRepositoryFactory(this IServiceCollection services, string scope_template = "")
        {
            services.AddSingleton<IRepositoryBuilder>((sp) =>
            {
                return new RepositoryBuilder(scope_template);
            });
            services.AddSingleton<IRepositoryFactory>((sp) =>
            {
                var RepositoryBuilder = sp.GetRequiredService<IRepositoryBuilder>();
                return new RepositoryFactory(RepositoryBuilder);
            });
        }
        public static void AddRepository<T>(this IServiceCollection services) where T : class
        {
            services.AddSingleton<T>(sp =>
            {
                var sqlMapper = sp.GetRequiredService<ISmartSqlMapper>();
                if (sqlMapper == null)
                {
                    throw new ArgumentNullException($"can not find ISmartSqlMapper impl");
                }
                var factory = sp.GetRequiredService<IRepositoryFactory>();

                return factory.CreateInstance<T>(sqlMapper);
            });
        }

        public static void AddRepositoryFromAssembly(this IServiceCollection services, Action<AssemblyAutoRegisterOptions> setupOptions)
        {
            var options = new AssemblyAutoRegisterOptions
            {
                Filter = (type) => { return type.IsInterface; }
            };
            setupOptions(options);
            var assembly = Assembly.Load(options.AssemblyString);
            var allTypes = assembly.GetTypes().Where(options.Filter);
            foreach (var type in allTypes)
            {
                services.AddSingleton(type, sp =>
                 {
                     var sqlMapper = sp.GetRequiredService<ISmartSqlMapper>();
                     if (sqlMapper == null)
                     {
                         throw new ArgumentNullException($"can not find ISmartSqlMapper impl");
                     }
                     var factory = sp.GetRequiredService<IRepositoryFactory>();
                     return factory.CreateInstance(type, sqlMapper);
                 });
            }
        }
    }


    public class AssemblyAutoRegisterOptions
    {
        public string AssemblyString { get; set; }
        public Func<Type, bool> Filter { get; set; }

        public void UseTypeFilter<T>()
        {
            Filter = (type) =>
            {
                return typeof(T).IsAssignableFrom(type);
            };
        }
    }
}

