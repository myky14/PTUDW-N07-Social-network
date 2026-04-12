# ARCHIVE

ARCHIVE là đồ án môn học chủ đề mạng xã hội, lấy cảm hứng từ Threads của Meta nhưng rút gọn phạm vi để phù hợp demo, báo cáo và chấm điểm môn học.

Ứng dụng được xây dựng theo hướng:
- ASP.NET Core MVC
- OOP + MVC rõ ràng
- Bootstrap làm nền tảng giao diện
- AJAX + JSON cho các tương tác chính
- Database thực tế bằng SQLite + EF Core

## 1. Stack được chọn

### Lựa chọn chính
- Backend: ASP.NET Core MVC (.NET 8)
- Frontend: Razor Views + Bootstrap 5
- ORM: Entity Framework Core
- Database: SQLite
- Auth: Cookie Authentication
- AJAX: Fetch API trả JSON

### Vì sao stack này phù hợp
- Đúng yêu cầu giảng viên: MVC, OOP, Bootstrap, database thật, JSON/AJAX
- Dễ chạy trong VS Code, không cần setup SQL Server phức tạp
- Source code rõ lớp, dễ trình bày trong báo cáo
- Dễ seed dữ liệu demo để feed không bị trống
- Dễ chia việc theo branch cho nhóm

## 2. Kiến trúc tổng thể

### Cấu trúc thư mục
```text
ARCHIVE.sln
Archive.Web/
  Areas/Admin/
    Controllers/
    Views/
  Controllers/
  Data/
  Extensions/
  Models/
  Services/
  ViewModels/
  Views/
  wwwroot/
    css/
    js/
    images/
```

### Danh sách Models
- `Role`
- `AppUser`
- `Topic`
- `Post`
- `PostImage`
- `Comment`
- `Like`
- `Follow`
- `Repost`
- `Hashtag`
- `PostHashtag`
- `Report`
- `AdminLog`

### Danh sách Controllers
- `HomeController`
- `AccountController`
- `FeedController`
- `PostsController`
- `ProfilesController`
- `SearchController`
- `ReportsController`
- `ApiController`

### Controllers trong Admin Area
- `DashboardController`
- `UsersController`
- `PostsController`
- `TopicsController`

### Danh sách Services
- `AuthService`
- `FileStorageService`
- `PostViewModelFactory`
- `FeedService`
- `PostService`
- `ProfileService`
- `SearchService`
- `AdminService`
- `ReportService`

### Luồng người dùng
1. Khách truy cập landing page hoặc about page
2. Đăng ký hoặc đăng nhập
3. Vào feed
4. Đăng bài, like, comment, repost, quote
5. Xem profile, chỉnh sửa profile, follow người khác
6. Tìm kiếm người dùng hoặc bài viết
7. Báo cáo bài viết hoặc tài khoản

### Luồng admin
1. Đăng nhập bằng tài khoản admin
2. Vào `Admin/Dashboard`
3. Xem thống kê cơ bản
4. Quản lý user
5. Quản lý bài viết
6. Quản lý topic
7. Khóa user hoặc ẩn bài viết vi phạm

## 3. Thiết kế database

### ERD mô tả bằng text
```text
Roles (1) ---- (n) Users
Users (1) ---- (n) Posts
Topics (1) ---- (n) Posts
Posts (1) ---- (n) PostImages
Posts (1) ---- (n) Comments
Users (1) ---- (n) Comments
Posts (1) ---- (n) Likes
Users (1) ---- (n) Likes
Users (1) ---- (n) Follows [Follower]
Users (1) ---- (n) Follows [Following]
Posts (1) ---- (n) Reposts
Users (1) ---- (n) Reposts
Posts (n) ---- (n) Hashtags qua PostHashtags
Users (1) ---- (n) Reports [Reporter]
Users (1) ---- (n) Reports [TargetUser]
Posts (1) ---- (n) Reports [TargetPost]
Users (1) ---- (n) AdminLogs
Posts (1) ---- (n) Posts [QuotePost self-reference]
Comments (1) ---- (n) Comments [Reply self-reference]
```

### Các bảng chính và thuộc tính

#### `Roles`
- `Id`
- `Name`
- `Description`

#### `Users`
- `Id`
- `RoleId`
- `UserName`
- `DisplayName`
- `Email`
- `PasswordHash`
- `Bio`
- `AvatarUrl`
- `IsActive`
- `IsLocked`
- `CreatedAt`
- `UpdatedAt`

#### `Topics`
- `Id`
- `Name`
- `Slug`
- `Description`
- `IsVisible`

#### `Posts`
- `Id`
- `UserId`
- `TopicId`
- `QuotePostId`
- `Content`
- `IsHidden`
- `IsDeleted`
- `CreatedAt`
- `UpdatedAt`

#### `PostImages`
- `Id`
- `PostId`
- `ImageUrl`
- `DisplayOrder`

#### `Comments`
- `Id`
- `PostId`
- `UserId`
- `ParentCommentId`
- `Content`
- `IsHidden`
- `CreatedAt`

#### `Likes`
- `Id`
- `PostId`
- `UserId`
- `CreatedAt`

#### `Follows`
- `Id`
- `FollowerId`
- `FollowingId`
- `CreatedAt`

#### `Reposts`
- `Id`
- `PostId`
- `UserId`
- `CreatedAt`

#### `Hashtags`
- `Id`
- `Name`
- `Slug`

#### `PostHashtags`
- `PostId`
- `HashtagId`

#### `Reports`
- `Id`
- `ReporterUserId`
- `TargetUserId`
- `TargetPostId`
- `Reason`
- `Details`
- `Status`
- `ReviewedByUserId`
- `CreatedAt`
- `ReviewedAt`

#### `AdminLogs`
- `Id`
- `AdminUserId`
- `Action`
- `EntityName`
- `EntityId`
- `Note`
- `CreatedAt`

## 4. Chức năng đã hoàn thành

### Khách chưa đăng nhập
- Landing page
- About page
- Đăng ký
- Đăng nhập
- Xem preview feed ở landing page

### Người dùng đăng nhập
- Xem home feed
- Tạo bài viết mới
- Upload 1 ảnh cho bài viết
- Sửa bài viết của mình
- Xóa mềm bài viết của mình
- Like / unlike bằng AJAX + JSON
- Repost / bỏ repost bằng AJAX + JSON
- Xem chi tiết bài viết
- Bình luận bằng AJAX + JSON
- Load lại comment bằng AJAX + JSON
- Quote bài viết
- Theo dõi / bỏ theo dõi bằng AJAX + JSON
- Xem hồ sơ cá nhân
- Chỉnh sửa hồ sơ: display name, bio, avatar
- Tìm kiếm user và post
- Search suggestion bằng AJAX + JSON
- Báo cáo bài viết
- Báo cáo tài khoản

### Quản trị
- Đăng nhập admin
- Dashboard thống kê cơ bản
- Quản lý user
- Khóa / mở khóa user
- Quản lý post
- Ẩn / hiện bài viết
- Quản lý topic
- Thêm / sửa topic

## 5. AJAX + JSON đang dùng ở đâu
- `POST /api/like/{postId}`
- `POST /api/repost/{postId}`
- `POST /api/follow/{targetUserId}`
- `GET /api/comments/{postId}`
- `POST /api/comments/{postId}`
- `GET /api/suggestions?q=...`
- `GET /api/feed/load-more?skip=...&take=...`

Mỗi endpoint đều trả về:
- `status`
- `message`
- `data`

## 6. Seed data và tài khoản demo

### Tài khoản mặc định
- Admin:
  - Email: `admin@archive.local`
  - Password: `Admin@123`
- User:
  - Email: `luna@archive.local`
  - Password: `User@123`
- User:
  - Email: `mimo@archive.local`
  - Password: `User@123`
- User:
  - Email: `ivy@archive.local`
  - Password: `User@123`
- User:
  - Email: `nami@archive.local`
  - Password: `User@123`

### Dữ liệu mẫu
- Có sẵn topic
- Có sẵn user
- Có sẵn bài viết
- Có sẵn ảnh minh họa SVG
- Có sẵn like, follow, comment, repost
- Có sẵn report mẫu để admin demo

## 7. Hướng dẫn chạy local

### Yêu cầu
- .NET SDK 8.0

### Các bước chạy
1. Mở terminal tại thư mục project
2. Chạy:

```bash
dotnet restore ARCHIVE.sln
dotnet run --project Archive.Web/Archive.Web.csproj
```

3. Mở trình duyệt theo URL mà ASP.NET Core in ra

### Ghi chú database
- Ứng dụng dùng SQLite
- File database `archive.db` sẽ được tạo tự động ở lần chạy đầu tiên
- Seed data cũng được nạp tự động khi database chưa có dữ liệu

## 8. Bảo mật và validation cơ bản
- Cookie authentication
- Phân quyền theo role `User` và `Admin`
- Chỉ tác giả mới sửa/xóa bài viết của mình
- Chỉ admin mới vào khu vực `Admin`
- Có validation cho form đăng nhập, đăng ký, post, profile, topic
- Giới hạn độ dài nội dung bài viết
- Kiểm tra upload ảnh cơ bản theo dung lượng và phần mở rộng

## 9. Tính năng chưa làm hoặc làm ở mức placeholder
- Notification center đầy đủ
- Quản lý comment riêng trong admin
- Duyệt report nâng cao theo workflow
- Story
- Chat
- Livestream
- Realtime websocket
- Reels/video short feed
- Recommendation engine

## 10. Điểm nhấn để thuyết trình
- MVC + OOP rõ ràng
- Có service layer riêng, controller không nhồi business logic
- Có database thật và seed data
- Có admin area
- Có AJAX JSON đúng yêu cầu môn học
- Giao diện Bootstrap nhưng được custom theo phong cách pastel, mềm, nữ tính
- Dễ chia branch GitHub và demo theo nhóm

## 11. Lưu ý khi nộp đồ án
- Nếu giảng viên yêu cầu SQL Server thay vì SQLite, có thể chuyển từ `UseSqlite` sang `UseSqlServer` và cập nhật connection string
- Với phạm vi môn học, bản hiện tại ưu tiên chạy ổn, dễ demo, dễ giải thích hơn là tối ưu production
# su_threads
