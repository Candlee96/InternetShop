namespace InternetShop.Services;

public interface IPaymentStrategy
{
    bool Pay(decimal amount);
}

public sealed class CardPayment : IPaymentStrategy
{
    public bool Pay(decimal amount) => amount > 0;
}

public sealed class BonusPayment : IPaymentStrategy
{
    private readonly Func<decimal, bool> balanceChecker;
    public BonusPayment(Func<decimal, bool> balanceChecker) => this.balanceChecker = balanceChecker;
    public bool Pay(decimal amount) => amount > 0 && balanceChecker(amount);
}
