namespace DependencyInjection.Lifetimes;

/// <summary>Кожна реалізація отримує унікальний Id при створенні — по ньому
/// й видно, коли контейнер створює новий екземпляр, а коли віддає наявний.</summary>
public interface IOperation
{
    Guid Id { get; }
}

public interface ITransientOperation : IOperation;
public interface IScopedOperation : IOperation;
public interface ISingletonOperation : IOperation;

public sealed class Operation : ITransientOperation, IScopedOperation, ISingletonOperation
{
    public Guid Id { get; } = Guid.NewGuid();
}

/// <summary>
/// Споживач усіх трьох сервісів. Реєструється як Scoped. Порівнюючи його Id
/// сервісів з тими, що бачить middleware у тому ж запиті, побачимо різницю
/// між Transient / Scoped / Singleton.
/// </summary>
public sealed class OperationLogger(
    ITransientOperation transient,
    IScopedOperation scoped,
    ISingletonOperation singleton)
{
    public object Snapshot(string caller) => new
    {
        caller,
        transient = transient.Id,
        scoped = scoped.Id,
        singleton = singleton.Id,
    };
}
