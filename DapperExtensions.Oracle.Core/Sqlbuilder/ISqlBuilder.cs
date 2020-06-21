﻿
namespace DapperExtensions.Oracle.Core
{
    internal interface ISqlBuilder
    {

        #region method (Insert Update Delete)

        string GetSchemaTableSql<T>(string returnFields);

        string GetInsertSql<T>();

        string GetInsertReturnIdSql<T>(string sequence = null);

        string GetUpdateSql<T>(string updateFields);

        string GetUpdateByWhereSql<T>(string where, string updateFields);

        string GetExistsKeySql<T>();

        string GetDeleteByIdSql<T>();

        string GetDeleteByIdsSql<T>();

        string GetDeleteByWhereSql<T>(string where);

        string GetDeleteAllSql<T>();

        #endregion


        #region method (Query)

        string GetSequenceCurrentSql(string sequence);

        string GetSequenceNextSql(string sequence);

        string GetTotalSql<T>(string where);

        string GetTotalQuerySql<T>(string query);

        string GetAllSql<T>(string returnFields, string orderBy);

        string GetByIdSql<T>(string returnFields);

        string GetByIdsSql<T>(string returnFields);

        string GetByIdsWithFieldSql<T>(string field, string returnFields);

        string GetByWhereSql<T>(string where, string returnFields, string orderBy);

        string GetByWhereFirstSql<T>(string where, string returnFields);

        string GetBySkipTakeSql<T>(int skip, int take, string where, string returnFields, string orderBy);

        string GetByPageIndexSql<T>(int pageIndex, int pageSize, string where, string returnFields, string orderBy);

        string GetByPageIndexSql<T>(string query, int pageIndex, int pageSize);

        #endregion


    }
}
