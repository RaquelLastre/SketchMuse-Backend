using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SketchMuse.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Imagenes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Imagenes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Imagenes");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Imagenes");
        }
    }
}
