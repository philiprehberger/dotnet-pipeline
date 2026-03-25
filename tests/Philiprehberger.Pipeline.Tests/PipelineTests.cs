using Xunit;
using Philiprehberger.Pipeline;

namespace Philiprehberger.Pipeline.Tests;

public class PipelineTests
{
    [Fact]
    public async Task ExecuteAsync_SingleMiddleware_SetsOutput()
    {
        var pipeline = new Pipeline<string, int>()
            .Use(async (ctx, next) =>
            {
                ctx.Output = ctx.Input.Length;
                await next();
            });

        var result = await pipeline.ExecuteAsync("hello");

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleMiddleware_ExecutesInOrder()
    {
        var order = new List<int>();

        var pipeline = new Pipeline<string, string>()
            .Use(async (ctx, next) =>
            {
                order.Add(1);
                await next();
                order.Add(3);
            })
            .Use(async (ctx, next) =>
            {
                order.Add(2);
                ctx.Output = ctx.Input.ToUpper();
                await next();
            });

        await pipeline.ExecuteAsync("test");

        Assert.Equal(new[] { 1, 2, 3 }, order);
    }

    [Fact]
    public async Task ExecuteAsync_NoOutputSet_ThrowsInvalidOperationException()
    {
        var pipeline = new Pipeline<string, string>()
            .Use(async (ctx, next) => await next());

        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline.ExecuteAsync("test"));
    }

    [Fact]
    public async Task Build_ReturnsReusableFunction()
    {
        var pipeline = new Pipeline<int, int>()
            .Use(async (ctx, next) =>
            {
                ctx.Output = ctx.Input * 2;
                await next();
            });

        var func = pipeline.Build();
        var ctx1 = new PipelineContext<int, int>(5);
        var ctx2 = new PipelineContext<int, int>(10);

        Assert.Equal(10, await func(ctx1));
        Assert.Equal(20, await func(ctx2));
    }
}
