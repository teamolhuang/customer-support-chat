namespace src.Commands;

/// <summary>
/// 取得使用者訊息後，回傳的 Command 結果。
/// </summary>
public class GetCustomerMessagesCommandResult
{
    /// <summary>
    /// 訊息的集合。
    /// </summary>
    public IEnumerable<GetCustomerMessagesCommandResultMessage> Messages { get; set; } = Array.Empty<GetCustomerMessagesCommandResultMessage>();
}

/// <summary>
/// 取得使用者訊息後，回傳的 Command 結果中表示單筆訊息的子物件。
/// </summary>
public class GetCustomerMessagesCommandResultMessage
{
    /// <summary>
    /// 訊息內容。
    /// </summary>
    /// <example>您好，請問 RTX-6090 是否還有貨呢？我要買 5 張。</example>
    public string Content { get; set; } = null!;
    
    /// <summary>
    /// 建立時間。
    /// </summary>
    public DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 這筆訊息是否來自使用者。
    /// </summary>
    public bool IsFromUser { get; set; }
}