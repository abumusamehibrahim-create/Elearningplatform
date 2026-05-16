using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearningPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddPlainPasswordColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlainPassword",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "Title" },
                values: new object[] { new DateTime(2026, 4, 10, 3, 6, 16, 576, DateTimeKind.Utc).AddTicks(9607), "تعلم بناء نفسك بشكل احترافية باستخدام الفديوهات = مستقبلك بين يديك", "دورة في مادة الرياضيات الشاملة" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlainPassword",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "Title" },
                values: new object[] { new DateTime(2026, 4, 7, 18, 59, 52, 992, DateTimeKind.Utc).AddTicks(9578), "تعلم بناء تطبيقات ويب احترافية باستخدام ASP.NET Core و C#", "دورة ASP.NET Core الشاملة" });
        }
    }
}
