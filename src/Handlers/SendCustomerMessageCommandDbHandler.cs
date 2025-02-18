
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using src.Commands;
using src.Contexts.Database.Entities;

namespace src.Handlers
{
    /// <summary>
    /// 一般使用者傳送訊息後，把訊息存到 DB
    /// </summary>
    public class SendCustomerMessageCommandDbHandler : IRequestHandler<SendCustomerMessageCommand>
    {
        private DatabaseContext _database { get; init; }

        /// <summary>
        /// 取得實例
        /// </summary>
        public SendCustomerMessageCommandDbHandler(
            DatabaseContext databaseContext
        )
        {
            _database = databaseContext;
        }

        /// <inheritdoc />
        public async Task Handle(SendCustomerMessageCommand command, CancellationToken cancellationToken)
        {
            // 1. 把訊息寫入 CustomerMessage 表
            ChatMessage message = new() {
                Content = command.Message,
                CreatedTime = command.CreatedTime
            };

            await _database.AddAsync(message);
            await _database.SaveChangesAsync();
        }
    }
}