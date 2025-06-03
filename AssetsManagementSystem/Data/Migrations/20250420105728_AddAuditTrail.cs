using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetsManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditTrails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrails", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "f4750027-9ad3-4c8a-996b-0effec8424f2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "628021f8-3587-486d-bfb7-172dbe0b7b2a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "af64523e-31e3-4587-b171-1f93cc146efe");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "bb42eda4-edd6-494e-84ae-c1bc3c9b57c6");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2025, 4, 20, 12, 57, 27, 560, DateTimeKind.Local).AddTicks(6465), "a0e05d36-42d3-4a6a-9e0f-de6103792b5c", "AQAAAAIAAYagAAAAEIZNTKwowX/fM+vhPHGuIPvLM6vXVTXBECAwUrgOLbvx0GBBPZ0kc/O8H8Lve1PJYQ==", "c8ae1041-b224-4ef7-9967-962078a76630" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTrails");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "14d6061f-b887-4d8a-a6a4-c0d6da21c867");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "1470974e-8402-404c-9fbe-489d771bbaac");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "1878a7de-b47e-40a7-8b30-4e543bf9df08");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "03e86803-5f32-4797-bdfd-e37cb248a509");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2024, 9, 19, 0, 34, 28, 354, DateTimeKind.Local).AddTicks(2161), "137332af-1262-477d-a963-108f3beadc28", "AQAAAAIAAYagAAAAEEJJ12BuMil+BPaRaU4IzvxOA8hidSYDSkW11HryL9AlQkbCnXv4U6W/9s0YdEwmiA==", "970c002b-a789-4239-b995-b70a362cf589" });
        }
    }
}
