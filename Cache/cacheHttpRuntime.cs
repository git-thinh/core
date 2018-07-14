using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class cacheHttpRuntime
    {
        public static void Set(string key, object value)
        {
            System.Web.HttpRuntime.Cache.Insert(key,value); 
        }

        public static void Set(string key, object value, int minuteAbsoluteExpiration)
        {
            System.Web.HttpRuntime.Cache.Insert(
                                   key,
                                   value,
                                   null,
                                   DateTime.Now.AddMinutes(minuteAbsoluteExpiration),
                                   System.Web.Caching.Cache.NoSlidingExpiration
                               );


            //System.Web.HttpRuntime.Cache
            //and could use

            // HttpRuntime.Cache.Insert(
            //                        CacheKey,
            //                        CacheValue,
            //                        null,
            //                        DateTime.Now.AddMinutes(CacheDuration),
            //                        Cache.NoSlidingExpiration
            //                    );

            //Hashtable table1 = HttpRuntime.Cache[CacheKey] as Hashtable;
            //HttpRuntime.Cache.Remove(CacheKey);
            //You'll also have access to

            //System.Web.HttpContext.Current.Application
            //and could use

            //System.Web.HttpContext.Current.Application.Add("table1", myHashTable);
            //Hashtable table1 = System.Web.HttpContext.Current.Application["table1"] as Hashtable;
        }

        public static T Get<T>(string key)  
        {
            try
            {
                if (System.Web.HttpRuntime.Cache[key] != null)
                    return ((T)System.Web.HttpRuntime.Cache[key]);
            }
            catch { }
            return default(T);
        }


    }
}
