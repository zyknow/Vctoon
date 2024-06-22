namespace Vctoon.Blazor.Client.Components.Paginations;

/// <summary>
///     Indicates a pagination behavior.
/// </summary>
public enum Page
{
    /// <summary>
    ///     Navigate to the first page of results.
    /// </summary>
    First,

    /// <summary>
    ///     Navigate to the previous page of results.
    /// </summary>
    Previous,

    /// <summary>
    ///     Navigate to the next page of results.
    /// </summary>
    Next,

    /// <summary>
    ///     Navigate to the last page of results.
    /// </summary>
    Last
}