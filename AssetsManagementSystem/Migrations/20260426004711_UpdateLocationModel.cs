using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetsManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLocationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Locations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentLocationId",
                table: "Locations",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "8440e498-9895-42b0-911b-25b8b95f2ca8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "4f08ade6-6aee-4092-b957-44098a8af3fb");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "aa9365aa-35be-41e4-bb57-74bf977477ad");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "32b7b126-a5d5-44ad-b0ad-35de349fc7bb");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2026, 4, 26, 3, 47, 11, 244, DateTimeKind.Local).AddTicks(4483), "241e40ea-b34a-4c3b-9ff1-9e94cc9d6c39", "AQAAAAIAAYagAAAAEL5meMeahyA1fiAwUqnl5EyjFV/U/Snnms0Y941cLAy+5m8nct7lGlLlAEBVTHlM4g==", "f23524b9-da8c-4171-9645-8b5700c1bd1b" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ParentLocationId",
                table: "Locations",
                column: "ParentLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Locations_ParentLocationId",
                table: "Locations",
                column: "ParentLocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Locations_ParentLocationId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_ParentLocationId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ParentLocationId",
                table: "Locations");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("62474474-f91b-483d-b0d8-2742c01146f0"),
                column: "ConcurrencyStamp",
                value: "eb8b3378-1458-459d-a392-37607d42d483");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("846e3679-1537-487d-969c-3a6116fc3b2d"),
                column: "ConcurrencyStamp",
                value: "da26b4eb-6110-4028-a915-714fc624e17d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d9c0c478-adf7-40db-ade3-2b7810d9659f"),
                column: "ConcurrencyStamp",
                value: "4245f3ed-52ca-43a7-9b87-f1dd6fd98cad");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fc05f613-0e97-444e-b19b-018a223a7484"),
                column: "ConcurrencyStamp",
                value: "c3b4c337-21f0-4229-8197-c94af76fdb6c");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bdabcf06-a956-4ef7-8045-3214e68b9b4c"),
                columns: new[] { "AddedOnDate", "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { new DateTime(2026, 4, 19, 5, 43, 15, 873, DateTimeKind.Local).AddTicks(7248), "d86e598c-0d47-40c1-8464-4ceb666c967c", "AQAAAAIAAYagAAAAEFVFuTFj+/6AS3SZXvnmAzZbY02TdyIxKOQnBRsoJ3oLm7TPR+1IcFIoxtcTfgQniA==", "feb871e9-42da-43af-b262-c93743c8b837" });
        }
    }
}
