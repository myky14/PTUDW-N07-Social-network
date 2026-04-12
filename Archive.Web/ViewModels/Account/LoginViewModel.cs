using System.ComponentModel.DataAnnotations;

namespace Archive.Web.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Hãy nhập email.")]
    [EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy nhập mật khẩu.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
