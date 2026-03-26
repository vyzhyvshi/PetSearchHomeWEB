using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetSearchHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDomainIdAndDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure gen_random_uuid() is available for default values
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");

            migrationBuilder.AddColumn<Guid>(
                name: "domain_id",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE users SET domain_id = gen_random_uuid() WHERE domain_id IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_users_domain_id",
                table: "users",
                column: "domain_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_domain_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "domain_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "display_name",
                table: "users");
        }
    }
}
