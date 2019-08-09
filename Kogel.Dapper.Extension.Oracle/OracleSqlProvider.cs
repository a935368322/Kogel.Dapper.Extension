﻿using System;
using System.Linq.Expressions;
using Kogel.Dapper.Extension.Core.Interfaces;
using Kogel.Dapper.Extension.Oracle.Extension;

namespace Kogel.Dapper.Extension.Oracle
{
    public class OracleSqlProvider : SqlProvider
    {
        private const string OpenQuote = "";
        private const string CloseQuote = "";
        private const char ParameterPrefix = ':';

        public OracleSqlProvider()
        {
            ProviderOption = new ProviderOption(OpenQuote, CloseQuote, ParameterPrefix);
            ResolveExpression.InitOption(ProviderOption);
        }

        public sealed override IProviderOption ProviderOption { get; set; }

        public override SqlProvider FormatGet<T>()
        {
            var selectSql = ResolveExpression.ResolveSelect(EntityCache.QueryEntity(typeof(T)), Context.Set.SelectExpression);

            var fromTableSql = FormatTableName();

            var joinSql = ResolveExpression.ResolveJoinSql(JoinList, ref selectSql, Context.Set.SelectExpression);

            var whereSql = string.Empty;

            //表查询条件
            var whereParamsList = ResolveExpression.ResolveWhereList(Context.Set,ref whereSql, Params);

            var orderbySql = ResolveExpression.ResolveOrderBy(Context.Set.OrderbyExpressionList);

            SqlString = $@"SELECT T.* FROM( 
                            {selectSql}
                            {fromTableSql} {joinSql}
                            {whereSql}
                            {orderbySql}
                            ) T
                            WHERE ROWNUM<=1";

            return this;
        }

        public override SqlProvider FormatToList<T>()
        {
            var topNum = DataBaseContext<T>().QuerySet.TopNum;

            var selectSql = ResolveExpression.ResolveSelect(EntityCache.QueryEntity(typeof(T)), Context.Set.SelectExpression);

            var fromTableSql = FormatTableName();

            var joinSql = ResolveExpression.ResolveJoinSql(JoinList, ref selectSql, Context.Set.SelectExpression);

            var whereSql = string.Empty;

            //表查询条件
            var whereParamsList = ResolveExpression.ResolveWhereList(Context.Set, ref whereSql, Params);

            var orderbySql = ResolveExpression.ResolveOrderBy(Context.Set.OrderbyExpressionList);

            SqlString = $"{selectSql} {fromTableSql} {joinSql} {whereSql} {orderbySql}";

            return this;
        }

        public override SqlProvider FormatToPageList<T>(int pageIndex, int pageSize,bool IsSelectCount=true)
        {
            var orderbySql = ResolveExpression.ResolveOrderBy(Context.Set.OrderbyExpressionList);
            //Oracle可以不用必须排序翻页
            //if (string.IsNullOrEmpty(orderbySql))
            //    throw new DapperExtensionException("order by takes precedence over pagelist");

            var selectSql = ResolveExpression.ResolveSelect(EntityCache.QueryEntity(typeof(T)), Context.Set.SelectExpression);

            var fromTableSql = FormatTableName();

            var joinSql = ResolveExpression.ResolveJoinSql(JoinList, ref selectSql, Context.Set.SelectExpression);

            var whereSql = string.Empty;

            //表查询条件
            var whereParamsList = ResolveExpression.ResolveWhereList(Context.Set, ref whereSql, Params);

            SqlString = $@" SELECT T2.* FROM(
                            SELECT T.*,ROWNUM ROWNUMS FROM (
                            SELECT 
                             {selectSql.Replace("SELECT", "")}
                            {fromTableSql} {joinSql} {whereSql} {orderbySql}
                            ) T 
                            )T2
                            WHERE ROWNUMS BETWEEN {((pageIndex - 1) * pageSize) + 1} and {pageIndex * pageSize}";

            IsSelectCount = true;
            return this;
        }

        public override SqlProvider FormatCount()
        {
            var selectSql = "SELECT COUNT(1)";

            var fromTableSql = FormatTableName();

            string noneSql = "";
            var joinSql = ResolveExpression.ResolveJoinSql(JoinList, ref noneSql, Context.Set.SelectExpression);

            var whereSql = string.Empty;

            //表查询条件
            var whereParamsList = ResolveExpression.ResolveWhereList(Context.Set, ref whereSql, Params);

            SqlString = $"{selectSql} {fromTableSql} {joinSql} {whereSql} ";

            return this;
        }

        public override SqlProvider FormatDelete()
        {
            var fromTableSql = FormatTableName();

            var whereSql = string.Empty;

            //表查询条件
            var whereParamsList = ResolveExpression.ResolveWhereList(Context.Set, ref whereSql, Params);

            SqlString = $"DELETE {fromTableSql} {whereSql }";

            return this;
        }

        public override SqlProvider FormatInsert<T>(T entity)
        {
            var paramsAndValuesSql = FormatInsertParamsAndValues(entity);
            SqlString = $"INSERT INTO {FormatTableName(false)} ({paramsAndValuesSql[0]}) VALUES({paramsAndValuesSql[1]})";

            //if (Context.Set.IfNotExistsExpression == null)
            //    SqlString = $"INSERT INTO {FormatTableName(false)} ({paramsAndValuesSql[0]}) VALUES({paramsAndValuesSql[1]})";
            //else
            //{
            //    var ifnotexistsWhere = ResolveExpression.ResolveWhere(Context.Set.IfNotExistsExpression, "INT_");

            //    SqlString = string.Format(@"INSERT INTO {0}({1})  
            //    SELECT {2}
            //    WHERE NOT EXISTS(
            //        SELECT 1
            //        FROM {0}  
            //    {3}
            //        ); ", FormatTableName(false), paramsAndValuesSql[0], paramsAndValuesSql[1], ifnotexistsWhere.SqlCmd);

            //    Params.AddDynamicParams(ifnotexistsWhere.Param);
            //}

            return this;
        }

        public override SqlProvider FormatUpdate<T>(Expression<Func<T, T>> updateExpression)
        {
            var update = ResolveExpression.ResolveUpdate(updateExpression);

            var whereSql = string.Empty;

            //表查询条件
            var whereParamsList = ResolveExpression.ResolveWhereList(Context.Set, ref whereSql, Params);
            Params.AddDynamicParams(update.Param);

            SqlString = $"UPDATE {FormatTableName(false)} {update.SqlCmd} {whereSql}";

            return this;
        }

        public override SqlProvider FormatUpdate<T>(T entity)
        {
            var update = ResolveExpression.ResolveUpdate<T>(a => entity);

            var whereSql = string.Empty;

            //表查询条件
            var whereParamsList = ResolveExpression.ResolveWhereList(Context.Set, ref whereSql, Params);
            Params.AddDynamicParams(update.Param);

            SqlString = $"UPDATE {FormatTableName(false)} {update.SqlCmd} {whereSql}";

            return this;
        }

        public override SqlProvider FormatSum(LambdaExpression sumExpression)
        {
            var selectSql = ResolveExpression.ResolveSum(sumExpression);

            var fromTableSql = FormatTableName();

            string noneSql = "";
            var joinSql = ResolveExpression.ResolveJoinSql(JoinList, ref noneSql, Context.Set.SelectExpression);

            var whereSql = string.Empty;

            //表查询条件
            var whereParamsList = ResolveExpression.ResolveWhereList(Context.Set, ref whereSql, Params);

            SqlString = $"{selectSql} {fromTableSql}{joinSql} {whereSql} ";

            return this;
        }

        public override SqlProvider FormatUpdateSelect<T>(Expression<Func<T, T>> updator)
        {
            var update = ResolveExpression.ResolveUpdate(updator);

            var selectSql = ResolveExpression.ResolveSelectOfUpdate(EntityCache.QueryEntity(typeof(T)), Context.Set.SelectExpression);

            var whereSql = string.Empty;

            //表查询条件
            var whereParamsList = ResolveExpression.ResolveWhereList(Context.Set, ref whereSql, Params);
            Params.AddDynamicParams(update.Param);

            SqlString = $"UPDATE {FormatTableName(false)} {update.SqlCmd} {selectSql} {whereSql}";

            return this;
        }
    }
}