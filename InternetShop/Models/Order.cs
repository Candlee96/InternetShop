using InternetShop.Services;

namespace InternetShop.Models;

public sealed class Order
{
    private readonly List<OrderItem> orderItems;
    public int OrderId { get; }
    public DateTime CreatedAt { get; }
    public string? TrackingNumber { get; private set; }
    public IOrderState CurrentState { get; private set; }
    public IReadOnlyList<OrderItem> OrderItems => orderItems;

    public Order(int orderId, IEnumerable<OrderItem> items)
    {
        orderItems = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        if (orderItems.Count == 0) throw new ArgumentException("Order must contain items.", nameof(items));
        OrderId = orderId;
        CreatedAt = DateTime.UtcNow;
        CurrentState = new CreatedState();
    }

    public decimal CalculateTotal() => orderItems.Sum(item => item.GetSubtotal());
    public void AssignTrackingNumber(string trackingNumber) => TrackingNumber = trackingNumber;
    public void UpdateStatus() => CurrentState.HandleStatus(this);
    public void SetState(IOrderState state) => CurrentState = state;
    public bool CheckPaymentTimeout(TimeSpan timeout) => DateTime.UtcNow - CreatedAt > timeout && CurrentState is CreatedState;
}
