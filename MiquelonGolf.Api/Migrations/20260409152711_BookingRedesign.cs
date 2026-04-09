using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiquelonGolf.Api.Migrations
{
    /// <inheritdoc />
    public partial class BookingRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StartingHole",
                table: "TeeTimeSlots",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationCode",
                table: "Bookings",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferralSource",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoundType",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TeeTimeSlots_Date_StartTime_StartingHole",
                table: "TeeTimeSlots",
                columns: new[] { "Date", "StartTime", "StartingHole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ConfirmationCode",
                table: "Bookings",
                column: "ConfirmationCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeeTimeSlots_Date_StartTime_StartingHole",
                table: "TeeTimeSlots");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ConfirmationCode",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StartingHole",
                table: "TeeTimeSlots");

            migrationBuilder.DropColumn(
                name: "ConfirmationCode",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ReferralSource",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RoundType",
                table: "Bookings");
        }
    }
}
