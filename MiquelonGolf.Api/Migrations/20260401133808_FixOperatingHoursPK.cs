using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiquelonGolf.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixOperatingHoursPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server can't alter IDENTITY inline — drop and recreate without it
            migrationBuilder.DropTable(name: "OperatingHours");

            migrationBuilder.CreateTable(
                name: "OperatingHours",
                columns: table => new
                {
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingHours", x => x.DayOfWeek);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OperatingHours");

            migrationBuilder.CreateTable(
                name: "OperatingHours",
                columns: table => new
                {
                    DayOfWeek = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingHours", x => x.DayOfWeek);
                });
        }
    }
}
