using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace src.Contexts.Database.Entities;

/// <summary>
/// 使用者帳號表
/// </summary>
[Table("Account")]
[Description("使用者帳號")]
[Index(nameof(Username), IsUnique = true, Name = "Index_Account_Username")]
public class Account
{
    /// <summary>
    /// 流水號
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Description("流水號")]
    public int Id { get; set; }

    /// <summary>
    /// 帳號
    /// </summary>
    [Description("帳號")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// 雜湊過的密碼
    /// </summary>
    [Description("雜湊過的密碼")]
    public string HashedPassword { get; set; } = null!;
}