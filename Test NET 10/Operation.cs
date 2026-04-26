using System.Numerics;

namespace Test_NET_10;

public interface IOperation<T>
{
    T GetResult();
}

public abstract class Operation<T>(T a, T b) : IOperation<T> where T : INumber<T>
{
    protected T a = a;
    protected T b = b;

    public abstract T GetResult();
}

public sealed class AddOperation<T>(T a, T b) : Operation<T>(a, b) where T : INumber<T>
{
    public override T GetResult()
    {
        return a + b;
    }
}

public sealed class RemoveOperation<T>(T a, T b) : Operation<T>(a, b) where T : INumber<T>
{
    public override T GetResult()
    {
        return a - b;
    }
}

public sealed class DivideOperation<T>(T a, T b) : Operation<T>(a, b) where T : INumber<T>
{
    public override T GetResult()
    {
        return a / b;
    }
}

public sealed class MultipleOperation<T>(T a, T b) : Operation<T>(a, b) where T : INumber<T>
{
    public override T GetResult()
    {
        return a * b;
    }
}

public sealed class ModuloOperation<T>(T a, T b) : Operation<T>(a, b) where T : IBinaryInteger<T>
{
    public override T GetResult()
    {
        return a % b;
    }
}