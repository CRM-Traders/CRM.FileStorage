using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.FileStorage.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KycProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SessionToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastActivityTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VerificationComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModifiedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BucketName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    KycProcessId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpirationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModifiedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredFiles_KycProcesses_KycProcessId",
                        column: x => x.KycProcessId,
                        principalTable: "KycProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KycProcesses_LastActivityTime",
                table: "KycProcesses",
                column: "LastActivityTime");

            migrationBuilder.CreateIndex(
                name: "IX_KycProcesses_SessionToken",
                table: "KycProcesses",
                column: "SessionToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KycProcesses_Status",
                table: "KycProcesses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KycProcesses_UserId",
                table: "KycProcesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_ExpirationTime",
                table: "StoredFiles",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_KycProcessId",
                table: "StoredFiles",
                column: "KycProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_Status",
                table: "StoredFiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_Status_ExpirationTime",
                table: "StoredFiles",
                columns: new[] { "Status", "ExpirationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_UserId",
                table: "StoredFiles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropTable(
                name: "KycProcesses");
        }
    }
}
