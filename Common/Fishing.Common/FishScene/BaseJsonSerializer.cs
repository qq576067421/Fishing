using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GF.Common;

namespace Ps
{
    public static class BaseJsonSerializer
    {
        public static string serialize(object base_object)
        {
            //JsonSerializer serializer = new JsonSerializer();
            //StringWriter sw = new StringWriter();
            //serializer.Serialize(new JsonTextWriter(sw), base_object);
            //return sw.ToString();

            return EbTool.jsonSerialize(base_object);
        }

        public static T deserialize<T>(string json_string)
        {
            //JsonReader reader = new JsonTextReader(new StringReader(json_string));
            //JsonSerializer serializer = new JsonSerializer();
            //return serializer.Deserialize<T>(reader);

            return EbTool.jsonDeserialize<T>(json_string);
        }
    }
}
