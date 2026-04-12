using Archive.Web.ViewModels.Search;
using Archive.Web.ViewModels.Shared;

namespace Archive.Web.Services;

public interface ISearchService
{
    Task<SearchPageViewModel> SearchAsync(string? query, int? currentUserId);
    Task<List<SearchSuggestionItemViewModel>> GetSuggestionsAsync(string? query);
}
