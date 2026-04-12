using Archive.Web.Data;
using Archive.Web.ViewModels.Search;
using Archive.Web.ViewModels.Shared;
using Microsoft.EntityFrameworkCore;

namespace Archive.Web.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _dbContext;
    private readonly IPostViewModelFactory _postViewModelFactory;

    public SearchService(AppDbContext dbContext, IPostViewModelFactory postViewModelFactory)
    {
        _dbContext = dbContext;
        _postViewModelFactory = postViewModelFactory;
    }

    public async Task<SearchPageViewModel> SearchAsync(string? query, int? currentUserId)
    {
        var normalizedQuery = (query ?? string.Empty).Trim().ToLower();
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return new SearchPageViewModel();
        }

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsLocked &&
                        (x.UserName.Contains(normalizedQuery) ||
                         x.DisplayName.ToLower().Contains(normalizedQuery)))
            .OrderBy(x => x.DisplayName)
            .Take(10)
            .Select(x => new UserSummaryViewModel
            {
                Id = x.Id,
                UserName = x.UserName,
                DisplayName = x.DisplayName,
                AvatarUrl = x.AvatarUrl,
                Bio = x.Bio,
                IsFollowing = currentUserId.HasValue && x.Followers.Any(f => f.FollowerId == currentUserId.Value)
            })
            .ToListAsync();

        var posts = await _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Topic)
            .Include(x => x.Images)
            .Include(x => x.Likes)
            .Include(x => x.Comments)
            .Include(x => x.Reposts)
            .Include(x => x.PostHashtags)
                .ThenInclude(x => x.Hashtag)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.User)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.Images)
            .Where(x => !x.IsDeleted && !x.IsHidden &&
                        (x.Content.ToLower().Contains(normalizedQuery) ||
                         x.PostHashtags.Any(tag => tag.Hashtag!.Name.Contains(normalizedQuery))))
            .OrderByDescending(x => x.CreatedAt)
            .Take(12)
            .ToListAsync();

        return new SearchPageViewModel
        {
            Query = query ?? string.Empty,
            Users = users,
            Posts = _postViewModelFactory.BuildPostCards(posts, currentUserId)
        };
    }

    public async Task<List<SearchSuggestionItemViewModel>> GetSuggestionsAsync(string? query)
    {
        var normalizedQuery = (query ?? string.Empty).Trim().ToLower();
        if (normalizedQuery.Length < 2)
        {
            return new List<SearchSuggestionItemViewModel>();
        }

        var userSuggestions = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsLocked &&
                        (x.UserName.Contains(normalizedQuery) ||
                         x.DisplayName.ToLower().Contains(normalizedQuery)))
            .OrderBy(x => x.DisplayName)
            .Take(4)
            .Select(x => new SearchSuggestionItemViewModel
            {
                Type = "user",
                Label = $"{x.DisplayName} (@{x.UserName})",
                Url = $"/Profiles/{x.UserName}"
            })
            .ToListAsync();

        var postSuggestions = await _dbContext.Posts
            .AsNoTracking()
            .Where(x => !x.IsDeleted && !x.IsHidden && x.Content.ToLower().Contains(normalizedQuery))
            .OrderByDescending(x => x.CreatedAt)
            .Take(4)
            .Select(x => new
            {
                x.Id,
                x.Content
            })
            .ToListAsync();

        var mappedPosts = postSuggestions
            .Select(x => new SearchSuggestionItemViewModel
            {
                Type = "post",
                Label = x.Content.Length > 48 ? $"{x.Content.Substring(0, 48)}..." : x.Content,
                Url = $"/Posts/Details/{x.Id}"
            });

        return userSuggestions.Concat(mappedPosts).Take(6).ToList();
    }
}
