using System;
using System.Collections.Generic;
using System.Text;

namespace core.Model
{

    //{"cmd":"get","selected":[1,2],"limit":50,"offset":0,"sort":[{"field":"recid","direction":"asc"}]}
    [Serializable]
    public class w2uiGridSort
    {
        public string field { set; get; }
        public string direction { set; get; }
    }

    [Serializable]
    public class w2uiGridRequest
    {
        public string cmd { set; get; }
        public int[] selected { set; get; }
        public int limit { set; get; }
        public int offset { set; get; }
        public w2uiGridSort[] sort { set; get; }
    }
}
