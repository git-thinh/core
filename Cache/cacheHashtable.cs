using Microsoft.Practices.EnterpriseLibrary;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class cacheHashtable
    {
        static CacheManager cacheManager = null;
        static cacheHashtable()
        {
            cacheManager = CacheFactory.GetCacheManager();
        }

        public static void Set(string key, object value)
        {
            cacheManager.Add(key, value);
        }
        public static void Remove(string key )
        {
            cacheManager.Remove(key);
        }

        public static void Set(string key, object value, int minuteAbsoluteExpiration)
        {
            AbsoluteTime _AbsoulteTime = new AbsoluteTime(TimeSpan.FromMinutes(minuteAbsoluteExpiration));
            cacheManager.Add(key, value, CacheItemPriority.Normal, null, _AbsoulteTime);
        }

        public static void Set(string key, object value, int minuteAbsoluteExpiration, string cachePath, string fileName)
        {
            //Creating Absolute Time Expiration
            AbsoluteTime _AbsoulteTime = new AbsoluteTime(TimeSpan.FromMinutes(minuteAbsoluteExpiration));
            //Creating FileDependecy Object
            FileDependency _objFileDependency = new FileDependency(fileName); //"MyFile.XML"
            // Using ICacheItemExpiration To Set multiple Cache Expiration policy
            cacheManager.Add(key, value, CacheItemPriority.Normal, null, new ICacheItemExpiration[] { _AbsoulteTime, _objFileDependency });
        }

        public static void Set<T>(string key, object value, 
            int minuteAbsoluteExpiration, 
            Action<T> functionCacheChangeStatus, T paraAction)
        {
            AbsoluteTime _AbsoulteTime = new AbsoluteTime(TimeSpan.FromMinutes(minuteAbsoluteExpiration));
            cacheManager.Add(key, value, CacheItemPriority.Normal, 
                new ItemCacheRefreshAction<T>(functionCacheChangeStatus, paraAction), _AbsoulteTime);
        }

        public static bool Exist(string key) {
            return cacheManager.Contains(key);
        }

        public static T Get<T>(string key)
        {
            try
            { 
                if (cacheManager.Contains(key))
                    return ((T)cacheManager.GetData(key));
            }
            catch { }
            return default(T);
        }
    }
}

//ThinhNV//???????????????
//ReadMe-----------------------------
/*
App.config

<configuration>
  <configSections>
    <section name="dataConfiguration"    type="Microsoft.Practices.EnterpriseLibrary.Data.Configuration.DatabaseSettings, core"/>
    <section name="cachingConfiguration" type="Microsoft.Practices.EnterpriseLibrary.Caching.Configuration.CacheManagerSettings, core"/>
  </configSections>

  <cachingConfiguration defaultCacheManager="Default Cache Manager" >
    <backingStores>
      <add name="inMemory" type="Microsoft.Practices.EnterpriseLibrary.Caching.BackingStoreImplementations.NullBackingStore, core" />
    </backingStores>
    <cacheManagers>
      <add name="Default Cache Manager" expirationPollFrequencyInSeconds="60" maximumElementsInCacheBeforeScavenging="1000" numberToRemoveWhenScavenging="10" backingStoreName="inMemory" />
    </cacheManagers>
  </cachingConfiguration>
  
</configuration>











<?xml version="1.0" encoding="utf-8" ?>
<configuration>

  <configSections>
    <section name="dataConfiguration"    type="Microsoft.Practices.EnterpriseLibrary.Data.Configuration.DatabaseSettings, Microsoft.Practices.EnterpriseLibrary.Data"/>
    <section name="cachingConfiguration" type="Microsoft.Practices.EnterpriseLibrary.Caching.Configuration.CacheManagerSettings, Microsoft.Practices.EnterpriseLibrary.Caching"/>
  </configSections>
  
  <cachingConfiguration defaultCacheManager="Default Cache Manager" >
    <backingStores>
      <add name="inMemory" type="Microsoft.Practices.EnterpriseLibrary.Caching.BackingStoreImplementations.NullBackingStore, Microsoft.Practices.EnterpriseLibrary.Caching" />
    </backingStores>
    <cacheManagers>
      <add name="Default Cache Manager" expirationPollFrequencyInSeconds="60" maximumElementsInCacheBeforeScavenging="1000" numberToRemoveWhenScavenging="10" backingStoreName="inMemory" />
    </cacheManagers>
  </cachingConfiguration>  

</configuration>

*/
