using System.Numerics;

namespace Test_NET_10;

public interface IOperationFactory<T> where T : struct, INumber<T>
{
    OperationEnum[] GetAvailableOperations();

    IOperation<T> Create(OperationEnum operation, T a, T b);
}

public class NumericOperationFactory<T> : IOperationFactory<T> where T : struct, INumber<T>
{
    private readonly Dictionary<OperationEnum, Func<T, T, IOperation<T>>> builders = new()
    {
        [OperationEnum.Addition] = static (a, b) => new AddOperation<T>(a, b),
        [OperationEnum.Subtract] = static (a, b) => new RemoveOperation<T>(a, b),
        [OperationEnum.Divide] = static (a, b) => new DivideOperation<T>(a, b),
        [OperationEnum.Multiple] = static (a, b) => new MultipleOperation<T>(a, b)
    };

    protected void Register(OperationEnum operation, Func<T, T, IOperation<T>> builder)
    {
        builders[operation] = builder;
    }

    public virtual OperationEnum[] GetAvailableOperations()
    {
        return [.. builders.Keys];
    }

    public virtual IOperation<T> Create(OperationEnum operation, T a, T b)
    {
        if (!builders.TryGetValue(operation, out var builder))
        {
            throw new InvalidOperationException($"Operation not supported for {typeof(T).Name}: {operation}");
        }

        return builder(a, b);
    }
}

public sealed class IntegerOperationFactory<T> : NumericOperationFactory<T> where T : struct, IBinaryInteger<T>
{
    public IntegerOperationFactory()
    {
        Register(OperationEnum.Modulo, static (a, b) => new ModuloOperation<T>(a, b));
    }
}
