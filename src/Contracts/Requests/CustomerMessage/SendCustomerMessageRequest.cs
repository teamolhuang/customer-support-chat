using System.ComponentModel.DataAnnotations;

namespace src.Contracts.Requests.CustomerMessage;

/// <summary>
/// 前台使用者傳送訊息時的要求物件。
/// </summary>
public class SendCustomerMessageRequest
{
    /// <summary>
    /// 訊息內容
    /// </summary>
    /// <example>您好，請問 RTX-6090 是否還有貨呢？我要買 5 張。</example>
    [Required]
    [MaxLength(200)]
    public string Message { get; set; } = null!;
}