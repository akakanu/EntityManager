using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EntityManager.Annotations
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Property)]
    [ComVisible(true)]
    public class JoinColumn : Colonne
    {
        public string Reference { get; set; }
    }
}
