using System;
using System.Collections.Generic;
using System.Text;

namespace core
{
    public static class dbExtend
    {
        /// <summary>
        /// Format char first upper, type Abcde
        /// </summary>
        /// <param name="model_name"></param>
        /// <returns></returns>
        public static string model_Format(this string model_name) {
            if (string.IsNullOrEmpty(model_name)) return string.Empty;
            model_name = model_name.Trim();
            return model_name.Substring(0, 1).ToUpper() + model_name.Substring(1).ToLower();
        }

        public static string json_Format(this string json) {
            if (string.IsNullOrEmpty(json)) return string.Empty;
            return json.Replace(@",""_status"":1", @",""_status"":true").Replace(@",""_status"":0", @",""_status"":false");
        }
    }
}
