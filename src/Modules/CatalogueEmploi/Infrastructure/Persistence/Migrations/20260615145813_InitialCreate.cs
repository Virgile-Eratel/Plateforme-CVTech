using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVTech.Modules.CatalogueEmploi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "emploi");

            migrationBuilder.CreateTable(
                name: "Annonces",
                schema: "emploi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntrepriseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    TypeContrat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DomaineCode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DomaineLibelle = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DatePublication = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annonces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Candidatures",
                schema: "emploi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnnonceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LettreMotivation = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DateDepot = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cvs",
                schema: "emploi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Presentation = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Competences = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cvs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Annonces",
                schema: "emploi");

            migrationBuilder.DropTable(
                name: "Candidatures",
                schema: "emploi");

            migrationBuilder.DropTable(
                name: "Cvs",
                schema: "emploi");
        }
    }
}
