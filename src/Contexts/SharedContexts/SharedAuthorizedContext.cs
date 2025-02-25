using src.Contexts.Database.Entities;
using src.Contexts.SharedContexts.Abstracts;

namespace src.Contexts.SharedContexts;

/// <inheritdoc />
public class SharedAuthorizedContext : ISharedAuthorizedContext
{
    /// <summary>
    /// 使用者的 <see cref="Account.Id"/>
    /// </summary>
    public int AccountId { get; set; }
}