using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetSearchHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "text",
                table: "reports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "text",
                table: "reports");
        }
    }
}
