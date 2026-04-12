using Archive.Web.Data;
using Archive.Web.ViewModels.Feed;
using Archive.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Archive.Web.Services;

public class FeedService : IFeedService
{
    private readonly AppDbContext _dbContext;
    private readonly IPostViewModelFactory _postViewModelFactory;

    public FeedService(AppDbContext dbContext, IPostViewModelFactory postViewModelFactory)
    {
        _dbContext = dbContext;
        _postViewModelFactory = postViewModelFactory;
    }

    public async Task<LandingPageViewModel> GetLandingPageAsync(int? currentUserId)
    {
        var posts = await GetVisiblePostsQuery()
            .OrderByDescending(x => x.CreatedAt)
            .Take(4)
            .ToListAsync();

        return new LandingPageViewModel
        {
            UserCount = await _dbContext.Users.CountAsync(x => x.IsActive),
            PostCount = await _dbContext.Posts.CountAsync(x => !x.IsDeleted),
            TopicCount = await _dbContext.Topics.CountAsync(x => x.IsVisible),
            FeaturedPosts = _postViewModelFactory.BuildPostCards(posts, currentUserId)
        };
    }

    public async Task<FeedPageViewModel> GetFeedAsync(int userId)
    {
        var followingIds = await _dbContext.Follows
            .Where(x => x.FollowerId == userId)
            .Select(x => x.FollowingId)
            .ToListAsync();

        var posts = await GetVisiblePostsQuery()
            .Where(x => x.UserId == userId || followingIds.Contains(x.UserId))
            .OrderByDescending(x => x.CreatedAt)
            .Take(15)
            .ToListAsync();

        if (posts.Count < 8)
        {
            var existingIds = posts.Select(x => x.Id).ToList();
            var recentPublic = await GetVisiblePostsQuery()
                .Where(x => !existingIds.Contains(x.Id))
                .OrderByDescending(x => x.CreatedAt)
                .Take(8 - posts.Count)
                .ToListAsync();

            posts.AddRange(recentPublic);
        }

        var suggestions = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id != userId && x.IsActive && !x.IsLocked && !followingIds.Contains(x.Id))
            .OrderByDescending(x => x.Posts.Count)
            .Take(4)
            .Select(x => new UserSummaryViewModel
            {
                Id = x.Id,
                UserName = x.UserName,
                DisplayName = x.DisplayName,
                AvatarUrl = x.AvatarUrl,
                Bio = x.Bio,
                IsFollowing = false
            })
            .ToListAsync();

        var topics = await _dbContext.Topics
            .AsNoTracking()
            .Where(x => x.IsVisible)
            .OrderByDescending(x => x.Posts.Count)
            .Take(5)
            .Select(x => new TopicSummaryViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                PostCount = x.Posts.Count(p => !p.IsDeleted && !p.IsHidden)
            })
            .ToListAsync();

        return new FeedPageViewModel
        {
            Composer = new PostComposerViewModel
            {
                TopicOptions = await GetTopicOptionsAsync()
            },
            Posts = _postViewModelFactory.BuildPostCards(posts, userId),
            Suggestions = suggestions,
            TrendingTopics = topics
        };
    }

    public async Task<List<PostCardViewModel>> LoadMorePostsAsync(int userId, int skip, int take)
    {
        var followingIds = await _dbContext.Follows
            .Where(x => x.FollowerId == userId)
            .Select(x => x.FollowingId)
            .ToListAsync();

        var posts = await GetVisiblePostsQuery()
            .Where(x => x.UserId == userId || followingIds.Contains(x.UserId))
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        if (posts.Count == 0)
        {
            posts = await GetVisiblePostsQuery()
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        return _postViewModelFactory.BuildPostCards(posts, userId);
    }

    public async Task<PostDetailPageViewModel?> GetPostDetailAsync(int postId, int? currentUserId)
    {
        var post = await _postViewModelFactory.BuildPostCardAsync(postId, currentUserId);
        if (post is null || post.IsHidden)
        {
            return null;
        }

        var comments = await _dbContext.Comments
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.PostId == postId && !x.IsHidden)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new CommentItemViewModel
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.User!.UserName,
                DisplayName = x.User!.DisplayName,
                AvatarUrl = x.User!.AvatarUrl,
                Content = x.Content,
                CreatedAt = x.CreatedAt,
                ParentCommentId = x.ParentCommentId
            })
            .ToListAsync();

        return new PostDetailPageViewModel
        {
            Post = post,
            Comments = comments,
            QuoteComposer = new PostComposerViewModel
            {
                QuotePostId = postId,
                TopicOptions = await GetTopicOptionsAsync()
            }
        };
    }

    private IQueryable<Models.Post> GetVisiblePostsQuery()
    {
        return _dbContext.Posts
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
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.PostHashtags)
                    .ThenInclude(x => x.Hashtag)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.Likes)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.Comments)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.Reposts)
            .Where(x => !x.IsDeleted && !x.IsHidden && x.User != null && x.User.IsActive && !x.User.IsLocked);
    }

    private async Task<List<SelectListItem>> GetTopicOptionsAsync()
    {
        return await _dbContext.Topics
            .AsNoTracking()
            .Where(x => x.IsVisible)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();
    }
}
