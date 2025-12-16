using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bryx_CRM.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameColumnToBotUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make ChatId nullable
            migrationBuilder.AlterColumn<string>(
                name: "ChatId",
                table: "BotUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            // Add Username column
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "BotUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Username column
            migrationBuilder.DropColumn(
                name: "Username",
                table: "BotUsers");

            // Make ChatId required again
            migrationBuilder.AlterColumn<string>(
                name: "ChatId",
                table: "BotUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
