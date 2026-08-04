using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgizDisSaglikTakip.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Suggestions",
                columns: new[] { "Id", "Text" },
                values: new object[,]
                {
                    { 1, "Dişlerinizi günde en az iki kez, sabah ve akşam fırçalayın." },
                    { 2, "Diş ipini her gün kullanarak diş aralarındaki plağı temizleyin." },
                    { 3, "Şekerli ve asitli içecekleri azaltarak dişlerinizi çürükten koruyun." },
                    { 4, "Diş fırçanızı her 3 ayda bir, kılları yıprandığında yenileyin." },
                    { 5, "Ağız gargarası kullanarak ağız kokusunu ve bakteri oluşumunu azaltabilirsiniz." },
                    { 6, "Diş hekiminizi yılda en az iki kez düzenli kontrol için ziyaret edin." },
                    { 7, "Sert kıllı yerine yumuşak kıllı diş fırçası tercih edin." },
                    { 8, "Asitli gıdalardan sonra dişlerinizi hemen değil, 30 dakika bekleyip fırçalayın." },
                    { 9, "Bol su içerek ağzınızın kurumasını önleyin, tükürük diş sağlığını korur." },
                    { 10, "Tırnak yeme ve kalem çiğneme gibi alışkanlıklardan kaçının, dişlerinize zarar verir." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Suggestions",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
