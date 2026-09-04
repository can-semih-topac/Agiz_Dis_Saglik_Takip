using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgizDisSaglikTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenamePasswordEncryptedToPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordEncrypted",
                table: "Users",
                newName: "PasswordHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "PasswordEncrypted");
        }
    }
}
