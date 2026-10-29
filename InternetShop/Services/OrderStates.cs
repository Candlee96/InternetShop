using InternetShop.Models;

namespace InternetShop.Services;

public interface IOrderState
{
    string Name { get; }
    void HandleStatus(Order order);
}

public sealed class CreatedState : IOrderState
{
    public string Name => "Создан";
    public void HandleStatus(Order order) => order.SetState(new PaidState());
}

public sealed class PaidState : IOrderState
{
    public string Name => "Оплачен";
    public void HandleStatus(Order order) => order.SetState(new ShippedState());
}

public sealed class ShippedState : IOrderState
{
    public string Name => "Отправлен";
    public void HandleStatus(Order order) => order.SetState(new DeliveredState());
}

public sealed class DeliveredState : IOrderState
{
    public string Name => "Доставлен";
    public void HandleStatus(Order order) { }
}
