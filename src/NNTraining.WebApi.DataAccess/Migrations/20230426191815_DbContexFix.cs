using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NNTraining.WebApi.DataAccess.Migrations
{
    public partial class DbContexFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Files_ModelId",
                table: "Files",
                column: "ModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Models_ModelId",
                table: "Files",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Models_ModelId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_ModelId",
                table: "Files");
        }
    }
}
