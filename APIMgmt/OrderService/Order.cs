using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OrderService
{
    [MessageContract]
    public class OrderRequest
    {
        [MessageHeader] // This cannot be handled yet by Azure API Management SOAP2REST
        public int OrderID { get; set; }
    }

    [MessageContract]
    public class OrderResponse
    {
        [MessageBodyMember]
        public Order Order { get; set; }
    }

    [DataContract]
    public class Order
    {
        [DataMember]
        public int OrderID { get; set; }
        [DataMember]
        public List<Product> Products { get; set; }
        [DataMember]
        public bool Cancelled { get; set; }
    }
}
