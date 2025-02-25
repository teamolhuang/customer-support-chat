using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Migrations
{
    public partial class AddChatMessagefieldAccountId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ChatMessage",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_AccountId",
                table: "ChatMessage",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessage_Account_AccountId",
                table: "ChatMessage",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessage_Account_AccountId",
                table: "ChatMessage");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessage_AccountId",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ChatMessage");
        }
    }
}
