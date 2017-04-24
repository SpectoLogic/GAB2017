using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace OrderService
{
    [ServiceContract]
    public interface IOrderService
    {
        [OperationContract]
        [FaultContract(typeof(string))]
        OrderResponse GetOrder(OrderRequest orderRequest);

        [OperationContract]
        [FaultContract(typeof(string))]
        Order GetOrderSimple(int orderId);
    }
}