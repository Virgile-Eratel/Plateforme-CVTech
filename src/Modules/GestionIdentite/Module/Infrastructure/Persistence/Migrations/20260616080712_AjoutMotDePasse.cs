using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVTech.Modules.GestionIdentite.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AjoutMotDePasse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MotDePasseHash",
                schema: "identite",
                table: "Utilisateurs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotDePasseHash",
                schema: "identite",
                table: "Utilisateurs");
        }
    }
}
