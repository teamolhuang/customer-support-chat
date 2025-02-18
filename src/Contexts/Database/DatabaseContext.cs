using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using src.Contexts.Database.Entities;

/// <summary>
/// 透過 EF 存取資料庫用的 DB 類型。 
/// </summary>
public class DatabaseContext : DbContext {

    /// <summary>
    /// 預設建構式，給單元測試 DI 使用
    /// </summary>
    public DatabaseContext() : base() {

    }

    /// <summary>
    /// 預設建構式，給 DI 使用
    /// </summary>
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) {

    }

    /// <summary>
    /// 聊天訊息表
    /// </summary>
    public virtual DbSet<ChatMessage> ChatMessages { get; set; }
}