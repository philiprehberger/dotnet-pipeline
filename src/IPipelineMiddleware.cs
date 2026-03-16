namespace Philiprehberger.Pipeline;

/// <summary>
/// Defines a middleware component that can be added to a <see cref="Pipeline{TInput, TOutput}"/>.
/// </summary>
/// <typeparam name="TInput">The type of the pipeline input.</typeparam>
/// <typeparam name="TOutput">The type of the pipeline output.</typeparam>
public interface IPipelineMiddleware<TInput, TOutput>
{
    /// <summary>
    /// Executes the middleware logic.
    /// </summary>
    /// <param name="context">The pipeline context containing input, output, and shared state.</param>
    /// <param name="next">A delegate that invokes the next middleware in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(PipelineContext<TInput, TOutput> context, Func<Task> next);
}
