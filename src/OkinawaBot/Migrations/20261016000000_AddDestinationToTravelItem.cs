using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkinawaBot.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationToTravelItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "travel_items",
                type: "TEXT",
                nullable: false,
                defaultValue: "沖繩");

            migrationBuilder.CreateIndex(
                name: "IX_travel_items_Destination_Category",
                table: "travel_items",
                columns: new[] { "Destination", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_travel_items_Destination_Name",
                table: "travel_items",
                columns: new[] { "Destination", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_travel_items_Destination_Category",
                table: "travel_items");

            migrationBuilder.DropIndex(
                name: "IX_travel_items_Destination_Name",
                table: "travel_items");

            migrationBuilder.DropColumn(
                name: "Destination",
                table: "travel_items");
        }
    }
}
