using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetsManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "0a24e54a-30b1-4e5d-97f8-22602df8ccc6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "cd52fed8-bdd8-47bc-b928-d5b756ca934c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "4077f3a5-3568-49c0-bc7b-35f42da200bc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "92aaf08c-f620-4522-947b-a53bb261b3be");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2025, 11, 3, 17, 3, 8, 550, DateTimeKind.Local).AddTicks(1343), "26ceb906-75e4-4e5d-8902-b0cf00d8ebdc", "AQAAAAIAAYagAAAAELWhiFbTwtb/4nXNziO3XA9wbfDwUM/h+pLZXzk1e2eG6vyGS+oq2LFAyovC5yV1fA==", "9cfaa9e9-e3c6-4f53-9a0c-8cddc8fb599b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "fd9c2e28-06dd-441c-98fd-19a44f5c8b02");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "c7038e68-4282-4a34-9d8a-6c219dc177cf");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "18d1bb83-beea-4dc2-b05d-946b0001429a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "562ff59c-1b76-42b5-b8cb-ef893eec09e0");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2025, 11, 3, 16, 43, 30, 480, DateTimeKind.Local).AddTicks(4174), "ff6b8d46-c7f0-425a-8e9e-929c41267aab", "AQAAAAIAAYagAAAAEKkBhp2aUYWjiJWilMEyrXXYjblCCcy7E0PrcOR+ePKnPyIqfRilOZx06IFofKGWvA==", "6e6b0e3a-69e2-44f6-9a77-17bf23634492" });
        }
    }
}
