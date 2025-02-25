using MediatR;
using src.Commands;
using src.Contexts.Database.Entities;
using src.Contexts.SharedContexts.Abstracts;

namespace src.Handlers
{
    /// <summary>
    /// 一般使用者傳送訊息後，把訊息存到 DB
    /// </summary>
    public class SendCustomerMessageCommandDbHandler : IRequestHandler<SendCustomerMessageCommand>
    {
        private readonly ISharedAuthorizedContext _sharedAuthorizedContext;
        private DatabaseContext Database { get; init; }

        /// <summary>
        /// 取得實例
        /// </summary>
        public SendCustomerMessageCommandDbHandler(
            DatabaseContext databaseContext,
            ISharedAuthorizedContext sharedAuthorizedContext
        )
        {
            _sharedAuthorizedContext = sharedAuthorizedContext;
            Database = databaseContext;
        }

        /// <inheritdoc />
        public async Task Handle(SendCustomerMessageCommand command, CancellationToken cancellationToken)
        {
            // 1. 把訊息寫入 CustomerMessage 表
            ChatMessage message = new() {
                Content = command.Message,
                CreatedTime = command.CreatedTime,
                AccountId = _sharedAuthorizedContext.AccountId
            };

            await Database.AddAsync(message, cancellationToken);
            await Database.SaveChangesAsync(cancellationToken);
        }
    }
}