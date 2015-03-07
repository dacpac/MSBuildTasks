using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTasks
{

    class Constants
    {
        public const string ModelXmlUri = "/model.xml";
        public const string OriginXmlUri = "/origin.xml";
        public const string PreDeployUri = "/predeploy.sql";
        public const string PostDeployUri = "/postdeploy.sql";
        public const string DacOriginXmlns = "http://schemas.microsoft.com/sqlserver/dac/Serialization/2012/02";
        public const string DacOriginRoot = "DacOrigin";
        public const string ProductSchemaElement = "ProductSchema";
        public const string ChecksumElement = "Checksum";
     }

}
