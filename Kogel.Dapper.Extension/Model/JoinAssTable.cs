using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dapper;

namespace Kogel.Dapper.Extension.Model
{
	public class JoinAssTable : ICloneable
	{
		public JoinAssTable()
		{
			MapperList = new Dictionary<string, string>();
		}

		public JoinAction Action { get; set; }
		public JoinMode JoinMode { get; set; }
		public string RightTabName { get; set; }
		public string LeftTabName { get; set; }
		public string RightAssName { get; set; }
		public string LeftAssName { get; set; }
		public Type TableType { get; set; }
		public string JoinSql { get; set; }
		public Type PropertyType { get => PropertyInfo.PropertyType; }
		/// <summary>
		/// 接收导航属性的对象
		/// </summary>
		public PropertyInfo PropertyInfo { get; set; }

		/// <summary>
		/// 自定义查询的字段
		/// </summary>
		public List<SelectField> SelectFieldPairs { get; set; }

		/// <summary>
		/// 表首字段
		/// </summary>
		public string FirstFieldName => MapperList.Values?.AsList()[0];

		/// <summary>
		/// 表尾字段
		/// </summary>
		public string LastFieldName => MapperList.Values.AsList()[MapperList.Count - 1];

		/// <summary>
		/// 隐射目录
		/// </summary>
		public Dictionary<string, string> MapperList { get; set; }

		/// <summary>
		/// 是否手动开关映射(优先级最高)
		/// </summary>
		public bool IsUseMapper { get; set; } = true;

		/// <summary>
		/// 是否需要隐射字段
		/// </summary>
		public bool IsMapperField { get; set; } = true;

		/// <summary>
		/// 是否是Dto
		/// </summary>
		public bool IsDto { get; set; } = false;

		/// <summary>
		/// Dto类
		/// </summary>
		public Type DtoType { get; set; }

		/// <summary>
		/// 克隆
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new JoinAssTable
			{
				Action = this.Action,
				JoinMode = this.JoinMode,
				RightTabName = this.RightTabName,
				LeftTabName = this.LeftTabName,
				RightAssName = this.RightAssName,
				LeftAssName = this.LeftAssName,
				TableType = this.TableType,
				JoinSql = this.JoinSql,
				SelectFieldPairs = new List<SelectField>(),
				MapperList = new Dictionary<string, string>(),
				IsMapperField = false,
				PropertyInfo = this.PropertyInfo
			};
		}
	}

	/// <summary>
	/// 连表方式
	/// </summary>
	public enum JoinAction
	{
		Default,//默认表达式
		Sql,//sql查询
		Navigation//导航属性
	}

	public enum JoinMode
	{
		LEFT,//左连接
		RIGHT,//右连接
		INNER,//内连接
		FULL,//全连接
	}
}
