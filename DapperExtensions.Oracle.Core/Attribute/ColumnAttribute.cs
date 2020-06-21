using System;

namespace DapperExtensions.Oracle.Core
{
    /// <summary>
    /// Table column map
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
