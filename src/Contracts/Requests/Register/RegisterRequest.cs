using System.ComponentModel.DataAnnotations;

namespace src.Contracts.Requests.Register;

/// <summary>
/// 註冊時的傳入物件。
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// 使用者名稱。必填，8~20 碼，只允許半形英數字。
    /// </summary>
    /// <example>userExample</example>
    [Required]
    [MinLength(8)]
    [MaxLength(20)]
    [RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// 密碼。必填，8~20 碼，只允許半形英數字、半形底線、半形 hyphen、半形句點。
    /// </summary>
    [Required]
    [MinLength(8)]
    [MaxLength(20)]
    [RegularExpression(@"^[a-zA-Z0-9_\-\.]+$")]
    public string Password { get; set; } = null!;
}