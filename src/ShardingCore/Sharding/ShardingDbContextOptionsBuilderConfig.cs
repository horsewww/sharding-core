﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/19 20:57:52
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingDbContextOptionsBuilderConfig<TShardingDbContext> : IShardingDbContextOptionsBuilderConfig where TShardingDbContext : DbContext, IShardingDbContext
    {
        public ShardingDbContextOptionsBuilderConfig(Action<DbConnection, DbContextOptionsBuilder> shardingDbContextOptionsCreator)
        {
            ShardingDbContextOptionsCreator = shardingDbContextOptionsCreator;
        }
        public Action<DbConnection, DbContextOptionsBuilder> ShardingDbContextOptionsCreator { get; }
        public Type ShardingDbContextType => typeof(TShardingDbContext);

        public DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection, DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            ShardingDbContextOptionsCreator(dbConnection, dbContextOptionsBuilder);
            dbContextOptionsBuilder.UseInnerDbContextSharding<TShardingDbContext>();
            return dbContextOptionsBuilder;
        }
    }
}
