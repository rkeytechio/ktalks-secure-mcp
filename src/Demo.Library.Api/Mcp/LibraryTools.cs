using System.ComponentModel;
using Demo.Library.Api.Authentication;
using Demo.Library.Api.Endpoints;
using Demo.Library.Api.Endpoints.Search.Contracts;
using Demo.Library.Api.Services;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol.Server;

namespace Demo.Library.Api.Mcp;

/// <summary>
/// MCP tool surface for the library domain.
/// </summary>
/// <remarks>
/// 
/// # Secure MCP Design Note:
/// This tool intentionally mixes anonymous, protected, destructive, and read-only operations
/// to demonstrate secure MCP design choices.
/// Runtime behavior depends on endpoint mode plus tool attributes:
/// - <see cref="AllowAnonymousAttribute"/> for safe discovery/read scenarios,
/// - <see cref="AuthorizeAttribute"/> for user-scoped and state-changing actions,
/// - destructive operations add explicit confirmation and server-side validation.
///
/// Security and safety definitions used in this project:
/// 1) <see cref="AllowAnonymousAttribute"/>:
///    Tool can be called without a user token when MCP endpoint mode allows anonymous discovery/calls
///    (tool-level authorization mode).
/// 2) <see cref="AuthorizeAttribute"/>:
///    Tool requires a valid authenticated user that satisfies the configured policy
///    (for this demo, <see cref="LibraryAuthorizationPolicies.McpScopePolicyName"/>).
/// 3) <see cref="McpServerToolAttribute.Destructive"/>:
///    Safety metadata for clients/models indicating the tool may cause disruptive state changes.
///    This is a planning/risk hint, not an authorization or runtime enforcement mechanism by itself.
/// 4) <see cref="McpServerToolAttribute.Idempotent"/>:
///    Metadata indicating whether repeated calls with identical inputs should have the same effect.
///    This informs client orchestration behavior; it does not replace business validation.
///
/// Important:
/// - Authorization still depends on endpoint policy + tool attributes + service-side validation.
/// - Route-level "require auth for all requests" mode will block anonymous tools, even if they
///   have <see cref="AllowAnonymousAttribute"/>.
/// - Destructive tools should additionally include explicit confirmation parameters and server-side
///   validation, as done in account closure.
/// </remarks>
[McpServerToolType]
internal sealed class LibraryTools(ILibraryService libraryService, IHttpContextAccessor httpContextAccessor)
{
    [McpServerTool(Name = "search_books", ReadOnly = true, Idempotent = true)]
    [Description("Find books in the library catalog by title, author, or ISBN. Use availableOnly=true to return only currently available books. No authentication required.")]
    [AllowAnonymous]
    public Task<IReadOnlyList<BookSearchResponse>> SearchBooksAsync(
        [Description("Free-text query for title or author (optional).")]
        string? query = null,
        [Description("Author name filter (optional).")]
        string? author = null,
        [Description("Exact ISBN filter (optional).")]
        string? isbn = null,
        [Description("If true, only books with at least one available copy are returned.")]
        bool availableOnly = false,
        CancellationToken cancellationToken = default) =>
        libraryService.SearchBooksAsync(
            new SearchBooksRequest(query, author, isbn, availableOnly),
            cancellationToken);

    [McpServerTool(Name = "list_my_borrowed_books", ReadOnly = true, Idempotent = true)]
    [Description("Show the signed-in user's currently borrowed books.")]
    [Authorize(Policy = LibraryAuthorizationPolicies.McpScopePolicyName)]
    public async Task<object> GetMyBorrowedBooksAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        return userId is null
            ? new { success = false, message = "Authentication is required." }
            : await libraryService.GetBorrowedBooksAsync(userId, cancellationToken);
    }

    [McpServerTool(Name = "borrow_book", Destructive = false, Idempotent = false)]
    [Description("Borrow a book for the signed-in user. Use when the user wants to check out a specific book by ID.")]
    [Authorize(Policy = LibraryAuthorizationPolicies.McpScopePolicyName)]
    public async Task<object> BorrowBookAsync(
        [Description("Book ID (GUID) to borrow.")] string bookId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return new { success = false, message = "Authentication is required." };
        }

        return ToToolResult(await libraryService.BorrowBookAsync(bookId, userId, cancellationToken));
    }

    [McpServerTool(Name = "return_book", Destructive = false, Idempotent = false)]
    [Description("Return a borrowed book for the signed-in user.")]
    [Authorize(Policy = LibraryAuthorizationPolicies.McpScopePolicyName)]
    public async Task<object> ReturnBookAsync(
        [Description("Book ID (GUID) to return.")] string bookId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return new { success = false, message = "Authentication is required." };
        }

        return ToToolResult(await libraryService.ReturnBookAsync(bookId, userId, cancellationToken));
    }

    [McpServerTool(Name = "request_my_account_closure", Destructive = true, Idempotent = false)]
    [Description("Submit an account-closure request for the signed-in user. High-impact action: only use when the user explicitly asks to close their account.")]
    [Authorize(Policy = LibraryAuthorizationPolicies.McpScopePolicyName)]
    public async Task<object> RequestAccountClosureAsync(
        [Description("Human-readable reason for account closure request.")] string reason,
        [Description("Safety confirmation. Must be exactly CLOSE_MY_ACCOUNT.")] string confirmation,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(confirmation?.Trim(), "CLOSE_MY_ACCOUNT", StringComparison.Ordinal))
        {
            return new
            {
                success = false,
                message = "Confirmation failed. Set confirmation to CLOSE_MY_ACCOUNT to proceed."
            };
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return new { success = false, message = "Authentication is required." };
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return new
            {
                success = false,
                message = "A reason for closing the account is required."
            };
        }

        return ToToolResult(await libraryService.RequestAccountClosureAsync(userId, reason.Trim(), cancellationToken));
    }

    private string? GetCurrentUserId() =>
        httpContextAccessor.HttpContext?.GetCurrentUserId();

    private static object ToToolResult(LibraryActionResult result) =>
        result.Status == LibraryActionStatus.Success
            ? new { success = true, result = result.Payload }
            : new { success = false, message = result.Message };
}