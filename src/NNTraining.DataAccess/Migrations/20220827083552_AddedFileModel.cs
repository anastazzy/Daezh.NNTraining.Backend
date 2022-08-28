using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NNTraining.DataAccess.Migrations
{
    public partial class AddedFileModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "TimeOfCreation",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Files",
                newName: "OriginalName");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Files",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "Files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "Files",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ModelFiles",
                columns: table => new
                {
                    ModelId = table.Column<long>(type: "bigint", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelFiles", x => new { x.ModelId, x.FileId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelFiles");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "OriginalName",
                table: "Files",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Files",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "Files",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOfCreation",
                table: "Files",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
