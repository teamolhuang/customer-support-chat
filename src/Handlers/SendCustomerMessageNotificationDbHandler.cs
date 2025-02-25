using MediatR;
using src.Commands;
using src.Contexts.Database.Entities;
using src.Contexts.SharedContexts.Abstracts;

namespace src.Handlers
{
    /// <summary>
    /// 一般使用者傳送訊息後，把訊息存到 DB
    /// </summary>
    public class SendCustomerMessageNotificationDbHandler : INotificationHandler<SendCustomerMessageNotification>
    {
        private readonly ISharedAuthorizedContext _sharedAuthorizedContext;
        private DatabaseContext Database { get; init; }

        /// <summary>
        /// 取得實例
        /// </summary>
        public SendCustomerMessageNotificationDbHandler(
            DatabaseContext databaseContext,
            ISharedAuthorizedContext sharedAuthorizedContext
        )
        {
            _sharedAuthorizedContext = sharedAuthorizedContext;
            Database = databaseContext;
        }

        /// <inheritdoc />
        public async Task Handle(SendCustomerMessageNotification notification, CancellationToken cancellationToken)
        {
            // 1. 把訊息寫入 CustomerMessage 表
            ChatMessage message = new() {
                Content = notification.Message,
                CreatedTime = notification.CreatedTime,
                AccountId = _sharedAuthorizedContext.AccountId,
                IsFromUser = true // 因為這裡是客戶使用者傳送訊息的指令，所以一定是 true。
            };

            await Database.AddAsync(message, cancellationToken);
            await Database.SaveChangesAsync(cancellationToken);
        }
    }
}