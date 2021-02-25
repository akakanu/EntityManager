using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static EntityManager.SGBD.Sgbd;

namespace EntityManager.Annotations
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Property)]
    [ComVisible(true)]
    public class OneToOne : Attribute
    {
        public string  MappedBy { get; set; }

        public FetchType Fetch { get; set; }
    }
}
