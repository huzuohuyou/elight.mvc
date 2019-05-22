



















// This file was automatically generated by the PetaPoco T4 Template
// Do not make changes directly to this file - edit the template instead
// 
// The following connection settings were used to generate this file
// 
//     Connection String Name: `connStr`
//     Provider:               `System.Data.SqlClient`
//     Connection String:      `Data Source=.\SQLEXPRESS;Initial Catalog=Elight_v1.0;User ID=sa;Password=sql2014`
//     Schema:                 ``
//     Include Views:          `False`



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Elight.Entity
{

	public partial class DbContext : Database
	{
		public DbContext() 
			: base("connStr")
		{
			CommonConstruct();
		}

		public DbContext(string connectionStringName) 
			: base(connectionStringName)
		{
			CommonConstruct();
		}
		
		partial void CommonConstruct();
		
		public interface IFactory
		{
			DbContext GetInstance();
		}
		
		public static IFactory Factory { get; set; }
        public static DbContext GetInstance()
        {
			if (_instance!=null)
				return _instance;
				
			if (Factory!=null)
				return Factory.GetInstance();
			else
				return new DbContext();
        }

		[ThreadStatic] static DbContext _instance;
		
		public override void OnBeginTransaction()
		{
			if (_instance==null)
				_instance=this;
		}
		
		public override void OnEndTransaction()
		{
			if (_instance==this)
				_instance=null;
		}
        

		public class Record<T> where T:new()
		{
			public static DbContext repo { get { return DbContext.GetInstance(); } }
			public bool IsNew() { return repo.IsNew(this); }
			public object Insert() { return repo.Insert(this); }

			public void Save() { repo.Save(this); }
			public int Update() { return repo.Update(this); }

			public int Update(IEnumerable<string> columns) { return repo.Update(this, columns); }
			public static int Update(string sql, params object[] args) { return repo.Update<T>(sql, args); }
			public static int Update(Sql sql) { return repo.Update<T>(sql); }
			public int Delete() { return repo.Delete(this); }
			public static int Delete(string sql, params object[] args) { return repo.Delete<T>(sql, args); }
			public static int Delete(Sql sql) { return repo.Delete<T>(sql); }
			public static int Delete(object primaryKey) { return repo.Delete<T>(primaryKey); }
			public static bool Exists(object primaryKey) { return repo.Exists<T>(primaryKey); }
			public static bool Exists(string sql, params object[] args) { return repo.Exists<T>(sql, args); }
			public static T SingleOrDefault(object primaryKey) { return repo.SingleOrDefault<T>(primaryKey); }
			public static T SingleOrDefault(string sql, params object[] args) { return repo.SingleOrDefault<T>(sql, args); }
			public static T SingleOrDefault(Sql sql) { return repo.SingleOrDefault<T>(sql); }
			public static T FirstOrDefault(string sql, params object[] args) { return repo.FirstOrDefault<T>(sql, args); }
			public static T FirstOrDefault(Sql sql) { return repo.FirstOrDefault<T>(sql); }
			public static T Single(object primaryKey) { return repo.Single<T>(primaryKey); }
			public static T Single(string sql, params object[] args) { return repo.Single<T>(sql, args); }
			public static T Single(Sql sql) { return repo.Single<T>(sql); }
			public static T First(string sql, params object[] args) { return repo.First<T>(sql, args); }
			public static T First(Sql sql) { return repo.First<T>(sql); }
			public static List<T> Fetch(string sql, params object[] args) { return repo.Fetch<T>(sql, args); }
			public static List<T> Fetch(Sql sql) { return repo.Fetch<T>(sql); }
			public static List<T> Fetch(long page, long itemsPerPage, string sql, params object[] args) { return repo.Fetch<T>(page, itemsPerPage, sql, args); }
			public static List<T> Fetch(long page, long itemsPerPage, Sql sql) { return repo.Fetch<T>(page, itemsPerPage, sql); }
			public static List<T> SkipTake(long skip, long take, string sql, params object[] args) { return repo.SkipTake<T>(skip, take, sql, args); }
			public static List<T> SkipTake(long skip, long take, Sql sql) { return repo.SkipTake<T>(skip, take, sql); }
			public static Page<T> Page(long page, long itemsPerPage, string sql, params object[] args) { return repo.Page<T>(page, itemsPerPage, sql, args); }
			public static Page<T> Page(long page, long itemsPerPage, Sql sql) { return repo.Page<T>(page, itemsPerPage, sql); }
			public static IEnumerable<T> Query(string sql, params object[] args) { return repo.Query<T>(sql, args); }
			public static IEnumerable<T> Query(Sql sql) { return repo.Query<T>(sql); }

		}

	}
	



    

	[TableName("dbo.Sys_Item")]



	[PrimaryKey("Id")]




	[ExplicitColumns]

    public partial class Sys_Item : DbContext.Record<Sys_Item>  
    {



		[Column] public string Id { get; set; }





		[Column] public string EnCode { get; set; }





		[Column] public string ParentId { get; set; }





		[Column] public string Name { get; set; }





		[Column] public int? Layer { get; set; }





		[Column] public int? SortCode { get; set; }





		[Column] public bool? IsTree { get; set; }





		[Column] public bool? DeleteMark { get; set; }





		[Column] public bool? IsEnabled { get; set; }





		[Column] public string Remark { get; set; }





		[Column] public string CreateUser { get; set; }





		[Column] public DateTime? CreateTime { get; set; }





		[Column] public string ModifyUser { get; set; }





		[Column] public DateTime? ModifyTime { get; set; }



	}

    

	[TableName("dbo.Sys_ItemsDetail")]



	[PrimaryKey("Id")]




	[ExplicitColumns]

    public partial class Sys_ItemsDetail : DbContext.Record<Sys_ItemsDetail>  
    {



		[Column] public string Id { get; set; }





		[Column] public string ItemId { get; set; }





		[Column] public string EnCode { get; set; }





		[Column] public string Name { get; set; }





		[Column] public bool? IsDefault { get; set; }





		[Column] public int? SortCode { get; set; }





		[Column] public bool? DeleteMark { get; set; }





		[Column] public bool? IsEnabled { get; set; }





		[Column] public string CreateUser { get; set; }





		[Column] public DateTime? CreateTime { get; set; }





		[Column] public string ModifyUser { get; set; }





		[Column] public DateTime? ModifyTime { get; set; }



	}

    

	[TableName("dbo.Sys_Log")]



	[PrimaryKey("Id", AutoIncrement=false)]


	[ExplicitColumns]

    public partial class Sys_Log : DbContext.Record<Sys_Log>  
    {



		[Column] public string Id { get; set; }





		[Column] public DateTime? CreateTime { get; set; }





		[Column] public string LogLevel { get; set; }





		[Column] public string Operation { get; set; }





		[Column] public string Message { get; set; }





		[Column] public string Account { get; set; }





		[Column] public string RealName { get; set; }





		[Column] public string IP { get; set; }





		[Column] public string IPAddress { get; set; }





		[Column] public string Browser { get; set; }





		[Column] public string StackTrace { get; set; }



	}

    

	[TableName("dbo.Sys_Organize")]



	[PrimaryKey("Id")]




	[ExplicitColumns]

    public partial class Sys_Organize : DbContext.Record<Sys_Organize>  
    {



		[Column] public string Id { get; set; }





		[Column] public string ParentId { get; set; }





		[Column] public int? Layer { get; set; }





		[Column] public string EnCode { get; set; }





		[Column] public string FullName { get; set; }





		[Column] public short? Type { get; set; }





		[Column] public string ManagerId { get; set; }





		[Column] public string TelePhone { get; set; }





		[Column] public string WeChat { get; set; }





		[Column] public string Fax { get; set; }





		[Column] public string Email { get; set; }





		[Column] public string Address { get; set; }





		[Column] public int? SortCode { get; set; }





		[Column] public bool? DeleteMark { get; set; }





		[Column] public bool? IsEnabled { get; set; }





		[Column] public string Remark { get; set; }





		[Column] public string CreateUser { get; set; }





		[Column] public DateTime? CreateTime { get; set; }





		[Column] public string ModifyUser { get; set; }





		[Column] public DateTime? ModifyTime { get; set; }



	}

    

	[TableName("dbo.Sys_Permission")]



	[PrimaryKey("Id", AutoIncrement = false)]




	[ExplicitColumns]

    public partial class Sys_Permission : DbContext.Record<Sys_Permission>  
    {


        private string id;
        [Column]
        public string Id
        {
            get
            {
                if (id == "")
                {
                    return Guid.NewGuid().ToString();
                }
                return id;
            }
            set
            {
                id = value;
            }
        }





        [Column] public string ParentId { get; set; }





		[Column] public int? Layer { get; set; }





		[Column] public string EnCode { get; set; }





		[Column] public string Name { get; set; }





		[Column] public string JsEvent { get; set; }





		[Column] public string Icon { get; set; }





		[Column] public string Url { get; set; }





		[Column] public string Remark { get; set; }





		[Column] public int? Type { get; set; }





		[Column] public int? SortCode { get; set; }





		[Column] public bool? IsPublic { get; set; }





		[Column] public bool? IsEnable { get; set; }





		[Column] public bool? IsEdit { get; set; }





		[Column] public bool? DeleteMark { get; set; }





		[Column] public string CreateUser { get; set; }





		[Column] public DateTime? CreateTime { get; set; }





		[Column] public string ModifyUser { get; set; }





		[Column] public DateTime? ModifyTime { get; set; }



	}

    

	[TableName("dbo.Sys_Role")]



	[PrimaryKey("Id")]




	[ExplicitColumns]

    public partial class Sys_Role : DbContext.Record<Sys_Role>  
    {



		[Column] public string Id { get; set; }





		[Column] public string OrganizeId { get; set; }





		[Column] public string EnCode { get; set; }





		[Column] public short? Type { get; set; }





		[Column] public string Name { get; set; }





		[Column] public bool? AllowEdit { get; set; }





		[Column] public bool? DeleteMark { get; set; }





		[Column] public bool? IsEnabled { get; set; }





		[Column] public string Remark { get; set; }





		[Column] public int? SortCode { get; set; }





		[Column] public string CreateUser { get; set; }





		[Column] public DateTime? CreateTime { get; set; }





		[Column] public string ModifyUser { get; set; }





		[Column] public DateTime? ModifyTime { get; set; }



	}

    

	[TableName("dbo.Sys_RoleAuthorize")]



	[PrimaryKey("Id")]




	[ExplicitColumns]

    public partial class Sys_RoleAuthorize : DbContext.Record<Sys_RoleAuthorize>  
    {



		[Column] public string Id { get; set; }





		[Column] public string RoleId { get; set; }





		[Column] public string ModuleId { get; set; }





		[Column] public string CreateUser { get; set; }





		[Column] public DateTime? CreateTime { get; set; }



	}

    

	[TableName("dbo.Sys_User")]



	[PrimaryKey("Id")]




	[ExplicitColumns]

    public partial class Sys_User : DbContext.Record<Sys_User>  
    {



		[Column] public string Id { get; set; }





		[Column] public string Account { get; set; }





		[Column] public string RealName { get; set; }





		[Column] public string NickName { get; set; }





		[Column] public string Avatar { get; set; }





		[Column] public bool? Gender { get; set; }





		[Column] public DateTime? Birthday { get; set; }





		[Column] public string MobilePhone { get; set; }





		[Column] public string Email { get; set; }





		[Column] public string Signature { get; set; }





		[Column] public string Address { get; set; }





		[Column] public string CompanyId { get; set; }





		[Column] public string DepartmentId { get; set; }





		[Column] public bool? IsEnabled { get; set; }





		[Column] public int? SortCode { get; set; }





		[Column] public bool? DeleteMark { get; set; }





		[Column] public string CreateUser { get; set; }





		[Column] public DateTime? CreateTime { get; set; }





		[Column] public string ModifyUser { get; set; }





		[Column] public DateTime? ModifyTime { get; set; }



	}

    

	[TableName("dbo.Sys_UserLogOn")]



	[PrimaryKey("Id")]




	[ExplicitColumns]

    public partial class Sys_UserLogOn : DbContext.Record<Sys_UserLogOn>  
    {



		[Column] public string Id { get; set; }





		[Column] public string UserId { get; set; }





		[Column] public string Password { get; set; }





		[Column] public string SecretKey { get; set; }





		[Column] public DateTime? PrevVisitTime { get; set; }





		[Column] public DateTime? LastVisitTime { get; set; }





		[Column] public DateTime? ChangePwdTime { get; set; }





		[Column] public int LoginCount { get; set; }





		[Column] public bool? AllowMultiUserOnline { get; set; }





		[Column] public bool? IsOnLine { get; set; }





		[Column] public string Question { get; set; }





		[Column] public string AnswerQuestion { get; set; }





		[Column] public bool? CheckIPAddress { get; set; }





		[Column] public string Language { get; set; }





		[Column] public string Theme { get; set; }



	}

    

	[TableName("dbo.Sys_UserRoleRelation")]



	[PrimaryKey("Id")]




	[ExplicitColumns]

    public partial class Sys_UserRoleRelation : DbContext.Record<Sys_UserRoleRelation>  
    {



		[Column] public string Id { get; set; }





		[Column] public string UserId { get; set; }





		[Column] public string RoleId { get; set; }





		[Column] public string CreateUser { get; set; }





		[Column] public DateTime? CreateTime { get; set; }



	}


}
