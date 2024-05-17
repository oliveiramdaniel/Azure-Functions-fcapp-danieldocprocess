using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Function
{
    public class DocEntity
    {
        public string PersonName { get; set; }
        public string PersonGovId { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}
