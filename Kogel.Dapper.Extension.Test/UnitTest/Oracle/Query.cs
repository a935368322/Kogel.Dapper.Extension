using Kogel.Dapper.Extension.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Kogel.Dapper.Extension.Oracle;
using Oracle.ManagedDataAccess.Client;
using Kogel.Dapper.Extension.Test.ViewModel;
using Kogel.Dapper.Extension.Oracle.Extension;
using Dapper;
using Kogel.Dapper.Extension.Test.Model.Digiwin;
using Digiwin.MES.Server.Application.Domain.EQP.Entities;

namespace Kogel.Dapper.Extension.Test.UnitTest.Oracle
{
    public class Query
    {
        string oracleConnection = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.105.0.224)(PORT=1521))
                    (CONNECT_DATA=(SERVICE_NAME=ORCL)));User Id=system;Password=A123456a";
        public void Test()
        {
            using (var conn = new OracleConnection(oracleConnection))
            {

                //conn.Open();

                //EntityCache.Register(typeof(EQP_TYPE_BAS));
                ////EntityCache.Register(typeof(SYS_USER));
                ////EntityCache.Register(typeof(SYS_USER_ROLE));

                //CodeFirst codeFirst = new CodeFirst(conn);
                //codeFirst.SyncStructure();

                //SqlMapper.Aop.OnExecuting += (ref CommandDefinition command) =>
                //  {

                //  };

                //var aaa = conn.QuerySet<Comment>()
                //	   .Count();
                var bbb = conn.QuerySet<Comment>()
                    .Join<Comment, News>((a, b) => a.ArticleId == b.Id && b.Id!=0 && b.Id>0)
                    .Where(x => x.Content.IsNotNull() && x.Content.Contains("a") )
                    .Distinct()
                    .PageList(1, 10, x => new
                    {
                        id = 1,
                        x.Content
                    });


                var AAA = conn.QuerySet<Comment>()
                    .Where(x => x.Content.IsNull())
                    .ToList(x => new
                    {
                        id = x.Id,
                        content = x.Content
                    });

                //SqlMapper.RemoveTypeMap(typeof(EQP_TYPE_BAS));
                //var result233 = conn.QuerySet<EQP_TYPE_BAS>()
                //.Where(x => x.DELETE_FLAG == "N")
                //.OrderBy(x => x.CREATE_TIME)
                //.PageList(1, 10, x => new
                //{
                //	Test = Function.Concact(Convert.ToString(x.Id), "ttt")
                //});

                //Function


                var guid = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6");
                var result = conn.QuerySet<SYS_ROLE>()
                 .Join<SYS_USER_ROLE>($"LEFT JOIN SYS_USER_ROLE ON SYS_ROLE.GUID = SYS_USER_ROLE.ROLE_GUID AND SYS_USER_ROLE.USER_GUID = '{guid.ToString()}'")
                 //.Join<SYS_ROLE, SYS_USER_ROLE>((a, b) => a.GUID == b.ROLE_GUID && b.USER_GUID == guid.ToString())
                 .From<SYS_ROLE, SYS_USER_ROLE>()
                 .PageList(1, 10, (a, b) => new
                 {
                     a.FLAG,
                     b.USER_GUID
                 });



                //conn.QuerySet<TB_SYS_USER>()
                //	.Where("USER_ID=1")
                //	.ToList();

                var query = conn.QuerySet<TB_SYS_USER>().Join<TB_SYS_USER, TB_SYS_DEPT>((x, y) => (x.DEPT == y.DEPT_ID))
                    .Join<TB_SYS_USER, TB_TDG_UPH_EMP>((x, y) => x.USER_ID == y.EMP_ID)
                    .Where(a => a.USER_ID == "" && a.PASSWORD == "")
                    .From<TB_SYS_USER, TB_SYS_DEPT, TB_TDG_UPH_EMP>();
                var user = query.Get(
                    (a, b, c) => new TB_SYS_USER
                    {
                        DEPT = a.DEPT,
                        DEPT_NAME = b.DEPT_NAME,
                        DISABLE = a.DISABLE,
                        ISADMIN = a.ISADMIN,
                        REMARK = a.REMARK,
                        USER_ID = a.USER_ID,
                        USER_NAME = a.USER_NAME,
                        BU_ID = c.BU_ID,
                        BU_NAME = c.BU_NAME
                    });




                DateTime dateTime = DateTime.Now.AddDays(-10);

                var test = conn.QuerySet<Comment>()
                    .ToList();

                var comm = conn.QuerySet<Comment>().PageList(1, 10);

                var getIfTest = conn.QuerySet<Comment>()
                    .Where(x => x.Content.In(new string[] { "1", "2", "3" }))
                    .Get(false, x => new CommentDto()
                    {
                        Id = x.Id,
                        ArticleIds = x.ArticleId
                    }, x => new CommentDto()
                    {
                        Id = x.Id,
                        Content = x.Content
                    });

                //单个属性返回
                var ContentList = conn.QuerySet<Comment>()
                     .Where(x => x.Content.IsNotNull() && x.Content != "")
                     .WhereIf(!string.IsNullOrEmpty("aaa"), x => x.ArticleId == 1, x => x.ArticleId == 2)
                     .ToList(x => new CommentDto()
                     {
                         Id = x.Id,
                         ArticleIds = x.ArticleId,
                         count = conn.QuerySet<News>().Where(y => y.Id == x.ArticleId).Count(),
                         NewsList = conn.QuerySet<News>().Where(y => y.Id == x.ArticleId).ToList(y => new NewsDto()
                         {
                             Id = y.Id,
                             Contents = y.Content
                         }).ToList(),
                         NewsDto = conn.QuerySet<News>().Where(y => y.Id == x.ArticleId).Get(y => new NewsDto()
                         {
                             Id = y.Id,
                             Contents = y.Content
                         })
                     });

                var commne = conn.QuerySet<Comment>()
                    .Where(x => x.Id > 0)
                    .Get(x => new
                    {
                        x.Id,
                        x.Content
                    });
                //翻页
                var comment1 = conn.QuerySet<Comment>()
                    .Join<Comment, News>((a, b) => a.ArticleId == b.Id)
                    .Where(x => x.Id.Between(80, 100)
                    && x.SubTime.AddDays(-10) < DateTime.Now && x.Id > 10
                    && x.Id > conn.QuerySet<News>().Where(y => y.Id < 3 && x.Id < y.Id).Sum(y => y.Id))
                    .From<Comment, News>()
                    .OrderBy<News>(x => x.Id)
                    .PageList(1, 1, (a, b) => new
                    {
                        test = new List<int>() { 3, 3, 1 }.FirstOrDefault(y => y == 1),
                        aaa = "6666" + "777",
                        Content = a.Content + "'test'" + b.Headlines + a.IdentityId,
                        bbb = conn.QuerySet<Comment1>()
                                    .Where(y => y.ArticleId == b.Id && y.Content.Contains("test"))
                                    .Sum(x => x.Id),
                        ccc = a.IdentityId,
                        a.Id,
                        times = DateTime.Now
                    });


                //多视图查询
                var comment5 = conn.QuerySet<Comment>()
                     .Join<Comment, News>((a, b) => a.ArticleId == b.Id)
                     .Join<Comment, ResourceMapping>((a, b) => a.Id == b.FKId)
                     .Where(x => x.Content == "test")
                     .From<Comment, News, ResourceMapping>()
                     .OrderBy<News>(x => x.Id)
                     .Where((a, b, c) => a.ArticleId == b.Id)
                     .PageList(1, 10, (a, b, c) => new
                     {
                         id = a.Id,
                         name = b.NewsLabel,
                         resources = c.RPath,
                     });

                //计总
                var sum = conn.QuerySet<Comment>()
                       .Join<Comment, News>((a, b) => a.ArticleId == b.Id)
                       .Where(x => x.Content == "test")
                       .WithNoLock()
                       .Sum(x => x.Id);
                //计数
                var count = conn.QuerySet<Comment>()
                     .Join<Comment, News>((a, b) => a.ArticleId == b.Id)
                     .Where(x => x.Content == "test1")
                     .Where<News>(x => x.NewsLabel.Contains("足球"))
                     .WithNoLock()
                     .Count();


            }
        }
    }
}
