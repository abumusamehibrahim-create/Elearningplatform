using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearningPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddWorksheetItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorksheetItems_Videos_VideoId",
                table: "WorksheetItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorksheetItems",
                table: "WorksheetItems");

            migrationBuilder.RenameTable(
                name: "WorksheetItems",
                newName: "WorksheetItem");

            migrationBuilder.RenameIndex(
                name: "IX_WorksheetItems_VideoId",
                table: "WorksheetItem",
                newName: "IX_WorksheetItem_VideoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorksheetItem",
                table: "WorksheetItem",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 13, 20, 48, 54, 79, DateTimeKind.Utc).AddTicks(2088));

            migrationBuilder.AddForeignKey(
                name: "FK_WorksheetItem_Videos_VideoId",
                table: "WorksheetItem",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorksheetItem_Videos_VideoId",
                table: "WorksheetItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorksheetItem",
                table: "WorksheetItem");

            migrationBuilder.RenameTable(
                name: "WorksheetItem",
                newName: "WorksheetItems");

            migrationBuilder.RenameIndex(
                name: "IX_WorksheetItem_VideoId",
                table: "WorksheetItems",
                newName: "IX_WorksheetItems_VideoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorksheetItems",
                table: "WorksheetItems",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 13, 16, 53, 43, 94, DateTimeKind.Utc).AddTicks(1216));

            migrationBuilder.AddForeignKey(
                name: "FK_WorksheetItems_Videos_VideoId",
                table: "WorksheetItems",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
