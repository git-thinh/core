using System;
using System.Collections.Generic;
using System.Text;

namespace core
{

    [Serializable]
    public enum dbStatus
    {
        CLOSED = 0,
        OPENING = 1,
        OPENED_READ = 2,
        OPENED_READ_WRITE = 3, 
        LOCKED = 4
    }

    [Serializable]
    public enum oRecordStatus
    {
        NONE = 0,
        INSERT = 1,
        UPDATE = 2,
        LOCK = 3,
        DELETE = 4
    }

    [Serializable]
    public enum oItemStatus
    {
        PENDING = 0,
        ACTIVED = 1
    }

}
