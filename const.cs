using core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace core
{
    public class _const
    {
        public const Char db_charSplitColumn = '^';
        public const int msg_ID_LEN = 25;
        public const int session_ID_LEN = 22; //1900111703550539613206
        public const string viewID = "ViewID";
        public const string user_nologin = "nologin";
         
    }

    public class Switch
    {
        public Switch(Object o)
        {
            Object = o;
        }

        public Switch(string className )
        {
            Object = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance("core.Model." + className);
        }

        public Object Object { get; private set; } 
    }


    /// <summary>
    /// Extensions, because otherwise casing fails on Switch==null
    /// </summary>
    public static class SwitchExtensions
    {
        public static Switch Case<T>(this Switch s, Action<T> a)
              where T : class
        {
            return Case(s, o => true, a, false);
        }

        public static Switch Case<T>(this Switch s, Action<T> a,
             bool fallThrough) where T : class
        {
            return Case(s, o => true, a, fallThrough);
        }

        public static Switch Case<T>(this Switch s,
            Func<T, bool> c, Action<T> a) where T : class
        {
            return Case(s, c, a, false);
        }

        public static Switch Case<T>(this Switch s,
            Func<T, bool> c, Action<T> a, bool fallThrough) where T : class
        {
            if (s == null)
            {
                return null;
            }

            T t = s.Object as T;
            if (t != null)
            {
                if (c(t))
                {
                    a(t);
                    return fallThrough ? s : null;
                }
            }

            return s;
        }
    }
}
