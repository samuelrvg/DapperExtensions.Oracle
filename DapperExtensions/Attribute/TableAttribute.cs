using System;

namespace DapperExtensions.Oracle
{
    /// <summary>
    /// TableName：Table name，KeyName：Key，IsIdentity：Identiy
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }
        public string KeyName { get; set; }
        public bool IsIdentity { get; set; }
        public string SequenceName { get; set; }
    }
}
