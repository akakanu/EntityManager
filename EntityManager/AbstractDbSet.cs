using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace EntityManager
{
    public abstract class AbstractDbSet
    {

        public static int AVG(Object property)
        {
            return 0;
        }

        public static int SUM(Object property)
        {
            return 0;
        }

        public static int MIN(Object property)
        {
            return 0;
        }

        public static int MAX(Object property)
        {
            return 0;
        }

        public static int COUNT(Object property)
        {
            return 0;
        }

        public abstract Object One(Object value);
    }
}
