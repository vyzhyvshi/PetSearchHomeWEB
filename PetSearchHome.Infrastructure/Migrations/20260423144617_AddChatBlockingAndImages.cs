using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetSearchHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatBlockingAndImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "chat_messages",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "chat_blocks",
                columns: table => new
                {
                    chat_block_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    blocker_id = table.Column<int>(type: "integer", nullable: false),
                    blocked_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_blocks", x => x.chat_block_id);
                    table.ForeignKey(
                        name: "FK_chat_blocks_users_blocked_id",
                        column: x => x.blocked_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chat_blocks_users_blocker_id",
                        column: x => x.blocker_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chat_blocks_blocked_id",
                table: "chat_blocks",
                column: "blocked_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_blocks_blocker_id_blocked_id",
                table: "chat_blocks",
                columns: new[] { "blocker_id", "blocked_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chat_blocks");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "chat_messages");
        }
    }
}
