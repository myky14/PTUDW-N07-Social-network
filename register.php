<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Đăng ký thành viên | Social Network</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css">
    <link rel="stylesheet" href="CSS/login-style.css">
    <style>
        /* Tinh chỉnh thêm một chút cho trang đăng ký vì form dài hơn */
        .login-container {
            max-width: 460px; /* Cho rộng hơn một tí để dàn hàng icon đẹp hơn */
            margin: 20px;
        }
        .register-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            text-align: left;
        }
        .full-width {
            grid-column: span 2;
        }
    </style>
</head>
<body>

    <div class="login-container">
        <h2>Tham gia cùng chúng mình!</h2>
        <p class="subtitle">Tạo tài khoản để kết nối và chia sẻ ngay</p>
        
        <form action="process-register.php" method="POST">
            <div class="register-grid">
                <div class="form-group full-width">
                    <label for="fullname">Họ và Tên</label>
                    <input type="text" id="fullname" name="fullname" placeholder="Nguyễn Văn A" required>
                </div>

                <div class="form-group">
                    <label for="username">Tài khoản</label>
                    <input type="text" id="username" name="username" placeholder="user123" required>
                </div>

                <div class="form-group">
                    <label for="email">Email</label>
                    <input type="email" id="email" name="email" placeholder="abc@gmail.com" required>
                </div>

                <div class="form-group">
                    <label for="password">Mật khẩu</label>
                    <input type="password" id="password" name="password" placeholder="••••••••" required>
                </div>

                <div class="form-group">
                    <label for="confirm_password">Xác nhận</label>
                    <input type="password" id="confirm_password" name="confirm_password" placeholder="••••••••" required>
                </div>
            </div>

            <button type="submit" class="btn-login" style="margin-top: 10px;">TẠO TÀI KHOẢN</button>
        </form>

        <div class="divider">
            <span>HOẶC</span>
        </div>

        <div class="extra-links" style="justify-content: center;">
            <p>Đã có tài khoản? <a href="login.php" style="color: var(--primary-color); margin-left: 5px;">Đăng nhập ngay</a></p>
        </div>

        <a href="index.php" class="back-home"><i class="fa-solid fa-house"></i> Về trang chủ</a>
    </div>

</body>
</html>