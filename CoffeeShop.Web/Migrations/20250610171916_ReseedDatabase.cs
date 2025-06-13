using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoffeeShop.Web.Migrations
{
    /// <inheritdoc />
    public partial class ReseedDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Кофе" },
                    { 2, "Чай" },
                    { 3, "Выпечка" },
                    { 4, "Десерты" }
                });

            migrationBuilder.InsertData(
                table: "Desserts",
                columns: new[] { "Id", "Description", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Классический Нью-Йоркский чизкейк.", "/images/cheesecake.jpg", "Чизкейк", 300.00m },
                    { 2, "Свежий французский круассан.", "/images/croissant.jpg", "Круассан", 120.00m }
                });

            migrationBuilder.InsertData(
                table: "Coffees",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, "Классический крепкий кофе.", "/images/espresso.jpg", "Эспрессо", 150.00m },
                    { 2, 1, "Нежный кофе с молоком.", "/images/latte.jpg", "Латте", 250.00m },
                    { 3, 1, "Кофе с пышной молочной пенкой.", "/images/cappuccino.jpg", "Капучино", 230.00m },
                    { 4, 2, "Бодрящий зеленый чай.", "/images/green_tea.jpg", "Зеленый чай", 180.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Coffees",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Coffees",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Coffees",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Coffees",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Desserts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Desserts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
