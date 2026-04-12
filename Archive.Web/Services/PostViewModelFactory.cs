using Archive.Web.Data;
using Archive.Web.Models;
using Archive.Web.ViewModels.Shared;
using Microsoft.EntityFrameworkCore;

namespace Archive.Web.Services;

public class PostViewModelFactory : IPostViewModelFactory
{
    private readonly AppDbContext _dbContext;

    public PostViewModelFactory(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PostCardViewModel?> BuildPostCardAsync(int postId, int? currentUserId)
    {
        var post = await QueryPosts()
            .FirstOrDefaultAsync(x => x.Id == postId);

        return post is null ? null : MapPost(post, currentUserId, includeQuote: true);
    }

    public List<PostCardViewModel> BuildPostCards(IEnumerable<Post> posts, int? currentUserId)
    {
        return posts.Select(post => MapPost(post, currentUserId, includeQuote: true)).ToList();
    }

    private IQueryable<Post> QueryPosts()
    {
        return _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Topic)
            .Include(x => x.Images.OrderBy(i => i.DisplayOrder))
            .Include(x => x.Likes)
            .Include(x => x.Comments)
            .Include(x => x.Reposts)
            .Include(x => x.PostHashtags)
                .ThenInclude(x => x.Hashtag)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.User)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.Images.OrderBy(i => i.DisplayOrder))
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.PostHashtags)
                    .ThenInclude(x => x.Hashtag)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.Likes)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.Comments)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.Reposts);
    }

    private PostCardViewModel MapPost(Post post, int? currentUserId, bool includeQuote)
    {
        return new PostCardViewModel
        {
            Id = post.Id,
            UserId = post.UserId,
            UserName = post.User?.UserName ?? string.Empty,
            DisplayName = post.User?.DisplayName ?? string.Empty,
            AvatarUrl = post.User?.AvatarUrl,
            Content = post.Content,
            TopicName = post.Topic?.Name,
            TopicSlug = post.Topic?.Slug,
            ImageUrl = post.Images.OrderBy(x => x.DisplayOrder).Select(x => x.ImageUrl).FirstOrDefault(),
            CreatedAt = post.CreatedAt,
            IsHidden = post.IsHidden,
            IsOwnedByCurrentUser = currentUserId == post.UserId,
            IsLikedByCurrentUser = currentUserId.HasValue && post.Likes.Any(x => x.UserId == currentUserId.Value),
            IsRepostedByCurrentUser = currentUserId.HasValue && post.Reposts.Any(x => x.UserId == currentUserId.Value),
            IsQuotedPost = post.QuotePostId.HasValue,
            LikeCount = post.Likes.Count,
            CommentCount = post.Comments.Count(x => !x.IsHidden),
            RepostCount = post.Reposts.Count,
            Hashtags = post.PostHashtags
                .Select(x => x.Hashtag?.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList(),
            QuotedPost = includeQuote && post.QuotePost is not null
                ? MapPost(post.QuotePost, currentUserId, includeQuote: false)
                : null
        };
    }
}
