using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransportManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RequiredUniquePrimaryKeyRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Id",
                table: "Trucks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Type",
                table: "Trucks",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_Abbr",
                table: "Routes",
                column: "Abbr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_Id",
                table: "Routes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_Name",
                table: "Routes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_Name_Abbr",
                table: "Routes",
                columns: new[] { "Name", "Abbr" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Id",
                table: "Products",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_Id",
                table: "Operations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_Id",
                table: "Expeditions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_Name",
                table: "Expeditions",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trucks_Id",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_Type",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Routes_Abbr",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_Id",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_Name",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_Name_Abbr",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Products_Code",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Operations_Id",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Expeditions_Id",
                table: "Expeditions");

            migrationBuilder.DropIndex(
                name: "IX_Expeditions_Name",
                table: "Expeditions");
        }
    }
}
