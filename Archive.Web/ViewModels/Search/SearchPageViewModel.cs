using Archive.Web.ViewModels.Shared;

namespace Archive.Web.ViewModels.Search;

public class SearchPageViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<UserSummaryViewModel> Users { get; set; } = new();
    public List<PostCardViewModel> Posts { get; set; } = new();
}
