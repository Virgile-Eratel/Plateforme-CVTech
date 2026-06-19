using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVTech.Modules.ActualiteEtAbonnement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "actualite");

            migrationBuilder.CreateTable(
                name: "Abonnements",
                schema: "actualite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abonnements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                schema: "actualite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuteurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categorie = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DomaineCode = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DomaineLibelle = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SourceNom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DatePublication = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "actualite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinataireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateCreation = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Lu = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbonnementDomaines",
                schema: "actualite",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AbonnementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbonnementDomaines", x => new { x.AbonnementId, x.Code });
                    table.ForeignKey(
                        name: "FK_AbonnementDomaines_Abonnements_AbonnementId",
                        column: x => x.AbonnementId,
                        principalSchema: "actualite",
                        principalTable: "Abonnements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abonnements_UtilisateurId",
                schema: "actualite",
                table: "Abonnements",
                column: "UtilisateurId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DestinataireId",
                schema: "actualite",
                table: "Notifications",
                column: "DestinataireId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbonnementDomaines",
                schema: "actualite");

            migrationBuilder.DropTable(
                name: "Articles",
                schema: "actualite");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "actualite");

            migrationBuilder.DropTable(
                name: "Abonnements",
                schema: "actualite");
        }
    }
}
