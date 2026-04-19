<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Đăng nhập | Social Network</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css">
    <link rel="stylesheet" href="CSS/login-style.css">
</head>
<body>

    <div class="login-container">
        <h2>Xin chào!</h2>
        <p class="subtitle">Vui lòng đăng nhập để kết nối với bạn bè</p>
        
        <form action="process-login.php" method="POST">
            <div class="form-group">
                <label for="username"><i class="fa-regular fa-user"></i> Tài khoản</label>
                <input type="text" id="username" name="username" placeholder="Tên đăng nhập hoặc Email" required>
            </div>

            <div class="form-group">
                <label for="password"><i class="fa-solid fa-lock"></i> Mật khẩu</label>
                <input type="password" id="password" name="password" placeholder="Mật khẩu của bạn" required>
            </div>

            <button type="submit" class="btn-login">ĐĂNG NHẬP NGAY</button>
        </form>

        <div class="divider">
            <span>HOẶC</span>
        </div>

        <div class="extra-links">
            <a href="forgot-password.php">Quên mật khẩu?</a>
            <a href="register.php" style="color: #d69096;">Đăng ký mới</a>
        </div>

        <br>
        <a href="index.php" class="back-home"><i class="fa-solid fa-house"></i> Về trang chủ</a>
    </div>

</body>
</html>