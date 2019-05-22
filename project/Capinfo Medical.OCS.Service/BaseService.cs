using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Elight.Infrastructure;
using EntityFramework.Extensions;

namespace TestingCenterSystem.Service
{

    /// <summary>
    /// 数据访问层父类。
    /// </summary>
    public partial class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {

        private DbContext _db = null;
        public string UserId { get { return OperatorProvider.Instance.Current.UserId; } }
        public DateTime Now { get { return DateTime.Now; } }


        public DbContext Db
        {
            get
            {
                if (_db == null)
                {
                    _db = null;// new PDPContext();
                }
                return _db;
            }
            set
            {
                _db = value;
            }
        }
        public BaseService()
        {
            //Db = new PDPContext();
        }

        public bool ExistAliveTask(List<Task> taskList)
        {
            foreach (var item in taskList)
            {
                if (!item.IsCompleted)
                {
                    return true;
                }
            }
            taskList.Where(r => r.IsCompleted == true).ToList().ForEach(r => { taskList.Remove(r); });
            return false;
        }
        public bool Exists(Expression<Func<TEntity, bool>> where)
        {
            return Get(where) != null;
        }

        public TEntity Get(Expression<Func<TEntity, bool>> where)
        {
            var entity = Db.Set<TEntity>().AsNoTracking().Where(where).FirstOrDefault();
            return entity;
        }

        public object GetInfo(TEntity model, string name)
        {
            return model.GetType().GetProperty(name).GetValue(model, null);
        }

        /// <summary>
        /// 实体查询
        /// </summary>
        public List<TEntity> GetSearchList(Expression<Func<TEntity, bool>> where)
        {
            return Db.Set<TEntity>().AsNoTracking().Where(where).ToList();
        }

        /// <summary>
        /// 应保证所有的实体都有UPD_USER_ID & UPD_DATE字段
        /// 否则报错
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public TEntity Insert(TEntity model)
        {
            dynamic temp = (dynamic)model;
            try
            {
                temp.UPD_USER_ID = UserId;
                temp.UPD_DATE = Now;
            }
            catch (Exception)
            {
            }

            try
            {
                temp.CREATE_DATE = Now;
                temp.CREATE_USER_ID = UserId;
            }
            catch (Exception ex)
            {
            }
            Db.Entry<TEntity>(temp).State = EntityState.Added;
            Db.SaveChanges();
            return model;
        }

        public void BulkInsert<T>(SqlConnection conn, string tableName, IList<T> list)
        {
            using (var bulkCopy = new SqlBulkCopy(conn))
            {
                bulkCopy.BatchSize = list.Count;
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();
                var props = TypeDescriptor.GetProperties(typeof(T))

                    .Cast<PropertyDescriptor>()
                    .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                    .ToArray();

                foreach (var propertyInfo in props)
                {
                    bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                    table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Length];
                foreach (var item in list)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = props[i].GetValue(item);
                    }

                    table.Rows.Add(values);
                }

                bulkCopy.WriteToServer(table);
            }
        }

        public void BulkInsert<T>(string tableName, IList<T> list)
        {
            if (Db.Database.Connection.State != ConnectionState.Open)
            {
                Db.Database.Connection.Open(); //打开Connection连接  
            }
            using (var bulkCopy = new SqlBulkCopy(Db.Database.Connection as SqlConnection))
            {
                bulkCopy.BatchSize = list.Count;
                bulkCopy.DestinationTableName = tableName;

                var table = new DataTable();
                var props = TypeDescriptor.GetProperties(typeof(T))

                    .Cast<PropertyDescriptor>()
                    .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                    .ToArray();

                foreach (var propertyInfo in props)
                {
                    bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                    table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Length];
                foreach (var item in list)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = props[i].GetValue(item);
                    }

                    table.Rows.Add(values);
                }

                bulkCopy.WriteToServer(table);
            }
            if (Db.Database.Connection.State != ConnectionState.Closed)
            {
                Db.Database.Connection.Close(); //关闭Connection连接  
            }
        }

        /// <summary>
        /// 单个实体删除，字段内容需完全匹配
        ///【过时】
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Obsolete]
        public int Delete(TEntity model)
        {
            Db.Entry<TEntity>(model).State = EntityState.Deleted;
            return Db.SaveChanges();
        }

        /// <summary>
        /// 支持批量删除
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete(Expression<Func<TEntity, bool>> where)
        {
            foreach (var item in Db.Set<TEntity>().Where(where))
            {
                Db.Set<TEntity>().Attach(item);
                Db.Set<TEntity>().Remove(item);
            }
            return Db.SaveChanges();
        }

        /// <summary>
        /// 高效批量删除方法
        /// 推荐用ExDelete2
        /// wuhailong
        /// 2017-09-11
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public int ExDelete(Expression<Func<TEntity, bool>> where)
        {
            return Db.Set<TEntity>().Where(where).Delete();
        }

        /// <summary>
        /// 高效批量删除方法2
        /// 先查后删除
        /// wuhailong
        /// 2017129-27
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public int ExDelete2(Expression<Func<TEntity, bool>> where)
        {
            var dels = Db.Set<TEntity>().Where(where)?.ToList();
            dels.ForEach(r =>
            {
                Db.Entry<TEntity>(r).State = EntityState.Deleted;
            });
            return Db.SaveChanges();
        }

        /// <summary>
        /// 应保证所有的实体都有UPD_USER_ID & UPD_DATE字段
        /// 否则报错
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(TEntity model)
        {
            dynamic newValue = (dynamic)model;
            try
            {
                newValue.UPD_USER_ID = UserId;
                newValue.UPD_DATE = Now;
            }
            catch (Exception)
            {
            }
            Db.Entry<TEntity>(newValue).State = EntityState.Modified;
            return Db.SaveChanges();
        }

        public int Modify(TEntity originalValue, TEntity newValue)
        {
            if (Db.Entry(originalValue).State != EntityState.Unchanged)
            {
                Db.Entry(originalValue).State = EntityState.Unchanged;
            }
            Db.Entry(originalValue).CurrentValues.SetValues(newValue);
            return Db.SaveChanges();
        }

        public List<TEntity> GetManay(Expression<Func<TEntity, bool>> where)
        {
            return Db.Set<TEntity>().AsNoTracking().Where(where)?.ToList();
        }

        public List<TEntity> GetManayByOrder(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, int?>> order)
        {
            return Db.Set<TEntity>().AsNoTracking().Where(where).OrderBy(order).ToList();
        }

        public string ToIdentitySql(Expression<Func<TEntity, bool>> where)
        {
            StringBuilder strBuilder = new StringBuilder();
            var entities = Db.Set<TEntity>().Where(where).AsNoTracking().ToList();
            if (entities.Count > 0)
            {
                //strBuilder.Append($"DELETE FROM {typeof(TEntity).Name};\n");
                strBuilder.Append($"SET IDENTITY_INSERT {typeof(TEntity).Name} ON;\n");
                entities.ForEach(entity => strBuilder.Append(ToSQL(entity)));
                strBuilder.Append($"SET IDENTITY_INSERT {typeof(TEntity).Name} OFF;\n");
            }
            return strBuilder.ToString();
        }

        public string ToDeletsSql(TEntity model)
        {
            return $"DELETE FROM {typeof(TEntity).Name};\n";
        }
        public string ToIdentitySqlWithWhere(Expression<Func<TEntity, bool>> where, string whereSql)
        {
            StringBuilder strBuilder = new StringBuilder();
            var entities = Db.Set<TEntity>().Where(where).AsNoTracking().ToList();
            if (entities.Count > 0)
            {
                //strBuilder.Append($"DELETE FROM {typeof(TEntity).Name}  {whereSql};\n");
                strBuilder.Append($"SET IDENTITY_INSERT {typeof(TEntity).Name} ON;\n");
                entities.ForEach(entity => strBuilder.Append(ToSQL(entity)));
                strBuilder.Append($"SET IDENTITY_INSERT {typeof(TEntity).Name} OFF;\n");
            }
            return strBuilder.ToString();
        }

        /// <summary>
        /// 无主见自增使用
        /// wuhailong
        /// 2017-09-16
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public string ToNonIdentitySql(Expression<Func<TEntity, bool>> where)
        {
            StringBuilder strBuilder = new StringBuilder();
            var entities = Db.Set<TEntity>().Where(where).AsNoTracking().ToList();
            if (entities.Count > 0)
            {
                //strBuilder.Append($"DELETE FROM {typeof(TEntity).Name};\n");
                //strBuilder.Append($"SET IDENTITY_INSERT {typeof(TEntity).Name} ON;\n");
                entities.ForEach(entity => strBuilder.Append(ToSQL(entity)));
                //strBuilder.Append($"SET IDENTITY_INSERT {typeof(TEntity).Name} OFF;\n");
            }
            return strBuilder.ToString();
        }

        public string ToNonIdentitySqlWithWhere(Expression<Func<TEntity, bool>> where, string whereSql)
        {
            StringBuilder strBuilder = new StringBuilder();
            var entities = Db.Set<TEntity>().Where(where).AsNoTracking().ToList();
            if (entities.Count > 0)
            {
                //strBuilder.Append($"DELETE FROM {typeof(TEntity).Name}  {whereSql};\n");
                //strBuilder.Append($"SET IDENTITY_INSERT {typeof(TEntity).Name} ON;\n");
                entities.ForEach(entity => strBuilder.Append(ToSQL(entity)));
                //strBuilder.Append($"SET IDENTITY_INSERT {typeof(TEntity).Name} OFF;\n");
            }
            return strBuilder.ToString();
        }

        

        public string ToSQL(TEntity model)
        {
            string sql = "INSERT {0} ({1}) VALUES ({2});\n";
            StringBuilder fields = new StringBuilder();
            StringBuilder values = new StringBuilder();
            Type type = model.GetType();
            if (type.GetProperties() != null && type.GetProperties().Length > 0)
            {
                foreach (var propertie in type.GetProperties())
                {
                    if (!string.IsNullOrEmpty(propertie.Name)
                        && !propertie.Name.Contains("UPD_USER_ID")
                        && !propertie.Name.Contains("UPD_DATE")
                        && !propertie.Name.Contains("CREATE_USER_ID")
                        && !propertie.Name.Contains("CREATE_DATE"))
                    {
                        fields.Append(propertie.Name).Append(",");
                        var trueValue = model.GetType().GetProperty(propertie.Name).GetValue(model, null);
                        var fixValue = trueValue != null ? string.Format(FixValue(propertie), trueValue) : "NULL";
                        values.Append(fixValue).Append(",");
                    }
                }
                return string.Format(sql, typeof(TEntity).Name, fields.ToString().Trim(','), values.ToString().Trim(','));
            }
            return "";
        }

        public string FixValue(PropertyInfo pi)
        {
            var typestr = pi.PropertyType.ToString();
            if (typestr.Contains("Int32"))
            {
                return "{0}";
            }
            else if (typestr.Contains("String"))
            {
                return "N'{0}'";
            }
            else if (typestr.Contains("DateTime"))
            {
                return " CAST(N'{0}' AS DateTime)";
            }
            else
            {
                return "N'{0}'";
            }

        }

        public Tuple<List<TEntity>, int> GetPageList(Expression<Func<TEntity, int?>> order, Expression<Func<TEntity, bool>> where, int pageIndex, int pageSize)
        {
            var tlist = Db.Set<TEntity>()
               .AsNoTracking()
               .OrderBy(order)
               .Where(where).ToList();
            var klist = tlist
                .Skip(((pageIndex - 1) < 0 ? 0 : (pageIndex - 1)) * pageSize)
                .Take(pageSize)
                .ToList();
            return new Tuple<List<TEntity>, int>(klist, tlist.Count);
        }

        public Tuple<List<TEntity>, int> GetPageList(Expression<Func<TEntity, DateTime?>> order, bool isAscending, Expression<Func<TEntity, bool>> where, int pageIndex, int pageSize)
        {
            var tlist = new List<TEntity>();
            if (isAscending)
                tlist = Db.Set<TEntity>().AsNoTracking().OrderBy(order).Where(where).ToList();
            else
                tlist = Db.Set<TEntity>().AsNoTracking().OrderByDescending(order).Where(where).ToList();
            var klist = tlist
                .Skip(((pageIndex - 1) < 0 ? 0 : (pageIndex - 1)) * pageSize)
                .Take(pageSize)
                .ToList();
            return new Tuple<List<TEntity>, int>(klist, tlist.Count);
        }

        public int Count(Expression<Func<TEntity, bool>> where)
        {
            return Db.Set<TEntity>().Where(where).AsNoTracking().Count();
        }

        public List<TEntity> GetManayByOrder(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, string>> order)
        {
            return Db.Set<TEntity>().AsNoTracking().Where(where).OrderBy(order).ToList();
        }

        public TEntity GetWithTrace(Expression<Func<TEntity, bool>> where)
        {
            var entity = Db.Set<TEntity>().Where(where).FirstOrDefault();
            return entity;
        }

        public List<TEntity> GetManayWithTrace(Expression<Func<TEntity, bool>> where)
        {
            var entity = Db.Set<TEntity>().Where(where).ToList();
            return entity;
        }

        
    }
}
