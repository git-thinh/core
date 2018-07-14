using Microsoft.Practices.EnterpriseLibrary.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Practices.EnterpriseLibrary
{
    public class ItemCacheRefreshAction<T> : ICacheItemRefreshAction
    {
        private Action<T> actionCacheChangeStatus;
        private T paraObject;

        public ItemCacheRefreshAction(Action<T> _actionCacheChangeStatus, T _paraObject)
        {
            actionCacheChangeStatus = _actionCacheChangeStatus;
            paraObject = _paraObject;
        }

        // Implement Referesh Method of interface ICacheItemRefreshAction
        public void Refresh(string removedKey, object expiredValue, CacheItemRemovedReason removalReason)
        {
            // Log The Information Or We can do some other stuff also
            actionCacheChangeStatus(paraObject);
        }

    }
}
