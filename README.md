# Philiprehberger.Pipeline

[![CI](https://github.com/philiprehberger/dotnet-pipeline/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-pipeline/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.Pipeline.svg)](https://www.nuget.org/packages/Philiprehberger.Pipeline)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-pipeline)](LICENSE)
[![Sponsor](https://img.shields.io/badge/sponsor-GitHub%20Sponsors-ec6cb9)](https://github.com/sponsors/philiprehberger)

Middleware pipeline builder for any operation — like ASP.NET Core middleware but for business logic.

## Installation

```bash
dotnet add package Philiprehberger.Pipeline
```

## Usage

```csharp
using Philiprehberger.Pipeline;
```

### Inline Middleware

Build a pipeline using delegate-based middleware:

```csharp
var pipeline = new Pipeline<string, string>();

pipeline.Use(async (context, next) =>
{
    context.Items["startedAt"] = DateTime.UtcNow;
    await next();
});

pipeline.Use(async (context, next) =>
{
    context.Output = context.Input.ToUpperInvariant();
    await next();
});

var result = await pipeline.ExecuteAsync("hello");
// result: "HELLO"
```

### Type-Based Middleware

Create reusable middleware classes:

```csharp
public class ValidationMiddleware : IPipelineMiddleware<CreateOrderRequest, OrderResult>
{
    public async Task ExecuteAsync(
        PipelineContext<CreateOrderRequest, OrderResult> context,
        Func<Task> next)
    {
        if (string.IsNullOrEmpty(context.Input.ProductId))
            throw new ArgumentException("ProductId is required.");

        await next();
    }
}

var pipeline = new Pipeline<CreateOrderRequest, OrderResult>();
pipeline.Use<ValidationMiddleware>();
pipeline.Use<ProcessOrderMiddleware>();
```

### Short-Circuiting

Skip downstream middleware by not calling `next()`:

```csharp
pipeline.Use(async (context, next) =>
{
    if (context.Input == "cached")
    {
        context.Output = cachedResult;
        return; // skip remaining middleware
    }

    await next();
});
```

### Passing Data Between Middleware

Use `context.Items` to share state:

```csharp
pipeline.Use(async (context, next) =>
{
    context.Items["userId"] = ResolveUser(context.Input);
    await next();
});

pipeline.Use(async (context, next) =>
{
    var userId = (string)context.Items["userId"];
    context.Output = await LoadProfile(userId);
    await next();
});
```

### Dependency Injection

Register a configured pipeline with `IServiceCollection`:

```csharp
services.AddPipeline<CreateOrderRequest, OrderResult>(pipeline =>
{
    pipeline.Use<ValidationMiddleware>();
    pipeline.Use<ProcessOrderMiddleware>();
    pipeline.Use<NotificationMiddleware>();
});
```

Then inject and use:

```csharp
public class OrderController
{
    private readonly Pipeline<CreateOrderRequest, OrderResult> _pipeline;

    public OrderController(Pipeline<CreateOrderRequest, OrderResult> pipeline)
        => _pipeline = pipeline;

    public async Task<OrderResult> CreateOrder(CreateOrderRequest request)
        => await _pipeline.ExecuteAsync(request);
}
```

### Reusable Pipeline Function

Build a pipeline once and reuse it:

```csharp
var pipeline = new Pipeline<int, int>();
pipeline.Use(async (context, next) =>
{
    context.Output = context.Input * 2;
    await next();
});

var func = pipeline.Build();

var result1 = await func(new PipelineContext<int, int>(5));   // 10
var result2 = await func(new PipelineContext<int, int>(10));  // 20
```

## API

### `Pipeline<TInput, TOutput>`

| Method | Description |
|--------|-------------|
| `Use(Func<PipelineContext, Func<Task>, Task>)` | Adds a delegate-based middleware |
| `Use<TMiddleware>()` | Adds a type-based middleware implementing `IPipelineMiddleware` |
| `ExecuteAsync(TInput, CancellationToken?)` | Executes the pipeline and returns the output |
| `Build()` | Builds the pipeline into a reusable `Func<PipelineContext, Task<TOutput>>` |

### `IPipelineMiddleware<TInput, TOutput>`

| Method | Description |
|--------|-------------|
| `ExecuteAsync(PipelineContext, Func<Task>)` | Executes middleware logic; call `next()` to continue the chain |

### `PipelineContext<TInput, TOutput>`

| Member | Description |
|--------|-------------|
| `Input` | The pipeline input value (read-only) |
| `Output` | The pipeline output value (read-write) |
| `Items` | Dictionary for passing data between middleware |
| `CancellationToken` | Cancellation token for the pipeline execution |

### `PipelineServiceCollectionExtensions`

| Method | Description |
|--------|-------------|
| `AddPipeline<TInput, TOutput>(Action<Pipeline>)` | Registers a configured pipeline as a singleton |

## Development

```bash
dotnet build src/Philiprehberger.Pipeline.csproj --configuration Release
```

## License

[MIT](LICENSE)
