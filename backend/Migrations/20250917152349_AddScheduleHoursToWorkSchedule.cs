using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COMP9034.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleHoursToWorkSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ScheduleHours",
                table: "WorkSchedule",
                type: "numeric(8,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleHours",
                table: "WorkSchedule");
        }
    }
}
