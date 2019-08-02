﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using Dapper;
using Kogel.Dapper.Extension.Extension;
using Kogel.Dapper.Extension.Model;
using Kogel.Dapper.Extension.Attributes;
using Kogel.Dapper.Extension.Core.Interfaces;

namespace Kogel.Dapper.Extension
{
    public abstract class SqlProvider
    {
        public AbstractDataBaseContext Context { get; set; }

        protected SqlProvider()
        {
            Params = new DynamicParameters();
            JoinList = new List<JoinAssTable>();
        }

        public abstract IProviderOption ProviderOption { get; set; }

        public string SqlString { get; set; }
        /// <summary>
        /// 是否在查询分页时查询总数
        /// </summary>
        public bool IsSelectCount = false;
        /// <summary>
        /// 连接对象集合
        /// </summary>
        public List<JoinAssTable> JoinList { get; set; }

        public DynamicParameters Params { get; set; }

        public abstract SqlProvider FormatGet<T>();

        public abstract SqlProvider FormatToList<T>();

        public abstract SqlProvider FormatToPageList<T>(int pageIndex, int pageSize,bool IsSelectCount=true);

        public abstract SqlProvider FormatCount();

        public abstract SqlProvider FormatDelete();

        public abstract SqlProvider FormatInsert<T>(T entity);

        public abstract SqlProvider FormatUpdate<T>(Expression<Func<T, T>> updateExpression);

        public abstract SqlProvider FormatUpdate<T>(T entity);

        public abstract SqlProvider FormatSum<TResult>(Expression<Func<TResult, object>> sumExpression);

        public abstract SqlProvider FormatUpdateSelect<T>(Expression<Func<T, T>> updator);

        protected string FormatTableName(bool isNeedFrom = true)
        {
            var typeOfTableClass = Context.Set.TableType;

            var tableName = EntityCache.QueryEntity(typeOfTableClass).Name;

            SqlString = $" {ProviderOption.OpenQuote}{tableName}{ProviderOption.CloseQuote} ";
            if (isNeedFrom)
                SqlString = " FROM " + SqlString;

            return SqlString;
        }

        protected string[] FormatInsertParamsAndValues<T>(T entity)
        {
            var paramSqlBuilder = new StringBuilder(64);
            var valueSqlBuilder = new StringBuilder(64);

            var properties = EntityCache.QueryEntity(entity.GetType()).Properties;

            var isAppend = false;
            foreach (var propertiy in properties)
            {
                //判断不是主键
                if (propertiy.CustomAttributes.FirstOrDefault(x => x.AttributeType.Equals(typeof(Identity))) == null)
                {
                    if (isAppend)
                    {
                        paramSqlBuilder.Append(",");
                        valueSqlBuilder.Append(",");
                    }
                    var name = propertiy.GetColumnAttributeName();
                    paramSqlBuilder.AppendFormat("{0}{1}{2}", ProviderOption.OpenQuote, name, ProviderOption.CloseQuote);
                    valueSqlBuilder.Append(ProviderOption.ParameterPrefix + name);
                    Params.Add(ProviderOption.ParameterPrefix + name, propertiy.GetValue(entity));
                    isAppend = true;
                }
            }

            return new[] { paramSqlBuilder.ToString(), valueSqlBuilder.ToString() };
        }

        protected DataBaseContext<T> DataBaseContext<T>()
        {
            return (DataBaseContext<T>)Context;
        }
    }
}