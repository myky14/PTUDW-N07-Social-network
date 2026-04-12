using System.ComponentModel.DataAnnotations;

namespace Archive.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Hãy nhập username.")]
    [StringLength(30, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy nhập tên hiển thị.")]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy nhập email.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy nhập mật khẩu.")]
    [StringLength(50, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy xác nhận mật khẩu.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận chưa khớp.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
