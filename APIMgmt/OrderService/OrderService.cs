using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OrderService
{
    public class OrderService : IOrderService
    {
        public OrderResponse GetOrder(OrderRequest orderRequest)
        {
            if (orderRequest.OrderID < 0)
                throw new FaultException<string>("OrderID is invalid");

            return new OrderResponse
            {
                Order = new Order()
                {
                    OrderID = orderRequest.OrderID,
                    Cancelled = false,
                    Products = new List<Product>()
                 {
                     new Product() { ProductID=1, Discontinued=false, ProductName="Product A", QuantityPerUnit=1, UnitPrice=3.0M },
                     new Product() { ProductID=2, Discontinued=false, ProductName="Product B", QuantityPerUnit=1, UnitPrice=1.5M }
                 }
                }
            };
        }

        public Order GetOrderSimple(int orderId)
        {
            if (orderId < 0)
                throw new FaultException<string>("OrderID is invalid");

            return new Order()
            {
                OrderID = orderId,
                Cancelled = false,
                Products = new List<Product>()
                 {
                     new Product() { ProductID=1, Discontinued=false, ProductName="Product A", QuantityPerUnit=1, UnitPrice=3.0M },
                     new Product() { ProductID=2, Discontinued=false, ProductName="Product B", QuantityPerUnit=1, UnitPrice=1.5M }
                 }
            };
        }
    }
}