using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;
using ShardingCore.SqlServer.EFCores;
using ShardingCore.TableCreator;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Query.Sql;
#endif

namespace ShardingCore.SqlServer
{
    /*
    * @Author: xjm
    * @Description: 
    * @Date: 2020年4月7日 9:30:18
    * @Email: 326308290@qq.com
    */
    public static class DIExtension
    {
        public static IServiceCollection AddShardingSqlServer(this IServiceCollection services, Action<SqlServerOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException($"AddScfSqlServerProvider 参数不能为空:{nameof(configure)}");
           
            var options = new SqlServerOptions();
            configure(options);
            services.AddSingleton(options);
            services.AddShardingCore();

            services.AddScoped<IDbContextOptionsProvider, SqlServerDbContextOptionsProvider>();
            services.AddSingleton<IShardingParallelDbContextFactory, ShardingSqlServerParallelDbContextFactory>();
            if (options.HasSharding)
            {
                foreach (var shardingRoute in options.ShardingRoutes)
                {
                    var genericVirtualRoute = shardingRoute.GetInterfaces().FirstOrDefault(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IVirtualRoute<>)
                                                                                                && it.GetGenericArguments().Any());
                    if (genericVirtualRoute == null)
                        throw new ArgumentException("add sharding route type error not assignable from IVirtualRoute<>.");
                    var shardingEntity=genericVirtualRoute.GetGenericArguments()[0];
                    if(!shardingEntity.IsShardingEntity())
                        throw new ArgumentException("add sharding route type error generic arguments first not assignable from IShardingEntity.");
                    Type genericType = typeof(IVirtualRoute<>);
                    Type interfaceType = genericType.MakeGenericType(shardingEntity);
                    services.AddSingleton(interfaceType, shardingRoute);
                }
            }
            services.AddSingleton(sp =>
            {
                var shardingCoreConfig = new ShardingCoreConfig();
                options.ShardingCoreConfigConfigure?.Invoke(sp,shardingCoreConfig);
                return shardingCoreConfig;
            });
            services.AddSingleton<IShardingBootstrapper,ShardingBootstrapper>();
            return services;
        }

        public static DbContextOptionsBuilder UseShardingSqlServerQuerySqlGenerator(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, ShardingSqlServerQuerySqlGeneratorFactory>();
            return optionsBuilder;
        }
    }
}