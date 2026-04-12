using System.ComponentModel.DataAnnotations;

namespace Archive.Web.ViewModels.Profile;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Hãy nhập tên hiển thị.")]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(260)]
    public string? Bio { get; set; }

    public IFormFile? AvatarFile { get; set; }

    public string? CurrentAvatarUrl { get; set; }
}
