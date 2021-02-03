using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EntityManager.Annotations
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    [ComVisible(true)]
    public class Table: Attribute
    {
        public string Name { get; set; }
        public string Schema { get; set; }
    }
}
