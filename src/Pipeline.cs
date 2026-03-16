namespace Philiprehberger.Pipeline;

/// <summary>
/// Builds and executes a middleware pipeline for processing an input into an output.
/// Middleware components run in the order they are registered, each deciding whether
/// to call the next component in the chain.
/// </summary>
/// <typeparam name="TInput">The type of the pipeline input.</typeparam>
/// <typeparam name="TOutput">The type of the pipeline output.</typeparam>
public sealed class Pipeline<TInput, TOutput>
{
    private readonly List<Func<PipelineContext<TInput, TOutput>, Func<Task>, Task>> _middlewares = new();
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>
    /// Creates a new <see cref="Pipeline{TInput, TOutput}"/> instance.
    /// </summary>
    public Pipeline() { }

    /// <summary>
    /// Creates a new <see cref="Pipeline{TInput, TOutput}"/> instance with a service provider
    /// for resolving type-based middleware.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve middleware instances.</param>
    public Pipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Adds a delegate-based middleware to the pipeline.
    /// </summary>
    /// <param name="middleware">A function that receives the context and a next delegate.</param>
    /// <returns>This pipeline for chaining.</returns>
    public Pipeline<TInput, TOutput> Use(Func<PipelineContext<TInput, TOutput>, Func<Task>, Task> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    /// <summary>
    /// Adds a type-based middleware to the pipeline. The middleware is resolved from the
    /// service provider if available, otherwise it is created via <see cref="Activator.CreateInstance{T}"/>.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to add.</typeparam>
    /// <returns>This pipeline for chaining.</returns>
    public Pipeline<TInput, TOutput> Use<TMiddleware>() where TMiddleware : IPipelineMiddleware<TInput, TOutput>
    {
        _middlewares.Add((context, next) =>
        {
            var middleware = _serviceProvider is not null
                ? (TMiddleware?)_serviceProvider.GetService(typeof(TMiddleware))
                  ?? Activator.CreateInstance<TMiddleware>()
                : Activator.CreateInstance<TMiddleware>();

            return middleware.ExecuteAsync(context, next);
        });

        return this;
    }

    /// <summary>
    /// Executes the pipeline with the given input.
    /// </summary>
    /// <param name="input">The input value to process.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The pipeline output.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no middleware sets the <see cref="PipelineContext{TInput, TOutput}.Output"/> value.
    /// </exception>
    public async Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default)
    {
        var func = Build();
        var context = new PipelineContext<TInput, TOutput>(input, cancellationToken);
        return await func(context);
    }

    /// <summary>
    /// Builds the pipeline into a reusable function.
    /// </summary>
    /// <returns>A function that accepts a <see cref="PipelineContext{TInput, TOutput}"/> and returns the output.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no middleware sets the <see cref="PipelineContext{TInput, TOutput}.Output"/> value.
    /// </exception>
    public Func<PipelineContext<TInput, TOutput>, Task<TOutput>> Build()
    {
        return async context =>
        {
            var index = 0;

            Task Next()
            {
                if (index < _middlewares.Count)
                {
                    var middleware = _middlewares[index];
                    index++;
                    return middleware(context, Next);
                }

                return Task.CompletedTask;
            }

            await Next();

            return context.Output
                ?? throw new InvalidOperationException(
                    "Pipeline completed but no middleware set the Output. " +
                    "Ensure at least one middleware assigns context.Output.");
        };
    }
}
