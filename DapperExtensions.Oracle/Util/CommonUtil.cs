using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperExtensions.Oracle
{
    internal class CommonUtil
    {
        public static string GetFieldsStr(IEnumerable<string> fieldList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in fieldList)
            {
                sb.AppendFormat("{0}", item);

                if (item != fieldList.Last())
                {
                    sb.Append(",");
                }
            }

            return sb.ToString();
        }

        public static string GetFieldsAtStr(IEnumerable<string> fieldList, string symbol = "@")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in fieldList)
            {
                sb.AppendFormat("{0}{1}", symbol, item);

                if (item != fieldList.Last())
                {
                    sb.Append(",");
                }
            }
            return sb.ToString();
        }

        public static string GetFieldsEqStr(IEnumerable<string> fieldList, string symbol = "@")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in fieldList)
            {
                sb.AppendFormat("{1}={3}{1}", item, symbol);

                if (item != fieldList.Last())
                {
                    sb.Append(",");
                }
            }
            return sb.ToString();
        }

        public static IEnumerable GetMultiExec(object param)
        {
            return (param is IEnumerable && !(param is string || param is IEnumerable<KeyValuePair<string, object>>)) ? (IEnumerable)param : null;
        }

        public static bool ObjectIsEmpty(object param)
        {
            bool result = true;
            IEnumerable data = GetMultiExec(param);
            if (data != null)
            {
                foreach (var item in data)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        public static TableEntity CreateTableEntity(Type t)
        {
            TableAttribute table = t.GetCustomAttributes(false).FirstOrDefault(f => f is TableAttribute) as TableAttribute;
            if (table == null)
            {
                throw new Exception("Class " + t.Name + " is not labeled [TableAttribute], please label it first");
            }
            else
            {
                TableEntity model = new TableEntity();
                model.TableName = table.TableName;
                model.KeyName = table.KeyName;
                model.IsIdentity = table.IsIdentity;
                model.SequenceName = table.SequenceName;
                model.AllFieldList = new List<string>();
                model.ExceptKeyFieldList = new List<string>();


                var allproperties = t.GetProperties();

                foreach (var item in allproperties)
                {
                    var igore = item.GetCustomAttributes(false).FirstOrDefault(f => f is IgoreAttribute) as IgoreAttribute;
                    if (igore == null)
                    {
                        string Name = item.Name;
                        var column = item.GetCustomAttributes(false).FirstOrDefault(f => f is ColumnAttribute) as ColumnAttribute;
                        if (column != null)
                        {
                            Name = column.Name;
                            model.AllFieldList.Add(Name);
                        }
                        else
                        {
                            model.AllFieldList.Add(Name.ToUpper());
                        }

                        if (Name.ToLower().Equals(model.KeyName.ToLower()))
                            model.KeyType = item.PropertyType;
                        else
                            model.ExceptKeyFieldList.Add(Name);
                    }
                }

                return model;
            }

        }

        public static void InitTableForOracle(TableEntity table)
        {
            string Fields = GetFieldsStr(table.AllFieldList);
            string FieldsAt = GetFieldsAtStr(table.AllFieldList, ":");
            string FieldsEq = GetFieldsEqStr(table.AllFieldList, ":");

            string FieldsExtKey = GetFieldsStr(table.ExceptKeyFieldList);
            string FieldsAtExtKey = GetFieldsAtStr(table.ExceptKeyFieldList, ":");
            string FieldsEqExtKey = GetFieldsEqStr(table.ExceptKeyFieldList, ":");

            table.AllFields = Fields;
            table.AllFieldsAt = FieldsAt;
            table.AllFieldsAtEq = FieldsEq;

            table.AllFieldsExceptKey = FieldsExtKey;
            table.AllFieldsAtExceptKey = FieldsAtExtKey;
            table.AllFieldsAtEqExceptKey = FieldsEqExtKey;

            //table.InsertSql = string.Format("INSERT INTO {0}({1})VALUES({2})", table.TableName, Fields, FieldsAt);

            if (!string.IsNullOrEmpty(table.KeyName))
            {
                table.InsertReturnIdSql = string.Format("INSERT INTO {0}({1})VALUES(```seq```.NEXTVAL,{2})", table.TableName, Fields, FieldsAtExtKey);
                if (!string.IsNullOrEmpty(table.SequenceName))
                {
                    table.InsertSql = string.Format("INSERT INTO {0}({1})VALUES({2}.NEXTVAL, {3})", table.TableName, Fields, table.SequenceName, FieldsAtExtKey);
                }
                if (table.IsIdentity)
                {
                    table.InsertSql = string.Format("INSERT INTO {0}({1})VALUES({2})", table.TableName, FieldsExtKey, FieldsAtExtKey);
                }

                table.DeleteByIdSql = string.Format("DELETE FROM {0} WHERE {1}=:id", table.TableName, table.KeyName);
                table.DeleteByIdsSql = string.Format("DELETE FROM {0} WHERE {1} IN :ids", table.TableName, table.KeyName);
                table.GetByIdSql = string.Format("SELECT {0} FROM {1} WHERE {2}=:id", Fields, table.TableName, table.KeyName);
                table.GetByIdsSql = string.Format("SELECT {0} FROM {1} WHERE {2} IN :ids", Fields, table.TableName, table.KeyName);
                table.UpdateSql = string.Format("UPDATE {0} SET {1} WHERE {2}=:{2}", table.TableName, FieldsEqExtKey, table.KeyName);
            }

            table.DeleteAllSql = string.Format("DELETE FROM {0} ", table.TableName);
            table.GetAllSql = string.Format("SELECT {0} FROM {1} ", Fields, table.TableName);
        }

        public static void CheckTableKey(TableEntity table)
        {
            if (string.IsNullOrEmpty(table.KeyName))
            {
                string msg = "table [" + table.TableName + "] has no primary key";
                throw new Exception(msg);
            }

        }

        public static string CreateUpdateSql(TableEntity table, string updateFields, string symbol = "@")
        {
            string updateList = GetFieldsEqStr(updateFields.Split(',').ToList(), symbol);
            return string.Format("UPDATE {0} SET {1} WHERE {2}={3}{2}", table.TableName, updateList, table.KeyName, symbol);
        }

        public static string CreateUpdateByWhereSql(TableEntity table, string where, string updateFields, string symbol = "@")
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("UPDATE {0} SET ", table.TableName);
            if (string.IsNullOrEmpty(updateFields))
            {
                if (!string.IsNullOrEmpty(table.KeyName))
                    sb.AppendFormat(table.AllFieldsAtEqExceptKey);
                else
                    sb.AppendFormat(table.AllFieldsAtEq);
            }
            else
            {
                string updateList = GetFieldsEqStr(updateFields.Split(',').ToList(), symbol);
                sb.Append(updateList);
            }
            sb.Append(" ");
            sb.Append(where);

            return sb.ToString();
        }

    }
}
