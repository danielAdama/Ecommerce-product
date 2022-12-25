using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations
{
    public partial class AddCompanyData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "TimeCreated", "TimeUpdated" },
                values: new object[] { new DateTimeOffset(new DateTime(2022, 12, 25, 0, 33, 37, 293, DateTimeKind.Unspecified).AddTicks(8251), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2022, 12, 25, 0, 33, 37, 293, DateTimeKind.Unspecified).AddTicks(8250), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "City", "Name", "PhoneNumber", "PostalCode", "State", "StreetAddress", "TimeCreated", "TimeUpdated" },
                values: new object[] { 1L, "Abuja", "Selbolt", "+2348033108645", "900108", "Federal Capital Territory", "News Engineering, Dawaki", new DateTimeOffset(new DateTime(2022, 12, 25, 0, 33, 37, 293, DateTimeKind.Unspecified).AddTicks(8429), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2022, 12, 25, 0, 33, 37, 293, DateTimeKind.Unspecified).AddTicks(8428), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "TimeCreated", "TimeUpdated" },
                values: new object[] { new DateTimeOffset(new DateTime(2022, 12, 24, 23, 23, 30, 924, DateTimeKind.Unspecified).AddTicks(9367), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2022, 12, 24, 23, 23, 30, 924, DateTimeKind.Unspecified).AddTicks(9367), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
