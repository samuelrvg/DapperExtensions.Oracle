﻿using System.Collections.Generic;
using System.Linq;

using Dapper;
using System.Data;


namespace DapperExtensions.Oracle
{
    public static partial class DapperExtension
    {

        #region common method for ado.net

        public static DataTable GetDataTable(this IDbConnection conn, string sql, object param = null, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            using (IDataReader reader = conn.ExecuteReader(sql, param, tran, commandTimeout, commandType))
            {
                DataTable dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
        }

        public static DataTable GetSchemaTable<T>(this IDbConnection conn, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return GetDataTable(conn, builder.GetSchemaTableSql<T>(returnFields), null, tran, commandTimeout);
        }

        #endregion

        #region method (Insert Update Delete)

        public static int Insert<T>(this IDbConnection conn, T model, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Execute(builder.GetInsertSql<T>(), model, tran, commandTimeout);
        }

        /// <summary>
        /// sqlserver、mysql、sqlite、postgresql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="model"></param>
        /// <param name="tran"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static dynamic InsertReturnId<T>(this IDbConnection conn, T model, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.ExecuteScalar<dynamic>(builder.GetInsertReturnIdSql<T>(), model, tran, commandTimeout);
        }

        /// <summary>
        /// only oracle use
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="model"></param>
        /// <param name="sequence"></param>
        /// <param name="tran"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static decimal InsertReturnIdForOracle<T>(this IDbConnection conn, T model, string sequence, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            conn.Execute(builder.GetInsertReturnIdSql<T>(sequence), model, tran, commandTimeout);
            return GetSequenceCurrent<decimal>(conn, sequence, tran, null);
        }


        /// <summary>
        /// for sqlserver insert identity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static int InsertIdentity<T>(this IDbConnection conn, T model, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Execute(builder.GetInsertIdentitySql<T>(), model, tran, commandTimeout);
        }

        public static int Update<T>(this IDbConnection conn, T model, string updateFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Execute(builder.GetUpdateSql<T>(updateFields), model, tran, commandTimeout);
        }

        public static int UpdateByWhere<T>(this IDbConnection conn, T model, string where, string updateFields, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Execute(builder.GetUpdateByWhereSql<T>(where, updateFields), model, tran, commandTimeout);
        }

        public static int InsertOrUpdate<T>(this IDbConnection conn, T model, string updateFields = null, bool update = true, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            int effectRow = 0;
            dynamic total = conn.ExecuteScalar<dynamic>(builder.GetExistsKeySql<T>(), model, tran, commandTimeout);
            if (total > 0)
            {
                if (update)
                {
                    effectRow += Update(conn, model, updateFields, tran, commandTimeout);
                }
            }
            else
            {
                effectRow += Insert(conn, model, tran, commandTimeout);
            }

            return effectRow;
        }

        /// <summary>
        /// for sqlserver insert identity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="updateFields"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public static int InsertIdentityOrUpdate<T>(this IDbConnection conn, T model, string updateFields = null, bool update = true, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            int effectRow = 0;
            dynamic total = conn.ExecuteScalar<dynamic>(builder.GetExistsKeySql<T>(), model, tran, commandTimeout);
            if (total > 0)
            {
                if (update)
                {
                    effectRow += Update(conn, model, updateFields, tran, commandTimeout);
                }
            }
            else
            {
                effectRow += InsertIdentity(conn, model, tran, commandTimeout);
            }

            return effectRow;
        }

        public static int Delete<T>(this IDbConnection conn, object id, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Execute(builder.GetDeleteByIdSql<T>(), new { id = id }, tran, commandTimeout);
        }

        public static int DeleteByIds<T>(this IDbConnection conn, object ids, IDbTransaction tran = null, int? commandTimeout = null)
        {
            if (CommonUtil.ObjectIsEmpty(ids))
                return 0;
            var builder = BuilderFactory.GetBuilder(conn);
            DynamicParameters dpar = new DynamicParameters();
            dpar.Add("@ids", ids);
            return conn.Execute(builder.GetDeleteByIdsSql<T>(), dpar, tran, commandTimeout);
        }

        public static int DeleteByWhere<T>(this IDbConnection conn, string where, object param, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Execute(builder.GetDeleteByWhereSql<T>(where), param, tran, commandTimeout);
        }

        public static int DeleteAll<T>(this IDbConnection conn, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Execute(builder.GetDeleteAllSql<T>(), null, tran, commandTimeout);

        }

        #endregion

        #region method (Query)

        public static IdType GetIdentity<IdType>(this IDbConnection conn, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.ExecuteScalar<IdType>(builder.GetIdentitySql(), null, tran, commandTimeout);
        }

        public static IdType GetSequenceCurrent<IdType>(this IDbConnection conn, string sequence, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.ExecuteScalar<IdType>(builder.GetSequenceCurrentSql(sequence), null, tran, commandTimeout);
        }

        public static IdType GetSequenceNext<IdType>(this IDbConnection conn, string sequence, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.ExecuteScalar<IdType>(builder.GetSequenceNextSql(sequence), null, tran, commandTimeout);
        }

        public static long GetTotal<T>(this IDbConnection conn, string where = null, object param = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.ExecuteScalar<long>(builder.GetTotalSql<T>(where), param, tran, commandTimeout);
        }

        public static IEnumerable<T> GetAll<T>(this IDbConnection conn, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Query<T>(builder.GetAllSql<T>(returnFields, orderBy), null, tran, true, commandTimeout);
        }

        public static IEnumerable<dynamic> GetAllDynamic<T>(this IDbConnection conn, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Query(builder.GetAllSql<T>(returnFields, orderBy), null, tran, true, commandTimeout);
        }

        public static T GetById<T>(this IDbConnection conn, object id, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.QueryFirstOrDefault<T>(builder.GetByIdSql<T>(returnFields), new { id = id }, tran, commandTimeout);
        }

        public static dynamic GetByIdDynamic<T>(this IDbConnection conn, object id, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.QueryFirstOrDefault(builder.GetByIdSql<T>(returnFields), new { id = id }, tran, commandTimeout);
        }

        public static IEnumerable<T> GetByIds<T>(this IDbConnection conn, object ids, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            if (CommonUtil.ObjectIsEmpty(ids))
                return Enumerable.Empty<T>();
            var builder = BuilderFactory.GetBuilder(conn);
            DynamicParameters dpar = new DynamicParameters();
            dpar.Add("@ids", ids);
            return conn.Query<T>(builder.GetByIdsSql<T>(returnFields), dpar, tran, true, commandTimeout);
        }

        public static IEnumerable<dynamic> GetByIdsDynamic<T>(this IDbConnection conn, object ids, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            if (CommonUtil.ObjectIsEmpty(ids))
                return Enumerable.Empty<dynamic>();
            var builder = BuilderFactory.GetBuilder(conn);
            DynamicParameters dpar = new DynamicParameters();
            dpar.Add("@ids", ids);
            return conn.Query(builder.GetByIdsSql<T>(returnFields), dpar, tran, true, commandTimeout);
        }

        public static IEnumerable<T> GetByIdsWithField<T>(this IDbConnection conn, object ids, string field, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            if (CommonUtil.ObjectIsEmpty(ids))
                return Enumerable.Empty<T>();
            var builder = BuilderFactory.GetBuilder(conn);
            DynamicParameters dpar = new DynamicParameters();
            dpar.Add("@ids", ids);
            return conn.Query<T>(builder.GetByIdsWithFieldSql<T>(field, returnFields), dpar, tran, true, commandTimeout);
        }

        public static IEnumerable<dynamic> GetByIdsWithFieldDynamic<T>(this IDbConnection conn, object ids, string field, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            if (CommonUtil.ObjectIsEmpty(ids))
                return Enumerable.Empty<dynamic>();
            var builder = BuilderFactory.GetBuilder(conn);
            DynamicParameters dpar = new DynamicParameters();
            dpar.Add("@ids", ids);
            return conn.Query(builder.GetByIdsWithFieldSql<T>(field, returnFields), dpar, tran, true, commandTimeout);
        }

        public static IEnumerable<T> GetByWhere<T>(this IDbConnection conn, string where, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Query<T>(builder.GetByWhereSql<T>(where, returnFields, orderBy), param, tran, true, commandTimeout);
        }

        public static IEnumerable<dynamic> GetByWhereDynamic<T>(this IDbConnection conn, string where, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Query(builder.GetByWhereSql<T>(where, returnFields, orderBy), param, tran, true, commandTimeout);
        }

        public static T GetByWhereFirst<T>(this IDbConnection conn, string where, object param = null, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.QueryFirstOrDefault<T>(builder.GetByWhereFirstSql<T>(where, returnFields), param, tran, commandTimeout);
        }

        public static dynamic GetByWhereFirstDynamic<T>(this IDbConnection conn, string where, object param = null, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.QueryFirstOrDefault(builder.GetByWhereFirstSql<T>(where, returnFields), param, tran, commandTimeout);
        }

        public static IEnumerable<T> GetBySkipTake<T>(this IDbConnection conn, int skip, int take, string where = null, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Query<T>(builder.GetBySkipTakeSql<T>(skip, take, where, returnFields, orderBy), param, tran, true, commandTimeout);
        }

        public static IEnumerable<dynamic> GetBySkipTakeDynamic<T>(this IDbConnection conn, int skip, int take, string where = null, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Query(builder.GetBySkipTakeSql<T>(skip, take, where, returnFields, orderBy), param, tran, true, commandTimeout);
        }

        public static IEnumerable<T> GetByPageIndex<T>(this IDbConnection conn, int pageIndex, int pageSize, string where = null, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Query<T>(builder.GetByPageIndexSql<T>(pageIndex, pageSize, where, returnFields, orderBy), param, tran, true, commandTimeout);
        }

        public static IEnumerable<dynamic> GetByPageIndexDynamic<T>(this IDbConnection conn, int pageIndex, int pageSize, string where = null, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return conn.Query(builder.GetByPageIndexSql<T>(pageIndex, pageSize, where, returnFields, orderBy), param, tran, true, commandTimeout);
        }

        public static PageEntity<T> GetPage<T>(this IDbConnection conn, int pageIndex, int pageSize, string where = null, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            PageEntity<T> pageEntity = new PageEntity<T>();
            using (var reader = conn.QueryMultiple(builder.GetPageSql<T>(pageIndex, pageSize, where, returnFields, orderBy), param, tran, commandTimeout))
            {
                pageEntity.Total = reader.ReadFirstOrDefault<long>();
                if (pageEntity.Total > 0)
                    pageEntity.Data = reader.Read<T>();
                else
                    pageEntity.Data = Enumerable.Empty<T>();
            }
            return pageEntity;
        }

        public static PageEntity<dynamic> GetPageDynamic<T>(this IDbConnection conn, int pageIndex, int pageSize, string where = null, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            PageEntity<dynamic> pageEntity = new PageEntity<dynamic>();
            using (var reader = conn.QueryMultiple(builder.GetPageSql<T>(pageIndex, pageSize, where, returnFields, orderBy), param, tran, commandTimeout))
            {
                pageEntity.Total = reader.ReadFirstOrDefault<long>();
                if (pageEntity.Total > 0)
                    pageEntity.Data = reader.Read<dynamic>();
                else
                    pageEntity.Data = Enumerable.Empty<dynamic>();
            }
            return pageEntity;
        }

        public static PageEntity<T> GetPageForOracle<T>(this IDbConnection conn, int pageIndex, int pageSize, string where = null, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            PageEntity<T> pageEntity = new PageEntity<T>();
            pageEntity.Total = GetTotal<T>(conn, where, param, tran, commandTimeout);
            if (pageEntity.Total > 0)
                pageEntity.Data = GetByPageIndex<T>(conn, pageIndex, pageSize, where, param, returnFields, orderBy, tran, commandTimeout);
            else
                pageEntity.Data = Enumerable.Empty<T>();
            return pageEntity;
        }

        public static PageEntity<dynamic> GetPageForOracleDynamic<T>(this IDbConnection conn, int pageIndex, int pageSize, string where = null, object param = null, string returnFields = null, string orderBy = null, IDbTransaction tran = null, int? commandTimeout = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            PageEntity<dynamic> pageEntity = new PageEntity<dynamic>();
            pageEntity.Total = GetTotal<T>(conn, where, param, tran, commandTimeout);
            if (pageEntity.Total > 0)
                pageEntity.Data = GetByPageIndexDynamic<T>(conn, pageIndex, pageSize, where, param, returnFields, orderBy, tran, commandTimeout);
            else
                pageEntity.Data = Enumerable.Empty<dynamic>();
            return pageEntity;
        }


        #endregion
    }
}
