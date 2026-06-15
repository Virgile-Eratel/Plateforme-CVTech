using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVTech.Modules.AppelOffreFreelance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "freelance");

            migrationBuilder.CreateTable(
                name: "AppelsOffre",
                schema: "freelance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntrepriseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Contexte = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Livrables = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Deadline = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    BudgetMin = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BudgetMax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DomaineCode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DomaineLibelle = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PropositionLaureateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DatePublication = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppelsOffre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Propositions",
                schema: "freelance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppelOffreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tjm = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DureeJours = table.Column<int>(type: "int", nullable: false),
                    Methodologie = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DateSoumission = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propositions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppelsOffre",
                schema: "freelance");

            migrationBuilder.DropTable(
                name: "Propositions",
                schema: "freelance");
        }
    }
}
