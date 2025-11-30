using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetsManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssetTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ToUserId",
                table: "AssetTransferRecords",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "FromUserId",
                table: "AssetTransferRecords",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "c098e4f7-7d58-4117-a2b3-d72d11a0566c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "f3a52c84-2222-4a8f-aa32-1206e62d447e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "90806a8f-f299-4a46-a649-8507884c3019");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "b2e725eb-a4b5-4a24-8921-4bbad1501b72");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2025, 11, 27, 1, 56, 23, 954, DateTimeKind.Local).AddTicks(6458), "e6269cfc-6a72-4a6c-95ea-f655b3650ba0", "AQAAAAIAAYagAAAAEHuU8xOTUVVNEZM8VjCUL/nA17pq4sYmqQl5FpriPr9xuGw5dsgN/L/kZaF84LoM2A==", "3a78c9e2-05b0-47a4-863c-9d84f96f24ea" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ToUserId",
                table: "AssetTransferRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FromUserId",
                table: "AssetTransferRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "628ad151-dcf5-4069-abd4-1cd1bad3babc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "92583ce9-8afc-4bf8-a4f6-00301321cc51");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "aced1dec-090b-43e3-8afe-0a119207f5fc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "7dc47d4b-3910-456f-8239-85dd65167c02");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2025, 11, 27, 1, 39, 16, 70, DateTimeKind.Local).AddTicks(7569), "9fc07e88-6706-43f8-9ac0-c0e26d8cbdfe", "AQAAAAIAAYagAAAAEFv7MXv+7cH9JGX+THM42UzqTMRAfJtug+UyS8Pa8hvPh9hZu8BHv1G1EqNZ3Kj4tA==", "eebe4310-0484-470e-9417-b9f1127b89c1" });
        }
    }
}
