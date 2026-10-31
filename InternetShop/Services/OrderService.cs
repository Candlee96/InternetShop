using InternetShop.Models;

namespace InternetShop.Services;

public sealed class OrderService
{
    private int nextOrderId = 1;

    public void SetNextOrderId(int nextId) => nextOrderId = Math.Max(nextOrderId, nextId);

    public Order CreateOrder(ShoppingCart cart)
    {
        if (cart.IsEmpty) throw new InvalidOperationException("Нельзя создать заказ из пустой корзины.");
        var order = new Order(nextOrderId++, cart.CheckoutItems());
        foreach (var item in order.OrderItems) item.Product.ChangeStock(-item.Quantity);
        cart.Clear();
        return order;
    }

    public bool Pay(Order order, IPaymentStrategy paymentStrategy)
    {
        if (order.CurrentState is not CreatedState) return false;
        var paid = paymentStrategy.Pay(order.CalculateTotal());
        if (paid) order.UpdateStatus();
        return paid;
    }
}
