using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RAGA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantUserDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "TenantUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "TenantUsers");
        }
    }
}
