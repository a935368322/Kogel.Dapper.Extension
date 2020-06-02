using Dapper;
using Kogel.Dapper.Extension.Core.Interfaces;
using Kogel.Dapper.Extension.Extension;
using Kogel.Dapper.Extension.Model;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using Kogel.Dapper.Extension.Helper;
using System.Collections.Generic;
using Kogel.Dapper.Extension.Exception;
using System;
using System.Reflection;

namespace Kogel.Dapper.Extension.Expressions
{
    /// <summary>
    /// 解析自定义查询字段
    /// </summary>
    public class SelectExpression : BaseExpressionVisitor
    {
        #region sql指令
        private readonly StringBuilder _sqlCmd;
        /// <summary>
        /// sql指令
        /// </summary>
        public string SqlCmd => _sqlCmd.ToString();
        /// <summary>
        /// 参数
        /// </summary>
        public new DynamicParameters Param;

        protected List<SelectField> SelectFieldPairs;
        #endregion
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="prefix">字段前缀</param>
        /// <param name="providerOption"></param>
        public SelectExpression(LambdaExpression expression, string prefix, SqlProvider provider) : base(provider)
        {
            this._sqlCmd = new StringBuilder();
            this.Param = new DynamicParameters();
            this.SelectFieldPairs = new List<SelectField>();
            //判断是不是实体类
            if (expression.Body is MemberInitExpression)
            {
                var memberInitExpression = expression.Body as MemberInitExpression;
                foreach (MemberAssignment memberInit in memberInitExpression.Bindings)
                {
                    base.SpliceField.Clear();
                    base.Param = new DynamicParameters();
                    if (_sqlCmd.Length != 0)
                        _sqlCmd.Append(",");
                    //实体类型
                    Type entityType;
                    //验证是实体类或者是泛型
                    if (ExpressionExtension.IsAnyBaseEntity(memberInit.Expression.Type, out entityType))
                    {
                        if (!memberInit.Expression.ToString().Contains("QuerySet"))
                        {
                            //导航属性
                            var navigationTable = provider.JoinList.FirstOrDefault(x => x.TableType.IsTypeEquals(entityType));
                            if (navigationTable != null)
                            {
                                navigationTable.IsMapperField = true;
                                //当前定义的查询返回对象
                                EntityObject entity = EntityCache.QueryEntity(entityType);
                                navigationTable.SelectFieldPairs = entity.FieldPairs.Select(x => new SelectField { Key = x.Key, Value = x.Value }).ToList();
                            }
                            //不存在第一层中，可能在后几层嵌套使用导航属性
                            //获取调用者表达式
                            var parentExpression = (memberInit.Expression as MemberExpression).Expression;
                            var parentEntity = EntityCache.QueryEntity(parentExpression.Type);
                            navigationTable = parentEntity.Navigations.Find(x => x.TableType == entityType);
                            if (navigationTable != null)
                            {
                                navigationTable = (JoinAssTable)navigationTable.Clone();
                                navigationTable.IsMapperField = true;
                                //加入导航连表到提供方
                                Provider.JoinList.Add(navigationTable);
                                //当前定义的查询返回对象
                                EntityObject entity = EntityCache.QueryEntity(entityType);
                                navigationTable.SelectFieldPairs = entity.FieldPairs.Select(x => new SelectField { Key = x.Key, Value = x.Value }).ToList();
                            }
                        }
                        else
                        {
                            _sqlCmd.Remove(_sqlCmd.Length - 1, 1);
                            //自定义查询列表
                            providerOption.NavigationList.Add(new NavigationMemberAssign()
                            {
                                MemberAssign = memberInit,
                                MemberAssignName = memberInit.Member.Name
                            });
                        }
                    }
                    else if (memberInit.Expression.Type.FullName.Contains("System.Collections.Generic"))//Select Dto
                    {
                        var selectMethCall = (memberInit.Expression as MethodCallExpression).Arguments[0] as MethodCallExpression;
                        var selectExpression = new SelectExpression(selectMethCall.Arguments[1] as LambdaExpression, prefix + "_Dto", provider);
                        Type selectEntity;
                        if (ExpressionExtension.IsAnyBaseEntity(selectMethCall.Arguments[0].Type, out selectEntity))
                        {
                            //导航属性
                            var itemJoin = provider.JoinList.FirstOrDefault(x => x.TableType.IsTypeEquals(selectEntity));
                            if (itemJoin != null)
                            {
                                itemJoin.IsMapperField = true;
                                itemJoin.IsDto = true;
                                itemJoin.DtoType = memberInit.Expression.Type.GenericTypeArguments[0];
                                itemJoin.SelectFieldPairs = selectExpression.SelectFieldPairs;
                            }
                        }
                    }
                    else
                    {
                        //值对象
                        Visit(memberInit.Expression);
                        _sqlCmd.Append($"{base.SpliceField} AS {memberInit.Member.Name}");
                        this.SelectFieldPairs.Add(new SelectField { Key = base.SpliceField.ToString(), Value = memberInit.Member.Name });
                        Param.AddDynamicParams(base.Param);
                        //记录映射字段对应关系
                        providerOption.MappingList.Add(base.SpliceField.ToString(), memberInit.Member.Name);
                    }
                    base.Index++;
                }
            }
            else//匿名类
            {
                //当前定义的查询返回对象
                EntityObject entity = EntityCache.QueryEntity(expression.Body.Type);
                var newExpression = expression.Body as NewExpression;
                if (newExpression != null)
                {
                    foreach (var argument in newExpression.Arguments)
                    {
                        base.SpliceField.Clear();
                        base.Param = new DynamicParameters();
                        if (_sqlCmd.Length != 0)
                            _sqlCmd.Append(",");
                        //返回类型
                        var returnProperty = entity.Properties[base.Index];
                        //实体类型
                        Type entityType;
                        //验证是实体类或者是泛型
                        if (ExpressionExtension.IsAnyBaseEntity(returnProperty.PropertyType, out entityType))
                        {
                            throw new DapperExtensionException("匿名类型不支持导航属性!");
                        }
                        else
                        {
                            //值对象
                            Visit(argument);
                            _sqlCmd.Append($" {base.SpliceField} AS {returnProperty.Name} ");
                            Param.AddDynamicParams(base.Param);
                        }
                        base.Index++;
                    }
                }
                else
                {
                    //单个属性，例如 .Get(x => x.Id);
                    Visit(expression.Body);
                    _sqlCmd.Append(base.SpliceField);
                    Param.AddDynamicParams(base.Param);
                }
            }
        }
    }
}
