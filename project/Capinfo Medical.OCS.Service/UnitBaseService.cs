using Elight.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using TestingCenter.Models.UN;
using TestingCenterSystem.Models.Unit;
using TestingCenterSystem.ViewModels.Project;
using System.Data.Entity.Core.EntityClient;
using TestingCenterSystem.ViewModels;
using System.Text;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;
using EntityFramework.Extensions;

namespace TestingCenterSystem.Service
{
    public class UnitBaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {
        private UTEntities _db = null;
        public string UserId { get { return OperatorProvider.Instance.Current.UserId; } }
        public string connstr = string.Empty;
        public DateTime Now { get { return DateTime.Now; } }
        public UTEntities Db
        {
            get
            {
                if (_db == null || connstr != ProjectProvider.Instance.Current.UNIT_CONNTIONSTRING)
                {
                    _db = new UNContext(new EntityConnection(ProjectProvider.Instance.Current.UNIT_CONNTIONSTRING), false);
                    connstr = ProjectProvider.Instance.Current.UNIT_CONNTIONSTRING;
                }
                return _db;
            }
            set
            {
                _db = value;
            }
        }
        //public UnitBaseService()
        //{
        //    Db = new UTEntities();
        //}
        //public BaseService(DbContext value)
        //{
        //    Db = value;
        //}

        public bool Exists(Expression<Func<TEntity, bool>> where)
        {
            return Get(where) != null;
        }

        public TEntity Get(Expression<Func<TEntity, bool>> where)
        {
            return Db.Set<TEntity>().Where(where).AsNoTracking().FirstOrDefault();
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

        /// <summary>
        /// 实体查询
        /// </summary>
        public List<TEntity> GetSearchList(Expression<Func<TEntity, bool>> where)
        {
            return Db.Set<TEntity>().Where(where).ToList();
        }

        public string ToDeletsSql(TEntity model)
        {
            return "";
        }

        public string ToIdentitySql(Expression<Func<TEntity, bool>> @where)
        {
            throw new NotImplementedException();
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

        public void BulkInsert<T>( string tableName, IList<T> list)
        {
            try
            {
                if (Db.Database.Connection.State == ConnectionState.Closed)
                {
                    Db.Database.Connection.Open();
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
                        dynamic temp = (dynamic)item;
                        try
                        {
                            temp.UPD_USER_ID = UserId;
                            temp.UPD_DATE = Now;
                        }
                        catch { }
                        for (var i = 0; i < values.Length; i++)
                        {
                            values[i] = props[i].GetValue(temp);
                        }

                        table.Rows.Add(values);
                    }

                    bulkCopy.WriteToServer(table);
                }
                Db.Database.Connection.Close();
            }
            catch (Exception ex)
            {
                throw ex;
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
        public int ExDelete(Expression<Func<TEntity, bool>> where)
        {
            return Db.Set<TEntity>().Where(where).Delete();
        }

        public int ExDelete2(Expression<Func<TEntity, bool>> where)
        {
            var dels = Db.Set<TEntity>().AsNoTracking().Where(where)?.ToList();
            dels.ForEach(r => {
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
            dynamic temp = (dynamic)model;
            try
            {
                temp.UPD_USER_ID = UserId;
                temp.UPD_DATE = Now;
            }
            catch (Exception)
            {
            }
            Db.Entry<TEntity>(temp).State = EntityState.Modified;
            return Db.SaveChanges();
        }

        public List<TEntity> GetManay(Expression<Func<TEntity, bool>> where)
        {
            return Db.Set<TEntity>().Where(where).AsNoTracking().ToList();
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
                    if (!string.IsNullOrEmpty(propertie.Name))
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
        public Page<TEntity> GetPage(Expression<Func<TEntity, int?>> order, Expression<Func<TEntity, bool>> where, int pageIndex, int pageSize)
        {
            var tlist = Db.Set<TEntity>()
                .AsNoTracking()
                .OrderBy(order)
                .Where(where).ToList();
            var klist = tlist
                .Skip(((pageIndex - 1) < 0 ? 0 : (pageIndex - 1)) * pageSize)
                .Take(pageSize)
                .ToList();
            var result = new Page<TEntity>
            {
                CurrentPage = pageIndex,
                ItemsPerPage = pageSize,
                TotalItems = tlist.Count,
                Items = klist
            };
            return result;
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

        public List<TEntity> GetManayByOrder(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, int?>> order)
        {
            throw new NotImplementedException();
        }

        public int UpdateOrder(Expression<Func<TEntity, bool>> where, int order)
        {
            throw new NotImplementedException();
        }

        public int Modify(TEntity originalValue, TEntity newValue)
        {
            throw new NotImplementedException();
        }

        public string ToNonIdentitySql(Expression<Func<TEntity, bool>> where)
        {
            throw new NotImplementedException();
        }

        public string ToNonIdentitySqlWithWhere(Expression<Func<TEntity, bool>> where, string whereSql)
        {
            throw new NotImplementedException();
        }

        public List<TEntity> GetManayByOrder(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, string>> order)
        {
            throw new NotImplementedException();
        }


    }
}
