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
    public int CustomerId { get; internal set; }

    public Order(int orderId, IEnumerable<OrderItem> items) : this(orderId, items, DateTime.UtcNow) { }

    public Order(int orderId, IEnumerable<OrderItem> items, DateTime createdAt)
    {
        if (orderId <= 0) throw new ArgumentOutOfRangeException(nameof(orderId));
        orderItems = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        OrderId = orderId;
        CreatedAt = createdAt;
        CurrentState = new CreatedState();
    }

    internal void AddPersistedItem(OrderItem item) => orderItems.Add(item);
    internal void SetPersistedStatus(string status, string tracking)
    {
        CurrentState = status switch { "Оплачен" => new PaidState(), "Отправлен" => new ShippedState(), "Доставлен" => new DeliveredState(), _ => new CreatedState() };
        TrackingNumber = string.IsNullOrWhiteSpace(tracking) ? null : tracking;
    }

    public decimal CalculateTotal() => orderItems.Sum(item => item.GetSubtotal());
    public void AssignTrackingNumber(string trackingNumber) => TrackingNumber = trackingNumber;
    public void UpdateStatus() => CurrentState.HandleStatus(this);
    public void SetState(IOrderState state) => CurrentState = state;
    public bool CheckPaymentTimeout(TimeSpan timeout) => DateTime.UtcNow - CreatedAt > timeout && CurrentState is CreatedState;
}
