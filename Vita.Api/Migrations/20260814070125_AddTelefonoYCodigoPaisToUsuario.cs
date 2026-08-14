using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vita.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTelefonoYCodigoPaisToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoPais",
                table: "AspNetUsers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoPais",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "AspNetUsers");
        }
    }
}
