using Kogel.Dapper.Extension.Core.SetQ;
using Kogel.Dapper.Extension.Extension.From;
//using Kogel.Dapper.Extension.MySql;
using Kogel.Dapper.Extension.Test.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Data.SqlClient;
using Kogel.Dapper.Extension.MsSql;
using Kogel.Dapper.Extension.Model;

namespace Kogel.Dapper.Extension.Test
{
    class Program
    {
        static void Main(string[] args)
        {
			////预热实体类
			//EntityCache.Register(new Type[] {
			//	typeof(Comment),
			//	typeof(Comment1),
			//	typeof(News),
			//	typeof(ResourceMapping),
			//});

			#region mssql单元测试
			//var mssqlQuery = new UnitTest.Mssql.Query();
			//mssqlQuery.Test();

			//var mssqlCommand = new UnitTest.Mssql.Command();
			//mssqlCommand.Test();
			//#endregion

			//#region mysql单元测试
			//var mysqlQuery = new UnitTest.Mysql.Query();
			//mysqlQuery.Test();

			//var mysqlResQuery = new UnitTest.Mysql.TestRepositoryQuery();
			//mysqlResQuery.Test();
			////mysqlQuery.TestMaxAndMin();

			//var mysqlCommand = new UnitTest.Mysql.Command();
			//mysqlCommand.Test();
			#endregion

			//stopwatch.Stop();

			//#region oracle单元测试
			var oracleQuery = new UnitTest.Oracle.Query();
			oracleQuery.Test();

			//var oracleCommand = new UnitTest.Oracle.Command();
			//oracleCommand.Test();
			//#endregion
		}
	}
}
