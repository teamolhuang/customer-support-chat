namespace src.Contexts.SharedContexts.Abstracts;

/// <summary>
/// 只支援登入後操作的情況。<br/>
/// 以要求為單位，在商業邏輯層設定共用參數，例如 JWT 中取出的 ID。
/// </summary>
public interface ISharedAuthorizedContext
{
    /// <summary>
    /// 使用者的 <see cref="Account.Id"/>
    /// </summary>
    int AccountId { get; set; }
}