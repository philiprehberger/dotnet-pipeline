using Xunit;
using Philiprehberger.Pipeline;

namespace Philiprehberger.Pipeline.Tests;

public class PipelineContextTests
{
    [Fact]
    public void Constructor_SetsInput()
    {
        var context = new PipelineContext<string, int>("hello");

        Assert.Equal("hello", context.Input);
    }

    [Fact]
    public void Output_DefaultIsNull()
    {
        var context = new PipelineContext<string, string>("test");

        Assert.Null(context.Output);
    }

    [Fact]
    public void Items_CanStoreAndRetrieveData()
    {
        var context = new PipelineContext<string, string>("test");

        context.Items["key"] = "value";

        Assert.Equal("value", context.Items["key"]);
    }

    [Fact]
    public void CancellationToken_DefaultIsNone()
    {
        var context = new PipelineContext<string, string>("test");

        Assert.Equal(CancellationToken.None, context.CancellationToken);
    }

    [Fact]
    public void CancellationToken_CanBeProvided()
    {
        using var cts = new CancellationTokenSource();
        var context = new PipelineContext<string, string>("test", cts.Token);

        Assert.Equal(cts.Token, context.CancellationToken);
    }
}
