using MediatR;

namespace src.Commands
{
    /// <summary>
    /// 使用者傳送訊息到 DB。
    /// </summary>
    public class SendCustomerMessageCommand : IRequest
    {
        /// <summary>
        /// 訊息內容
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// 此訊息的建立時間。
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}