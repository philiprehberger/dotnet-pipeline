namespace Philiprehberger.Pipeline;

/// <summary>
/// Carries input, output, and shared state through the pipeline.
/// </summary>
/// <typeparam name="TInput">The type of the pipeline input.</typeparam>
/// <typeparam name="TOutput">The type of the pipeline output.</typeparam>
public sealed class PipelineContext<TInput, TOutput>
{
    /// <summary>
    /// Creates a new <see cref="PipelineContext{TInput, TOutput}"/> instance.
    /// </summary>
    /// <param name="input">The pipeline input value.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    public PipelineContext(TInput input, CancellationToken cancellationToken = default)
    {
        Input = input;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the pipeline input value.
    /// </summary>
    public TInput Input { get; }

    /// <summary>
    /// Gets or sets the pipeline output value.
    /// </summary>
    public TOutput? Output { get; set; }

    /// <summary>
    /// Gets a dictionary for passing arbitrary data between middleware components.
    /// </summary>
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the cancellation token for the pipeline execution.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}
