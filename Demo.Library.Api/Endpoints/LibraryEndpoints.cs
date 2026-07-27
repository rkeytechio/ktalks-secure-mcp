using Demo.Library.Api.Endpoints.Me;
using Demo.Library.Api.Endpoints.Search;

namespace Demo.Library.Api.Endpoints;

internal static class LibraryEndpoints
{
    public static RouteGroupBuilder MapLibraryEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var search = api.MapGroup("/search")
            .WithTags("Library Search");
        var me = api.MapGroup("/me")
            .WithTags("My Library");

        search.MapSearchBooks();
        me.MapBorrowBook();
        me.MapReturnBook();
        me.MapGetMyBorrowedBooks();

        return api;
    }
}
