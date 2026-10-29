namespace InternetShop.Models;

public abstract class AUser
{
    private string? password;

    public int Id { get; }
    public string Email { get; }
    public bool IsBlocked { get; private set; }

    protected AUser(int id, string email)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        Id = id;
        Email = email.Trim().ToLowerInvariant();
    }

    public bool Register(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword) || rawPassword.Length < 6) return false;
        password = rawPassword;
        return true;
    }

    public bool Login(string rawPassword) => !IsBlocked && password == rawPassword;

    public void Block() => IsBlocked = true;
    public void Unblock() => IsBlocked = false;
}

public sealed class Customer : AUser
{
    public string Name { get; }
    public int BonusPoints { get; private set; }
    public ShoppingCart Cart { get; }

    public Customer(int id, string name, string email) : base(id, email)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        Name = name.Trim();
        Cart = new ShoppingCart();
    }

    public void AddBonusPoints(int points) => BonusPoints += Math.Max(0, points);
}

public sealed class Manager : AUser
{
    public int EmployeeId { get; }
    public Manager(int id, int employeeId, string email) : base(id, email) => EmployeeId = employeeId;
}

public sealed class Administrator : AUser
{
    public int AdminLevel { get; }
    public Administrator(int id, int adminLevel, string email) : base(id, email) => AdminLevel = adminLevel;

    public void BlockUser(AUser user) => user.Block();
    public void UnblockUser(AUser user) => user.Unblock();
}
