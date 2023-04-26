using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NNTraining.WebApi.DataAccess.Migrations
{
    public partial class DeleteManyToManyTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelFiles");

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "Files",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "Files");

            migrationBuilder.CreateTable(
                name: "ModelFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModelFiles_ModelId_FileId",
                table: "ModelFiles",
                columns: new[] { "ModelId", "FileId" },
                unique: true);
        }
    }
}
