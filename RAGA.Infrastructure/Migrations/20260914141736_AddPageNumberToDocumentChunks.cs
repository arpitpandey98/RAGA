using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RAGA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPageNumberToDocumentChunks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PageNumber",
                table: "DocumentChunks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageNumber",
                table: "DocumentChunks");
        }
    }
}
