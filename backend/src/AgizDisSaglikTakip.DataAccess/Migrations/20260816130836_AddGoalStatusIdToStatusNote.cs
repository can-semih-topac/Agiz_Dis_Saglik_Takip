using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgizDisSaglikTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalStatusIdToStatusNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GoalStatusId",
                table: "StatusNotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusNotes_GoalStatusId",
                table: "StatusNotes",
                column: "GoalStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusNotes_GoalStatuses_GoalStatusId",
                table: "StatusNotes",
                column: "GoalStatusId",
                principalTable: "GoalStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusNotes_GoalStatuses_GoalStatusId",
                table: "StatusNotes");

            migrationBuilder.DropIndex(
                name: "IX_StatusNotes_GoalStatusId",
                table: "StatusNotes");

            migrationBuilder.DropColumn(
                name: "GoalStatusId",
                table: "StatusNotes");
        }
    }
}
