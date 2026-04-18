<?php
// Đóng gói toàn bộ logic quản lý Admin vào một Class để dễ quản lý
class AdminSystem {
    // sử dụng các mảng [] (Array) để mô phỏng dữ liệu thực tế từ Database

    // Lưu các con số tổng quan trên Dashboard
    public $stats = ['users' => '1,250', 'reports' => 12, 'posts' => 450, 'activity' => '98%'];

    // Mảng đa chiều lưu danh sách các báo cáo vi phạm
    public $reports = [
        ['id' => 101, 'user' => 'Linh Chi', 'avatar' => 'img/avatar-101.jpg', 'type' => 'Hình ảnh', 'reason' => 'Nội dung nhạy cảm', 'time' => '10 phút trước', 'status' => 'Chờ duyệt'],
        ['id' => 102, 'user' => 'Minh Nhật', 'avatar' => 'img/avatar-102.jpg', 'type' => 'Bình luận', 'reason' => 'Ngôn từ gây hấn', 'time' => '1 giờ trước', 'status' => 'Chờ duyệt'],
        ['id' => 103, 'user' => 'Hoàng Nam', 'avatar' => 'img/avatar-103.jpg', 'type' => 'Bài viết', 'reason' => 'Spam quảng cáo', 'time' => '3 giờ trước', 'status' => 'Đã xử lý'],
        ['id' => 104, 'user' => 'Bảo Trân', 'avatar' => 'img/avatar-104.jpg', 'type' => 'Tài khoản', 'reason' => 'Mạo danh', 'time' => '5 giờ trước', 'status' => 'Chờ duyệt'],
        ['id' => 105, 'user' => 'Anh Thư', 'avatar' => 'img/avatar-105.jpg', 'type' => 'Hình ảnh', 'reason' => 'Vi phạm bản quyền', 'time' => 'Hôm qua', 'status' => 'Đã xử lý'],
    ];

    // Mảng đa chiều lưu danh sách thành viên
    public $members = [
        ['name' => 'Khánh Linh', 'avatar' => 'img/avatar-106.jpg', 'role' => 'Thành viên', 'joined' => '12/04/2026', 'status' => 'Hoạt động'],
        ['name' => 'Bảo Hân', 'avatar' => 'img/avatar-107.jpg', 'role' => 'Quản trị viên', 'joined' => '01/01/2026', 'status' => 'Hoạt động'],
        ['name' => 'Người tình mùa đông', 'avatar' => 'img/avatar-banned.jpg', 'role' => 'Thành viên', 'joined' => '18/04/2026', 'status' => 'Đã khóa'],
    ];

    // Hàm này để tự động hóa việc render (hiển thị) giao diện dựa trên dữ liệu
    
    public function renderStatus($status) {
        // Cấu trúc điều kiện: Chọn class CSS dựa trên trạng thái
        // Nếu là 'Chờ duyệt' hoặc 'Đã khóa' thì gán class 'status-pending' (màu hồng), ngược lại là 'status-resolved' (màu nâu)
        $class = ($status == 'Chờ duyệt' || $status == 'Đã khóa') ? 'status-pending' : 'status-resolved';
        // Trả về một chuỗi HTML hoàn chỉnh để in ra giao diện
        return "<span class='badge rounded-pill px-3 py-2 $class'>$status</span>";
    }
}
// Khởi tạo đối tượng $admin từ lớp AdminSystem để bắt đầu sử dụng các dữ liệu và hàm bên trên
$admin = new AdminSystem();
?>

<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Archive - Management Center</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Playfair+Display:wght@700;800&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="CSS/style.css">
    <link rel="stylesheet" href="CSS/admin-style.css">
</head>

<body class="admin-body">

    <!-- HEADER -->
    <header class="archive-header">
        <div class="container-fluid px-4 px-lg-5">
            <div class="row align-items-center py-3">

                <!-- LEFT -->
                <div class="col-4 d-flex align-items-center">
                    <div class="brand-logo">ARCHIVE</div>
                </div>

                <!-- CENTER -->
                <div class="col-4 d-flex justify-content-center align-items-center">
                    <div class="header-badge">
                        <i class="bi bi-stars"></i>
                    </div>
                </div>

                <!-- RIGHT -->
                <div class="col-4 d-flex justify-content-end align-items-center gap-3">
                    <div class="d-none d-md-flex align-items-center gap-2 me-2">
                        <span class="text-muted small fw-bold">Quản trị viên</span>
                        <div class="admin-profile-icon">
                            <i class="bi bi-person-badge-fill"></i>
                        </div>
                    </div>

                    <button id="logoutBtn" class="header-logout-btn">
                        <i class="bi bi-box-arrow-right"></i> <span>Đăng xuất</span>
                    </button>
                </div>
            </div>
        </div>
    </header>

    <!-- py-5 tạo khoảng trống phía trên và dưới cho trang web trông thoáng đãng, không bị ngộp -->
    <main class="container py-5">
        <div class="text-center mb-5">
            <h1 class="management-title">Trung tâm điều khiển</h1>
            <p class="management-subtitle">Nơi điều phối và lưu giữ những khoảnh khắc của Archive.</p>
        </div>

        <div class="d-flex justify-content-center mb-5">
            <!-- nav-pills: Một thành phần của Bootstrap giúp tạo ra các nút bấm dạng "viên thuốc" -->
            <ul class="nav nav-pills custom-admin-tabs" id="adminTab" role="tablist">
                <li class="nav-item">
                    <!-- data-bs-toggle="tab": là thuộc tính quan trọng nhất. Nó ra lệnh cho JavaScript của Bootstrap không được load lại trang khi bấm nút này -->
                    <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#overview"><i class="bi bi-grid-1x2 me-2"></i>Tổng quan</button>
                </li>
                <li class="nav-item">
                    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#reports"><i class="bi bi-shield-check me-2"></i>Kiểm duyệt</button>
                </li>
                <li class="nav-item">
                    <button class="nav-link" data-bs-toggle="tab" data-bs-target="#members"><i class="bi bi-person-badge me-2"></i>Thành viên</button>
                </li>
            </ul>
        </div>

        <div class="tab-content">
            <div class="tab-pane fade show active" id="overview">
                <!-- row g-4: Tạo một hàng với khoảng cách giữa các cột (gutter) là mức 4
                col-md-3: Chia màn hình thành 4 cột đều nhau (vì Bootstrap có tổng 12 cột, 12/3 = 4)
                align-items-stretch: Đảm bảo tất cả các card trong hàng luôn cao bằng nhau, dù chữ bên trong ngắn hay dài. -->
                <div class="row g-4 d-flex align-items-stretch">
                    <div class="col-md-3">
                        <div class="admin-stat-card">
                            <i class="bi bi-people mb-3"></i>
                            <span class="stat-label">Thành viên</span>
                            <!-- php echo $admin->stats['users'];: Đây là kỹ thuật đổ dữ liệu động. Thay vì viết tay con số, mình gọi từ đối tượng $admin ra. Nếu sau này dữ liệu trong Database thay đổi, con số này sẽ tự cập nhật theo. -->
                            <h2 class="stat-value"><?php echo $admin->stats['users']; ?></h2>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="admin-stat-card">
                            <i class="bi bi-exclamation-octagon mb-3 text-danger"></i>
                            <span class="stat-label">Báo cáo mới</span>
                            <h2 class="stat-value text-danger"><?php echo $admin->stats['reports']; ?></h2>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="admin-stat-card">
                            <i class="bi bi-file-earmark-post mb-3"></i>
                            <span class="stat-label">Bài viết</span>
                            <h2 class="stat-value"><?php echo $admin->stats['posts']; ?></h2>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="admin-stat-card">
                            <i class="bi bi-heart-pulse mb-3 pink-icon"></i>
                            <span class="stat-label">Hoạt động</span>
                            <h2 class="stat-value"><?php echo $admin->stats['activity']; ?></h2>
                        </div>
                    </div>
                </div>
            </div>

            <div class="tab-pane fade" id="reports">
                <div class="admin-table-container">
                    <table class="table align-middle">
                        <thead>
                            <tr>
                                <th>Đối tượng</th>
                                <th>Lý do vi phạm</th>
                                <th>Thời gian</th>
                                <th>Trạng thái</th>
                                <th class="text-end">Hành động</th>
                            </tr>
                        </thead>
                        <tbody>
                            <?php foreach($admin->reports as $r): ?>
                            <!-- Đây là vòng lặp. Nó sẽ tự động đi qua từng hàng dữ liệu trong mảng reports
                            Có bao nhiêu báo cáo vi phạm thì nó sẽ tự tạo ra bấy nhiêu dòng <tr> tương ứng -->
                            <tr>
                                <td>
                                    <div class="d-flex align-items-center">
                                        <div class="me-3">
                                            <img src="<?php echo $r['avatar']; ?>" alt="avatar"  class="rounded-circle" style="width: 40px; height: 40px; object-fit: cover;">  
                                        </div>
                                        <div>
                                            <h6 class="mb-0 fw-bold"><?php echo $r['user']; ?></h6>
                                            <small class="text-muted"><?php echo $r['type']; ?></small>
                                        </div>
                                    </div>
                                </td>
                                <td><span class="small"><?php echo $r['reason']; ?></span></td>
                                <td class="small text-muted"><?php echo $r['time']; ?></td>
                                <td><?php echo $admin->renderStatus($r['status']); ?></td>
                                <td class="text-end">
                                    <button class="btn btn-pink-admin" 
                                        <?php echo ($r['status'] == 'Đã xử lý') ? 'disabled style="opacity: 0.7;"' : ''; ?>>
                                        <?php echo ($r['status'] == 'Đã xử lý') ? '<i class="bi bi-check2-all"></i> Hoàn tất' : 'Xử lý'; ?>
                                    </button>
                                </td>
                            </tr>
                            <?php endforeach; ?>
                        </tbody>
                    </table>
                </div>
            </div>

            <div class="tab-pane fade" id="members">
                <div class="admin-table-container">
                    <table class="table align-middle">
                        <thead>
                            <tr>
                                <th>Thành viên</th>
                                <th>Vai trò</th>
                                <th>Tham gia</th>
                                <th>Trạng thái</th>
                                <th class="text-end">Thao tác</th>
                            </tr>
                        </thead>
                        <tbody>
                            <?php foreach($admin->members as $m): ?>
                            <tr>
                                <td>
                                    <div class="d-flex align-items-center">
                                        <div class="me-3">
                                            <img src="<?php echo $m['avatar']; ?>" 
                                                alt="avatar" 
                                                class="rounded-circle border" 
                                                style="width: 40px; height: 40px; object-fit: cover; border-color: rgba(121, 91, 74, 0.15) !important;">
                                        </div>
                                        <div class="fw-bold"><?php echo $m['name']; ?></div>
                                    </div>
                                </td>
                                <td class="small text-muted"><?php echo $m['role']; ?></td>
                                <td class="small"><?php echo $m['joined']; ?></td>
                                <td><?php echo $admin->renderStatus($m['status']); ?></td>
                                <td class="text-end"><button class="btn btn-outline-brown">Sửa</button></td>
                            </tr>
                            <?php endforeach; ?>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </main>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="Script/admin-script.js"></script>
</body>
</html>