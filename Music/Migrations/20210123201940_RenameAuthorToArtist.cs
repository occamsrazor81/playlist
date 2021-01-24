using Microsoft.EntityFrameworkCore.Migrations;

namespace Music.Migrations
{
    public partial class RenameAuthorToArtist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Author",
                table: "Songs",
                newName: "Artist");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Artist",
                table: "Songs",
                newName: "Author");
        }
    }
}
