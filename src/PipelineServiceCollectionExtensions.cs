using Microsoft.Extensions.DependencyInjection;

namespace Philiprehberger.Pipeline;

/// <summary>
/// Extension methods for registering <see cref="Pipeline{TInput, TOutput}"/> with dependency injection.
/// </summary>
public static class PipelineServiceCollectionExtensions
{
    /// <summary>
    /// Registers a configured <see cref="Pipeline{TInput, TOutput}"/> as a singleton in the service collection.
    /// </summary>
    /// <typeparam name="TInput">The type of the pipeline input.</typeparam>
    /// <typeparam name="TOutput">The type of the pipeline output.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">A delegate that configures the pipeline by adding middleware.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPipeline<TInput, TOutput>(
        this IServiceCollection services,
        Action<Pipeline<TInput, TOutput>> configure)
    {
        services.AddSingleton(sp =>
        {
            var pipeline = new Pipeline<TInput, TOutput>(sp);
            configure(pipeline);
            return pipeline;
        });

        return services;
    }
}
