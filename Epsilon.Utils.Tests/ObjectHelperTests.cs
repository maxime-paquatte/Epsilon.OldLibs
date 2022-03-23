using System.Globalization;
using Xunit;

namespace Epsilon.Utils.Tests;

public class ObjectHelperTests
{
    private readonly CultureInfo _fr = new CultureInfo("fr-FR");
    private readonly CultureInfo _en = new CultureInfo("en-GB");
    
    [Theory]
    [InlineData("00001", 1)]
    [InlineData("1", 1)]
    [InlineData("1,1", 1.1)]
    [InlineData("1.1", 1.1)]
    [InlineData("1 000.1", 1000.1)]
    [InlineData("1 000,1", 1000.1)]
    public void FillFromString_Decimal_Fr_Ok(string value, decimal ok)
    {
        var o = new DecimalObject();
        ObjectHelper.FillFromString(o, n => value, _fr);
        Assert.Equal(ok, o.Value);
    }
    
    [Theory]
    [InlineData("00001", 1)]
    [InlineData("1", 1)]
    [InlineData("1,1", 1.1)]
    [InlineData("1.1", 1.1)]
    [InlineData("1 000.1", 1000.1)]
    [InlineData("1 000,1", 1000.1)]
    public void FillFromString_Decimal_En_Ok(string value, decimal ok)
    {
        var o = new DecimalObject();
        ObjectHelper.FillFromString(o, n => value, _en);
        Assert.Equal(ok, o.Value);
    }

    public class DecimalObject
    {
        public decimal Value { get; set; }
    }
}