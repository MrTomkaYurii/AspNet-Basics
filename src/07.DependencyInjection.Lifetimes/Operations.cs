namespace DependencyInjection.Lifetimes;

/// <summary>
/// Кожен екземпляр отримує унікальний Id при створенні. Порівнюючи Id, видно,
/// коли контейнер створює новий об'єкт, а коли віддає наявний.
/// </summary>
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
