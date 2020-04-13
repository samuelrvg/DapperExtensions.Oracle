﻿using System.Collections.Generic;

namespace DapperExtensions.Oracle
{
    public class PageEntity<T>
    {
        public IEnumerable<T> Data { get; set; }
        public long Total { get; set; }
        public dynamic OtherData { get; set; }
    }
}
