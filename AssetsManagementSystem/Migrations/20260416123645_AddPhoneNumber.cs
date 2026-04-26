using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetsManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "e6cb5cbd-5c78-4879-b379-e590e2012369");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "a7b99195-91b9-4617-8b6d-47ad38efd7af");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "aac9b764-9c29-4f37-af25-a49844b3b079");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "ac40824e-d1e4-4ae2-b34b-bf66f1fcf634");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2026, 4, 16, 14, 36, 44, 320, DateTimeKind.Local).AddTicks(3864), "e4c2f1dc-afce-4168-8c02-87aae7635ee0", "AQAAAAIAAYagAAAAEGFDOOe/rhYXbWriCECchdw2aLHeIDhd+cC9RtBKFDNqo9mT+9nSS9ccQ8SEORq/dA==", "bd84d9f0-07de-478f-ace4-4ad5c01bdd2e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "45a82529-06e4-4c2f-84c9-652ca0ca2da3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "9b262d5d-1bd4-4a88-bb51-b7ab5e1229ed");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "661adb26-6acd-4045-a288-8f80d9550a5a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "c0b0ad83-ecdd-4e6b-b2b7-aceb56fdcb55");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2026, 2, 1, 3, 34, 59, 423, DateTimeKind.Local).AddTicks(7460), "675ad6d9-7b04-431e-a5c5-8b8cd8ab0c32", "AQAAAAIAAYagAAAAEOTdcDLy+rL4ziJObEPmVL7qH/BTkR02IZs+VLKvOlPGUVtLTfHpbbY6mVKuDWrE1A==", "86e87517-7ba2-41a7-93e1-a09e632605c0" });
        }
    }
}
