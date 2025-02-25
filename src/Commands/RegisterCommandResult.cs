namespace src.Commands;

/// <summary>
/// 處理完註冊後回傳的結果。
/// </summary>
public class RegisterCommandResult
{
    /// <summary>
    /// 建立帳號的 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 建立帳號的使用者名稱
    /// </summary>
    public string Username { get; set; } = null!;
}