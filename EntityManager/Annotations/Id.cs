using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EntityManager.Annotations
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Property)]
    [ComVisible(true)]
    public class Id : Attribute
    {
        private bool auto = true;
        public bool AutoIncrement { get { return auto; } set { auto = value; } }

    }
}