using Archive.Web.Data;
using Archive.Web.Extensions;
using Archive.Web.Models;
using Archive.Web.ViewModels.Feed;
using Archive.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Archive.Web.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public PostService(AppDbContext dbContext, IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    public async Task<ServiceResult<int>> CreatePostAsync(int userId, PostComposerViewModel model)
    {
        var post = new Post
        {
            UserId = userId,
            TopicId = model.TopicId,
            QuotePostId = model.QuotePostId,
            Content = model.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        if (model.QuotePostId.HasValue)
        {
            var quoteExists = await _dbContext.Posts.AnyAsync(x => x.Id == model.QuotePostId.Value && !x.IsHidden && !x.IsDeleted);
            if (!quoteExists)
            {
                return ServiceResult<int>.Fail("Bài viết được quote không còn khả dụng.");
            }
        }

        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync();

        if (model.ImageFile is not null)
        {
            var uploadResult = await _fileStorageService.SaveImageAsync(model.ImageFile, "posts");
            if (!uploadResult.Success || string.IsNullOrWhiteSpace(uploadResult.Data))
            {
                return ServiceResult<int>.Fail(uploadResult.Message);
            }

            _dbContext.PostImages.Add(new PostImage
            {
                PostId = post.Id,
                ImageUrl = uploadResult.Data,
                DisplayOrder = 0
            });
        }

        await SyncHashtagsAsync(post.Id, model.Content);
        await _dbContext.SaveChangesAsync();

        return ServiceResult<int>.Ok(post.Id, "Đăng bài thành công.");
    }

    public async Task<PostComposerViewModel?> GetPostForEditAsync(int postId, int userId)
    {
        var post = await _dbContext.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == postId && x.UserId == userId && !x.IsDeleted);

        if (post is null)
        {
            return null;
        }

        return new PostComposerViewModel
        {
            Content = post.Content,
            TopicId = post.TopicId,
            QuotePostId = post.QuotePostId,
            TopicOptions = await GetTopicOptionsAsync()
        };
    }

    public async Task<ServiceResult> UpdatePostAsync(int postId, int userId, PostComposerViewModel model)
    {
        var post = await _dbContext.Posts
            .Include(x => x.Images)
            .Include(x => x.PostHashtags)
            .FirstOrDefaultAsync(x => x.Id == postId && x.UserId == userId && !x.IsDeleted);

        if (post is null)
        {
            return ServiceResult.Fail("Không tìm thấy bài viết để chỉnh sửa.");
        }

        post.Content = model.Content.Trim();
        post.TopicId = model.TopicId;
        post.UpdatedAt = DateTime.UtcNow;

        if (model.ImageFile is not null)
        {
            var uploadResult = await _fileStorageService.SaveImageAsync(model.ImageFile, "posts");
            if (!uploadResult.Success || string.IsNullOrWhiteSpace(uploadResult.Data))
            {
                return ServiceResult.Fail(uploadResult.Message);
            }

            var currentImage = post.Images.OrderBy(x => x.DisplayOrder).FirstOrDefault();
            if (currentImage is null)
            {
                _dbContext.PostImages.Add(new PostImage
                {
                    PostId = post.Id,
                    ImageUrl = uploadResult.Data,
                    DisplayOrder = 0
                });
            }
            else
            {
                currentImage.ImageUrl = uploadResult.Data;
            }
        }

        await SyncHashtagsAsync(post.Id, model.Content);
        await _dbContext.SaveChangesAsync();

        return ServiceResult.Ok("Cập nhật bài viết thành công.");
    }

    public async Task<ServiceResult> DeletePostAsync(int postId, int userId)
    {
        var post = await _dbContext.Posts
            .FirstOrDefaultAsync(x => x.Id == postId && x.UserId == userId && !x.IsDeleted);

        if (post is null)
        {
            return ServiceResult.Fail("Không tìm thấy bài viết.");
        }

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return ServiceResult.Ok("Đã ẩn bài viết khỏi feed.");
    }

    public async Task<ServiceResult<ToggleStateViewModel>> ToggleLikeAsync(int postId, int userId)
    {
        var postExists = await _dbContext.Posts.AnyAsync(x => x.Id == postId && !x.IsDeleted && !x.IsHidden);
        if (!postExists)
        {
            return ServiceResult<ToggleStateViewModel>.Fail("Bài viết không tồn tại.");
        }

        var like = await _dbContext.Likes.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId);
        var active = false;
        if (like is null)
        {
            _dbContext.Likes.Add(new Like
            {
                PostId = postId,
                UserId = userId
            });
            active = true;
        }
        else
        {
            _dbContext.Likes.Remove(like);
        }

        await _dbContext.SaveChangesAsync();

        var likeCount = await _dbContext.Likes.CountAsync(x => x.PostId == postId);
        return ServiceResult<ToggleStateViewModel>.Ok(new ToggleStateViewModel
        {
            Active = active,
            Count = likeCount
        }, active ? "Đã thả tim bài viết." : "Đã bỏ tim bài viết.");
    }

    public async Task<ServiceResult<ToggleStateViewModel>> ToggleRepostAsync(int postId, int userId)
    {
        var postExists = await _dbContext.Posts.AnyAsync(x => x.Id == postId && !x.IsDeleted && !x.IsHidden);
        if (!postExists)
        {
            return ServiceResult<ToggleStateViewModel>.Fail("Bài viết không tồn tại.");
        }

        var repost = await _dbContext.Reposts.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId);
        var active = false;
        if (repost is null)
        {
            _dbContext.Reposts.Add(new Repost
            {
                PostId = postId,
                UserId = userId
            });
            active = true;
        }
        else
        {
            _dbContext.Reposts.Remove(repost);
        }

        await _dbContext.SaveChangesAsync();

        var repostCount = await _dbContext.Reposts.CountAsync(x => x.PostId == postId);
        return ServiceResult<ToggleStateViewModel>.Ok(new ToggleStateViewModel
        {
            Active = active,
            Count = repostCount
        }, active ? "Đã repost bài viết." : "Đã bỏ repost.");
    }

    public async Task<ServiceResult<CommentFeedResponseViewModel>> GetCommentsAsync(int postId)
    {
        var postExists = await _dbContext.Posts.AnyAsync(x => x.Id == postId && !x.IsDeleted);
        if (!postExists)
        {
            return ServiceResult<CommentFeedResponseViewModel>.Fail("Không tìm thấy bài viết.");
        }

        var comments = await QueryComments(postId).ToListAsync();
        return ServiceResult<CommentFeedResponseViewModel>.Ok(new CommentFeedResponseViewModel
        {
            Count = comments.Count,
            Comments = comments
        });
    }

    public async Task<ServiceResult<CommentFeedResponseViewModel>> AddCommentAsync(int postId, int userId, string content, int? parentCommentId)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return ServiceResult<CommentFeedResponseViewModel>.Fail("Bình luận không được để trống.");
        }

        var postExists = await _dbContext.Posts.AnyAsync(x => x.Id == postId && !x.IsDeleted && !x.IsHidden);
        if (!postExists)
        {
            return ServiceResult<CommentFeedResponseViewModel>.Fail("Bài viết không tồn tại.");
        }

        if (parentCommentId.HasValue)
        {
            var parentExists = await _dbContext.Comments.AnyAsync(x => x.Id == parentCommentId.Value && x.PostId == postId);
            if (!parentExists)
            {
                return ServiceResult<CommentFeedResponseViewModel>.Fail("Bình luận gốc không tồn tại.");
            }
        }

        _dbContext.Comments.Add(new Comment
        {
            PostId = postId,
            UserId = userId,
            ParentCommentId = parentCommentId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        var comments = await QueryComments(postId).ToListAsync();
        return ServiceResult<CommentFeedResponseViewModel>.Ok(new CommentFeedResponseViewModel
        {
            Count = comments.Count,
            Comments = comments
        }, "Đã gửi bình luận.");
    }

    private async Task SyncHashtagsAsync(int postId, string content)
    {
        var tags = SlugHelper.ExtractHashtags(content);

        var currentLinks = await _dbContext.PostHashtags
            .Where(x => x.PostId == postId)
            .ToListAsync();

        if (currentLinks.Count != 0)
        {
            _dbContext.PostHashtags.RemoveRange(currentLinks);
        }

        foreach (var tag in tags)
        {
            var hashtag = await _dbContext.Hashtags.FirstOrDefaultAsync(x => x.Slug == tag);
            if (hashtag is null)
            {
                hashtag = new Hashtag
                {
                    Name = tag,
                    Slug = tag
                };
                _dbContext.Hashtags.Add(hashtag);
                await _dbContext.SaveChangesAsync();
            }

            _dbContext.PostHashtags.Add(new PostHashtag
            {
                PostId = postId,
                HashtagId = hashtag.Id
            });
        }
    }

    private IQueryable<CommentItemViewModel> QueryComments(int postId)
    {
        return _dbContext.Comments
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
            });
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
