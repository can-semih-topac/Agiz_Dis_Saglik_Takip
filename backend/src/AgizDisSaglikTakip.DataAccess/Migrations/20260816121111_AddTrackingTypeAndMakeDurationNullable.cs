using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgizDisSaglikTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingTypeAndMakeDurationNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApplied",
                table: "GoalStatuses");

            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                table: "GoalStatuses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TrackingType",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackingType",
                table: "Goals");

            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                table: "GoalStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApplied",
                table: "GoalStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
