using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjoutAgeCv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                schema: "emploi",
                table: "Cvs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                schema: "emploi",
                table: "Cvs");
        }
    }
}
