const csrfToken = document.querySelector('meta[name="csrf-token"]')?.content ?? '';

async function apiRequest(url, options = {}) {
    const finalOptions = {
        headers: {
            'X-Requested-With': 'XMLHttpRequest',
            ...options.headers
    },
    ...options
  };

  if (finalOptions.method && finalOptions.method.toUpperCase() !== 'GET' && csrfToken) {
    finalOptions.headers.RequestVerificationToken = csrfToken;
  }

    const response = await fetch(url, finalOptions);
    const contentType = response.headers.get('content-type') || '';

    if (!contentType.includes('application/json')) {
        const text = await response.text();
        throw new Error(text || 'Server did not return JSON.');
    }

    return response.json();
}

function escapeHtml(value) {
  return String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

function buildCommentHtml(comment) {
  const initials = (comment.displayName || 'AU')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map(part => part.charAt(0))
    .join('')
    .toUpperCase();

  return `
    <div class="comment-row">
      <div class="avatar-circle avatar-fallback small-avatar">${escapeHtml(initials)}</div>
      <div>
        <div class="comment-author">${escapeHtml(comment.displayName)} <span>@${escapeHtml(comment.userName)}</span></div>
        <p>${escapeHtml(comment.content)}</p>
        <small>${new Date(comment.createdAt).toLocaleString('vi-VN')}</small>
      </div>
    </div>`;
}

function buildLoadMorePostHtml(post) {
  const hashtags = (post.hashtags || [])
    .map(tag => `<a class="hashtag-chip" href="/Search?q=${encodeURIComponent(tag)}">#${escapeHtml(tag)}</a>`)
    .join('');

  const quote = post.quotedPost
    ? `
      <div class="quote-card mt-3">
        <div class="quote-card-meta">
          <svg class="icon"><use href="#i-quote"></use></svg>
          <span>@${escapeHtml(post.quotedPost.userName)}</span>
        </div>
        <div class="fw-semibold">${escapeHtml(post.quotedPost.displayName)}</div>
        <div class="quote-card-text">${escapeHtml(post.quotedPost.content)}</div>
        ${post.quotedPost.imageUrl ? `<div class="post-media-frame mt-3"><img src="${escapeHtml(post.quotedPost.imageUrl)}" class="post-cover-image" alt="quote image" /></div>` : ''}
      </div>`
    : '';

  const initials = (post.displayName || 'AU')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map(part => part.charAt(0))
    .join('')
    .toUpperCase();

  return `
    <article class="archive-card post-card" data-post-card data-post-id="${post.id}">
      <div class="post-card-shell">
        <div class="post-card-header">
          <div class="post-card-identity">
            <a href="/Profiles/${encodeURIComponent(post.userName)}" class="text-decoration-none">
              ${post.avatarUrl
                ? `<img src="${escapeHtml(post.avatarUrl)}" alt="${escapeHtml(post.displayName)}" class="avatar-circle" />`
                : `<div class="avatar-circle avatar-fallback">${escapeHtml(initials)}</div>`}
            </a>
            <div class="post-card-author">
              <div class="post-meta-line">
                <a href="/Profiles/${encodeURIComponent(post.userName)}" class="post-display-name">${escapeHtml(post.displayName)}</a>
                <span class="post-handle">@${escapeHtml(post.userName)}</span>
              </div>
              <div class="post-subline">
                <span class="post-time">${new Date(post.createdAt).toLocaleString('vi-VN')}</span>
                ${post.topicName ? `<span class="topic-pill">${escapeHtml(post.topicName)}</span>` : ''}
              </div>
            </div>
          </div>
        </div>
        <div class="post-card-body">
          <p class="post-content">${escapeHtml(post.content)}</p>
          ${hashtags ? `<div class="post-tag-row">${hashtags}</div>` : ''}
          ${post.imageUrl ? `<div class="post-media-frame"><img src="${escapeHtml(post.imageUrl)}" alt="Ảnh bài viết" class="post-cover-image" /></div>` : ''}
          ${quote}
        </div>
        <div class="post-actions post-footer-strip">
            <button type="button" class="post-action-button ${post.isLikedByCurrentUser ? 'active' : ''}" data-like-button data-post-id="${post.id}" title="Thích" aria-label="Thích">
              <svg class="icon"><use href="#i-heart"></use></svg>
              <span class="visually-hidden">Thích</span>
              <strong data-like-count>${post.likeCount}</strong>
            </button>
            <a href="/Posts/Details/${post.id}" class="post-action-button" title="Bình luận" aria-label="Bình luận">
              <svg class="icon"><use href="#i-comment"></use></svg>
              <span class="visually-hidden">Bình luận</span>
              <strong>${post.commentCount}</strong>
            </a>
            <button type="button" class="post-action-button ${post.isRepostedByCurrentUser ? 'active' : ''}" data-repost-button data-post-id="${post.id}" title="Repost" aria-label="Repost">
              <svg class="icon"><use href="#i-repost"></use></svg>
              <span class="visually-hidden">Repost</span>
              <strong data-repost-count>${post.repostCount}</strong>
            </button>
            <a href="/Posts/Details/${post.id}#quote-box" class="post-action-button" title="Quote" aria-label="Quote">
              <svg class="icon"><use href="#i-quote"></use></svg>
              <span class="visually-hidden">Quote</span>
              <strong>+</strong>
            </a>
        </div>
      </div>
    </article>`;
}

async function bindLikeButtons() {
  document.querySelectorAll('[data-like-button]').forEach(button => {
    if (button.dataset.bound === 'true') {
      return;
    }

    button.dataset.bound = 'true';
    button.addEventListener('click', async () => {
      const postId = button.dataset.postId;
      const result = await apiRequest(`/api/like/${postId}`, { method: 'POST' });
      if (!result.status) {
        alert(result.message || 'Không thể cập nhật lượt thích.');
        return;
      }

      button.classList.toggle('active', result.data.active);
      const countNode = button.querySelector('[data-like-count]');
      if (countNode) {
        countNode.textContent = result.data.count;
      }
    });
  });
}

async function bindRepostButtons() {
  document.querySelectorAll('[data-repost-button]').forEach(button => {
    if (button.dataset.bound === 'true') {
      return;
    }

    button.dataset.bound = 'true';
    button.addEventListener('click', async () => {
      const postId = button.dataset.postId;
      const result = await apiRequest(`/api/repost/${postId}`, { method: 'POST' });
      if (!result.status) {
        alert(result.message || 'Không thể cập nhật repost.');
        return;
      }

      button.classList.toggle('active', result.data.active);
      const countNode = button.querySelector('[data-repost-count]');
      if (countNode) {
        countNode.textContent = result.data.count;
      }
    });
  });
}

async function bindFollowButtons() {
  document.querySelectorAll('[data-follow-button]').forEach(button => {
    if (button.dataset.bound === 'true') {
      return;
    }

    button.dataset.bound = 'true';
    button.addEventListener('click', async () => {
      const userId = button.dataset.userId;
      const result = await apiRequest(`/api/follow/${userId}`, { method: 'POST' });
      if (!result.status) {
        alert(result.message || 'Không thể cập nhật theo dõi.');
        return;
      }

      document.querySelectorAll(`[data-follow-button][data-user-id="${userId}"]`).forEach(targetButton => {
        targetButton.textContent = result.data.active ? 'Đang theo dõi' : 'Theo dõi';
      });

      document.querySelectorAll('[data-followers-count]').forEach(node => {
        node.textContent = result.data.followersCount;
      });
    });
  });
}

async function refreshComments(postId) {
  const container = document.querySelector('[data-comments-container]');
  if (!container) {
    return;
  }

  const result = await apiRequest(`/api/comments/${postId}`);
  if (!result.status) {
    container.innerHTML = `<div class="soft-panel">${escapeHtml(result.message)}</div>`;
    return;
  }

  if (!result.data.comments.length) {
    container.innerHTML = '<div class="soft-panel">Chưa có bình luận nào. Bạn có thể mở lời đầu tiên.</div>';
    return;
  }

  container.innerHTML = result.data.comments.map(buildCommentHtml).join('');
}

function bindCommentForm() {
  const form = document.querySelector('[data-comment-form]');
  if (!form || form.dataset.bound === 'true') {
    return;
  }

  form.dataset.bound = 'true';
  form.addEventListener('submit', async event => {
    event.preventDefault();
    const postId = form.dataset.postId;
    const formData = new FormData(form);
    const antiforgeryToken = form.querySelector('input[name="__RequestVerificationToken"]')?.value ?? csrfToken;
    let result;

    try {
      result = await apiRequest(`/api/comments/${postId}`, {
        method: 'POST',
        body: formData,
        headers: {
          RequestVerificationToken: antiforgeryToken
        }
      });
    } catch (error) {
      alert('Không thể gửi bình luận lúc này. Hãy tải lại trang và thử lại.');
      return;
    }

    if (!result.status) {
      alert(result.message || 'Không thể gửi bình luận.');
      return;
    }

    const container = document.querySelector('[data-comments-container]');
    if (container) {
      container.innerHTML = result.data.comments.map(buildCommentHtml).join('');
    }

    form.reset();
  });

  document.querySelectorAll('[data-refresh-comments]').forEach(button => {
    if (button.dataset.bound === 'true') {
      return;
    }

    button.dataset.bound = 'true';
    button.addEventListener('click', async () => {
      await refreshComments(button.dataset.postId);
    });
  });
}

function bindSearchSuggestions() {
  const input = document.getElementById('globalSearchInput');
  const container = document.getElementById('globalSearchSuggestions');
  if (!input || !container) {
    return;
  }

  let timeoutId;
  input.addEventListener('input', () => {
    clearTimeout(timeoutId);
    const query = input.value.trim();

    if (query.length < 2) {
      container.classList.add('d-none');
      container.innerHTML = '';
      return;
    }

    timeoutId = setTimeout(async () => {
      const result = await apiRequest(`/api/suggestions?q=${encodeURIComponent(query)}`);
      const items = result.data || [];

      if (!items.length) {
        container.innerHTML = '<div class="suggestion-empty">Không có gợi ý phù hợp.</div>';
        container.classList.remove('d-none');
        return;
      }

      container.innerHTML = items.map(item => `
        <a href="${escapeHtml(item.url)}">
          ${escapeHtml(item.label)}
          <small>${escapeHtml(item.type === 'user' ? 'Người dùng' : 'Bài viết')}</small>
        </a>`).join('');
      container.classList.remove('d-none');
    }, 250);
  });

  document.addEventListener('click', event => {
    if (!container.contains(event.target) && event.target !== input) {
      container.classList.add('d-none');
    }
  });
}

function bindLoadMore() {
  const button = document.getElementById('loadMoreButton');
  const list = document.getElementById('feedList');
  if (!button || !list) {
    return;
  }

  button.addEventListener('click', async () => {
    button.disabled = true;
    const skip = Number(button.dataset.skip || 0);
    const result = await apiRequest(`/api/feed/load-more?skip=${skip}&take=5`);

    if (!result.status || !result.data.length) {
      button.textContent = 'Đã xem hết';
      return;
    }

    list.insertAdjacentHTML('beforeend', result.data.map(buildLoadMorePostHtml).join(''));
    button.dataset.skip = String(skip + result.data.length);
    button.disabled = false;

    bindLikeButtons();
    bindRepostButtons();
  });
}

document.addEventListener('DOMContentLoaded', () => {
  bindLikeButtons();
  bindRepostButtons();
  bindFollowButtons();
  bindCommentForm();
  bindSearchSuggestions();
  bindLoadMore();
});
