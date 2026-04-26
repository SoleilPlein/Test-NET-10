using System.Globalization;
using System.Numerics;

namespace Test_NET_10.Tests;

public class OperationTests
{
    [Fact]
    public void AddOperation_ShouldWork_ForMultipleNumericTypes()
    {
        AssertOperationResult(new AddOperation<int>(2, 3), 5);
        AssertOperationResult(new AddOperation<float>(2.5f, 1.5f), 4f);
        AssertOperationResult(new AddOperation<double>(2.5, 1.5), 4d);
        AssertOperationResult(new AddOperation<decimal>(2.5m, 1.5m), 4m);
    }

    [Fact]
    public void RemoveOperation_ShouldWork_ForMultipleNumericTypes()
    {
        AssertOperationResult(new RemoveOperation<int>(7, 3), 4);
        AssertOperationResult(new RemoveOperation<float>(7.5f, 2.5f), 5f);
        AssertOperationResult(new RemoveOperation<double>(7.5, 2.5), 5d);
        AssertOperationResult(new RemoveOperation<decimal>(7.5m, 2.5m), 5m);
    }

    [Fact]
    public void MultipleOperation_ShouldWork_ForMultipleNumericTypes()
    {
        AssertOperationResult(new MultipleOperation<int>(4, 3), 12);
        AssertOperationResult(new MultipleOperation<float>(2.5f, 2f), 5f);
        AssertOperationResult(new MultipleOperation<double>(2.5, 2), 5d);
        AssertOperationResult(new MultipleOperation<decimal>(2.5m, 2m), 5m);
    }

    [Fact]
    public void DivideOperation_ShouldWork_ForMultipleNumericTypes()
    {
        AssertOperationResult(new DivideOperation<int>(8, 2), 4);
        AssertOperationResult(new DivideOperation<float>(7.5f, 2.5f), 3f);
        AssertOperationResult(new DivideOperation<double>(7.5, 2.5), 3d);
        AssertOperationResult(new DivideOperation<decimal>(7.5m, 2.5m), 3m);
    }

    [Fact]
    public void ModuloOperation_ShouldWork_ForBinaryIntegerTypes()
    {
        AssertOperationResult(new ModuloOperation<int>(10, 4), 2);
        AssertOperationResult(new ModuloOperation<long>(10, 4), 2L);
        AssertOperationResult(new ModuloOperation<BigInteger>(new BigInteger(10), new BigInteger(4)), new BigInteger(2));
    }

    [Fact]
    public void DivideOperation_ShouldThrow_ForDivideByZero_OnIntegerAndDecimalTypes()
    {
        Assert.Throws<DivideByZeroException>(() => new DivideOperation<int>(10, 0).GetResult());
        Assert.Throws<DivideByZeroException>(() => new DivideOperation<decimal>(10m, 0m).GetResult());
    }

    [Fact]
    public void ModuloOperation_ShouldThrow_ForModuloByZero_OnIntegerTypes()
    {
        Assert.Throws<DivideByZeroException>(() => new ModuloOperation<int>(10, 0).GetResult());
        Assert.Throws<DivideByZeroException>(() => new ModuloOperation<long>(10L, 0L).GetResult());
    }

    [Fact]
    public void TryParseFlexible_ShouldAccept_CommaAndDot_ForDecimalTypes()
    {
        var frCulture = CultureInfo.GetCultureInfo("fr-FR");

        bool fromComma = NumberParser.TryParseFlexible<decimal>("12,5", frCulture, out decimal commaValue);
        bool fromDot = NumberParser.TryParseFlexible<decimal>("12.5", frCulture, out decimal dotValue);

        Assert.True(fromComma);
        Assert.True(fromDot);
        Assert.Equal(12.5m, commaValue);
        Assert.Equal(12.5m, dotValue);
    }

    [Fact]
    public void TryParseFlexible_ShouldReject_DecimalInput_ForInt()
    {
        var frCulture = CultureInfo.GetCultureInfo("fr-FR");

        bool parsed = NumberParser.TryParseFlexible<int>("12.5", frCulture, out _);

        Assert.False(parsed);
    }

    [Fact]
    public void NumericOperationFactory_ShouldExpose_CoreOperations_Only()
    {
        IOperationFactory<double> factory = new NumericOperationFactory<double>();

        OperationEnum[] operations = factory.GetAvailableOperations();

        Assert.Equal(
            [
                OperationEnum.Addition,
                OperationEnum.Subtract,
                OperationEnum.Divide,
                OperationEnum.Multiple
            ],
            operations);
        Assert.DoesNotContain(OperationEnum.Modulo, operations);
    }

    [Fact]
    public void IntegerOperationFactory_ShouldExposeModulo_AndCreateModuloOperation()
    {
        IOperationFactory<int> factory = new IntegerOperationFactory<int>();

        OperationEnum[] operations = factory.GetAvailableOperations();

        Assert.Contains(OperationEnum.Modulo, operations);

        IOperation<int> moduloOperation = factory.Create(OperationEnum.Modulo, 10, 4);
        Assert.Equal(2, moduloOperation.GetResult());
    }

    private static void AssertOperationResult<T>(IOperation<T> operation, T expected)
        where T : INumber<T>
    {
        T result = operation.GetResult();
        Assert.Equal(expected, result);
    }
}
