using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace src.Contexts.Database.Entities
{
    /// <summary>
    /// 聊天室訊息的資料表
    /// </summary>
    [Table("ChatMessage")]
    [Index(nameof(CreatedTime), Name = "Idx_ChatMessage_CreatedTime")]
    public class ChatMessage
    {
        /// <summary>
        /// 流水號
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 訊息內容
        /// </summary>
        [MaxLength(200)]
        public string Content { get; set; } = null!;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 這筆資料對應的使用者帳號 ID。
        /// </summary>
        public int? AccountId { get; set; }
        
        /// <summary>
        /// 與這筆資料關聯的使用者帳號。
        /// </summary>
        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }
        
    }
}