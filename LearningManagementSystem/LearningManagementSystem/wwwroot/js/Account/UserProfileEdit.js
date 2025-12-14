// Hàm này được gọi bởi nút 'onclick="confirmDelete()"'
function confirmDelete() {
    Swal.fire({
        title: 'Bạn có chắc chắn?',
        text: "Bạn sẽ không thể hoàn tác hành động này! Tất cả dữ liệu của bạn sẽ bị xóa vĩnh viễn.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#e53e3e', // Màu đỏ (danger)
        cancelButtonColor: '#718096',  // Màu xám (secondary)
        confirmButtonText: 'Vâng, xóa tài khoản!',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            // Nếu người dùng đồng ý, tìm form và submit
            const form = document.getElementById('deleteAccountForm');
            if (form) {
                form.submit();
            } else {
                Swal.fire('Lỗi!', 'Không tìm thấy form xóa. Vui lòng tải lại trang.', 'error');
            }
        }
    });
}