using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace COMP9034.Backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Device",
                columns: new[] { "DeviceId", "CreatedAt", "DeviceName", "DeviceType", "IpAddress", "IsActive", "Location", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 9, 17, 15, 57, 3, 924, DateTimeKind.Utc).AddTicks(8040), "Main Terminal", "terminal", "192.168.1.100", true, "Main Entrance", "Active", new DateTime(2025, 9, 17, 15, 57, 3, 924, DateTimeKind.Utc).AddTicks(8040) },
                    { 2, new DateTime(2025, 9, 17, 15, 57, 3, 924, DateTimeKind.Utc).AddTicks(8040), "Biometric Scanner", "biometric", "192.168.1.101", true, "Security Office", "Active", new DateTime(2025, 9, 17, 15, 57, 3, 924, DateTimeKind.Utc).AddTicks(8040) }
                });

            migrationBuilder.InsertData(
                table: "Staff",
                columns: new[] { "StaffId", "Address", "ContractType", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "OvertimePayRate", "PasswordHash", "Phone", "Pin", "Role", "StandardHoursPerWeek", "StandardPayRate", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1001, null, "Casual", new DateTime(2025, 9, 17, 15, 57, 3, 924, DateTimeKind.Utc).AddTicks(7690), "worker@farmtimems.com", "Farm", true, "Worker", 37.50m, "$2a$11$qB30P0mxt3bESfOwYrBCv.XumZ8UjsiXfkQupgMrl9WrvHfANhPWe", null, "1001", "Staff", 20, 25.00m, new DateTime(2025, 9, 17, 15, 57, 3, 924, DateTimeKind.Utc).AddTicks(7690), "worker" },
                    { 8001, null, "FullTime", new DateTime(2025, 9, 17, 15, 57, 3, 790, DateTimeKind.Utc).AddTicks(5710), "manager@farmtimems.com", "Farm", true, "Manager", 52.50m, "$2a$11$Qti/DzU6MhFc6Cz4dDGl4.D5xWkHQY5RG7E2EKMatI3tmOB4cSjae", null, "8001", "Manager", 40, 35.00m, new DateTime(2025, 9, 17, 15, 57, 3, 790, DateTimeKind.Utc).AddTicks(5710), "manager" },
                    { 9001, null, "FullTime", new DateTime(2025, 9, 17, 15, 57, 3, 656, DateTimeKind.Utc).AddTicks(3880), "admin@farmtimems.com", "System", true, "Administrator", 75.00m, "$2a$11$nztUxg5SMp5Be9UnjOipBeUmeYIxMUleUVf.J/GE38ioHXA8j1AF6", null, "1234", "Admin", 40, 50.00m, new DateTime(2025, 9, 17, 15, 57, 3, 656, DateTimeKind.Utc).AddTicks(3880), "admin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Device",
                keyColumn: "DeviceId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Device",
                keyColumn: "DeviceId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Staff",
                keyColumn: "StaffId",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "Staff",
                keyColumn: "StaffId",
                keyValue: 8001);

            migrationBuilder.DeleteData(
                table: "Staff",
                keyColumn: "StaffId",
                keyValue: 9001);
        }
    }
}
