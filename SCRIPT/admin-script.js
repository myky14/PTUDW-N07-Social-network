/**
 * ARCHIVE ADMIN SCRIPT
 * Tập trung vào xử lý DOM và tương tác người dùng
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log("Archive Management Center đã sẵn sàng!");

    // 1. XỬ LÝ NÚT "XỬ LÝ" TRONG BẢNG KIỂM DUYỆT
    const approveBtns = document.querySelectorAll('.btn-pink-admin');
    
    approveBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            // Xác định dòng (row) chứa nút vừa bấm
            const row = this.closest('tr');
            // Tìm badge trạng thái trong dòng đó
            const statusBadge = row.querySelector('.badge');
            
            // --- THAO TÁC DOM 1: Cập nhật trạng thái tại chỗ ---
            statusBadge.textContent = 'Đã xử lý';
            // Gỡ bỏ class màu hồng (status-pending)
            statusBadge.classList.remove('status-pending');
            // Thêm class màu nâu (status-resolved) vào
            statusBadge.classList.add('status-resolved');
            
            // Vô hiệu hóa nút sau khi bấm để tránh bấm nhiều lần
            this.disabled = true;
            this.innerHTML = '<i class="bi bi-check2-all"></i> Hoàn tất';
            this.style.opacity = '0.7';

            // --- THAO TÁC DOM 2: Cập nhật con số ở Tab Tổng quan ---
            // Tìm con số "Báo cáo mới" ở tab Tổng quan dựa trên class chuẩn
            const reportStatElement = document.querySelector('#overview .stat-value.text-danger'); 
            if (reportStatElement) {
                let currentCount = parseInt(reportStatElement.textContent);
                if (currentCount > 0) {
                    reportStatElement.textContent = currentCount - 1;
                    
                    // Thêm hiệu ứng nháy nhẹ cho con số khi nó thay đổi
                    reportStatElement.style.transition = '0.3s';
                    reportStatElement.style.transform = 'scale(1.2)';
                    setTimeout(() => {
                        reportStatElement.style.transform = 'scale(1)';
                    }, 300);
                }
            }

            // Thông báo nhỏ cho người dùng
            console.log("Đã xử lý thành công một mục vi phạm.");
        });
    });

    // 2. XỬ LÝ NÚT "SỬA" TRONG TAB THÀNH VIÊN
    const editBtns = document.querySelectorAll('.btn-outline-brown');
    
    editBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            // Tìm hàng (tr) chứa cái nút vừa được bấm
            const row = this.closest('tr');
            // Lấy tên thành viên (nằm trong cột đầu tiên của hàng)
            // Lưu ý: Vì mình có thêm Avatar nên cấu trúc DOM bên trong cột 1 hơi phức tạp hơn
            const userName = row.querySelector('.fw-bold').textContent;
            
            // Demo tương tác: Hiện thông báo xác nhận
            alert('Bạn đang chuẩn bị chỉnh sửa quyền hạn của: ' + userName);
            const newRole = prompt("Nhập vai trò mới cho " + userName + ":", "Thành viên");
            if (newRole) {
                row.cells[1].textContent = newRole; // Cập nhật vai trò mới vào cột thứ 2 ngay lập tức
                alert("Đã cập nhật vai trò mới thành công!");
            }
        });
    });

    // 3. HIỆU ỨNG KHI CHUYỂN TAB (Optional - Cho mượt mà hơn)
    const tabLinks = document.querySelectorAll('button[data-bs-toggle="tab"]');
    tabLinks.forEach(tab => {
        tab.addEventListener('shown.bs.tab', function (event) {
            const targetId = event.target.getAttribute('data-bs-target');
            console.log("Đã chuyển sang phân hệ: " + targetId);
        });
    });

    // 4. Xử lý sự kiện Đăng xuất
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function() {
            // Hiện thông báo xác nhận của trình duyệt
            const confirmLogout = confirm("Khoan khoan, bạn chắc chắn muốn đăng xuất chứ?");
            
            if (confirmLogout) {
                // Nếu bấm "Có" (OK), chuyển hướng về trang index.html
                window.location.href = 'index.html';
            } 
            // Nếu bấm "Không" (Cancel) thì không làm gì cả, ở lại trang admin
        });
    }
});