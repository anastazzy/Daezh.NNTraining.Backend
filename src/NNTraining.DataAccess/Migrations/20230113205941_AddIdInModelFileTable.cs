using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NNTraining.DataAccess.Migrations
{
    public partial class AddIdInModelFileTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelFiles",
                table: "ModelFiles");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ModelFiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelFiles",
                table: "ModelFiles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ModelFiles_ModelId_FileId",
                table: "ModelFiles",
                columns: new[] { "ModelId", "FileId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ModelFiles",
                table: "ModelFiles");

            migrationBuilder.DropIndex(
                name: "IX_ModelFiles_ModelId_FileId",
                table: "ModelFiles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ModelFiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModelFiles",
                table: "ModelFiles",
                columns: new[] { "ModelId", "FileId" });
        }
    }
}
