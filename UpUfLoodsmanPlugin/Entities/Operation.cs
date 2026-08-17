using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpUfLoodsmanPlugin.Entities
{
    public class Operation
    {
        public Operation(string name, OperationStatus operationStatus)
        {
            Name = name;
            OperationStatus = operationStatus;
        }
        public string Name { get; set; }
        public OperationStatus OperationStatus { get; set; }
    }
}
