using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeBook.Backend.Data.Sqlite.Migrations
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
                type: "INTEGER",
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
