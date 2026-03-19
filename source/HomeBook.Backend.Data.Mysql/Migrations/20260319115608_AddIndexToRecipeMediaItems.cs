using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBook.Backend.Data.Mysql.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToRecipeMediaItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "Recipe2MediaItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "Recipe2MediaItems");
        }
    }
}
