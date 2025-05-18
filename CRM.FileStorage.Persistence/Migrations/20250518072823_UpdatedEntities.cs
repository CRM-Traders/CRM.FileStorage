using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.FileStorage.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StoredFiles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "StoredFiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_Reference",
                table: "StoredFiles",
                column: "Reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_Reference",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "StoredFiles");
        }
    }
}
