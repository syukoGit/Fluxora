using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class BudgetCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Alimentation et Boissons" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Auto et Transports" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Besoins essentiels" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Dépenses professionnelles" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Divers" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "Espèces et Chèques" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "Factures et Service" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "Frais" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "Impôts" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "Investissements" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "Loisirs et Divertissements" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "Remboursements" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "Rembousement emprunt" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "Revenus" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "Santé" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "Virements" }
                });

            migrationBuilder.InsertData(
                table: "SubCategories",
                columns: new[] { "Id", "CategoryId", "Name", "UserId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), new Guid("00000000-0000-0000-0000-000000000001"), "Bars", null },
                    { new Guid("00000000-0000-0000-0001-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), "Cafés", null },
                    { new Guid("00000000-0000-0000-0001-000000000003"), new Guid("00000000-0000-0000-0000-000000000001"), "Restaurants", null },
                    { new Guid("00000000-0000-0000-0001-000000000004"), new Guid("00000000-0000-0000-0000-000000000001"), "Fast Food", null },
                    { new Guid("00000000-0000-0000-0001-000000000005"), new Guid("00000000-0000-0000-0000-000000000001"), "Courses", null },
                    { new Guid("00000000-0000-0000-0001-000000000006"), new Guid("00000000-0000-0000-0000-000000000001"), "Snacks", null },
                    { new Guid("00000000-0000-0000-0001-000000000007"), new Guid("00000000-0000-0000-0000-000000000001"), "Abonnement", null },
                    { new Guid("00000000-0000-0000-0001-000000000008"), new Guid("00000000-0000-0000-0000-000000000002"), "Abonnement", null },
                    { new Guid("00000000-0000-0000-0001-000000000009"), new Guid("00000000-0000-0000-0000-000000000002"), "Location de voiture", null },
                    { new Guid("00000000-0000-0000-0001-000000000010"), new Guid("00000000-0000-0000-0000-000000000002"), "Lavage", null },
                    { new Guid("00000000-0000-0000-0001-000000000011"), new Guid("00000000-0000-0000-0000-000000000002"), "Carburant", null },
                    { new Guid("00000000-0000-0000-0001-000000000012"), new Guid("00000000-0000-0000-0000-000000000002"), "Assurance", null },
                    { new Guid("00000000-0000-0000-0001-000000000013"), new Guid("00000000-0000-0000-0000-000000000002"), "Parking", null },
                    { new Guid("00000000-0000-0000-0001-000000000014"), new Guid("00000000-0000-0000-0000-000000000002"), "Billets d'avion", null },
                    { new Guid("00000000-0000-0000-0001-000000000015"), new Guid("00000000-0000-0000-0000-000000000002"), "Transports publics", null },
                    { new Guid("00000000-0000-0000-0001-000000000016"), new Guid("00000000-0000-0000-0000-000000000002"), "Réparations", null },
                    { new Guid("00000000-0000-0000-0001-000000000017"), new Guid("00000000-0000-0000-0000-000000000002"), "Taxis, VTC et Covoiturage", null },
                    { new Guid("00000000-0000-0000-0001-000000000018"), new Guid("00000000-0000-0000-0000-000000000002"), "Péages", null },
                    { new Guid("00000000-0000-0000-0001-000000000019"), new Guid("00000000-0000-0000-0000-000000000002"), "Billets de train", null },
                    { new Guid("00000000-0000-0000-0001-000000000020"), new Guid("00000000-0000-0000-0000-000000000003"), "Garde d'enfants", null },
                    { new Guid("00000000-0000-0000-0001-000000000021"), new Guid("00000000-0000-0000-0000-000000000003"), "Vêtements", null },
                    { new Guid("00000000-0000-0000-0001-000000000022"), new Guid("00000000-0000-0000-0000-000000000003"), "Éducation", null },
                    { new Guid("00000000-0000-0000-0001-000000000023"), new Guid("00000000-0000-0000-0000-000000000003"), "Soins personnels", null },
                    { new Guid("00000000-0000-0000-0001-000000000024"), new Guid("00000000-0000-0000-0000-000000000003"), "Cadeaux", null },
                    { new Guid("00000000-0000-0000-0001-000000000025"), new Guid("00000000-0000-0000-0000-000000000003"), "Animaux de compagnie", null },
                    { new Guid("00000000-0000-0000-0001-000000000026"), new Guid("00000000-0000-0000-0000-000000000003"), "Loyer", null },
                    { new Guid("00000000-0000-0000-0001-000000000027"), new Guid("00000000-0000-0000-0000-000000000003"), "Fournitures ménagères", null },
                    { new Guid("00000000-0000-0000-0001-000000000028"), new Guid("00000000-0000-0000-0000-000000000003"), "Frais de scolarité", null },
                    { new Guid("00000000-0000-0000-0001-000000000029"), new Guid("00000000-0000-0000-0000-000000000004"), "Comptabilité", null },
                    { new Guid("00000000-0000-0000-0001-000000000030"), new Guid("00000000-0000-0000-0000-000000000004"), "Publicité", null },
                    { new Guid("00000000-0000-0000-0001-000000000031"), new Guid("00000000-0000-0000-0000-000000000004"), "Voyages d'affaires", null },
                    { new Guid("00000000-0000-0000-0001-000000000032"), new Guid("00000000-0000-0000-0000-000000000004"), "Conseil financier", null },
                    { new Guid("00000000-0000-0000-0001-000000000033"), new Guid("00000000-0000-0000-0000-000000000004"), "Freelancing", null },
                    { new Guid("00000000-0000-0000-0001-000000000034"), new Guid("00000000-0000-0000-0000-000000000004"), "Assistance juridique", null },
                    { new Guid("00000000-0000-0000-0001-000000000035"), new Guid("00000000-0000-0000-0000-000000000004"), "Marketing", null },
                    { new Guid("00000000-0000-0000-0001-000000000036"), new Guid("00000000-0000-0000-0000-000000000004"), "Fournitures de bureau", null },
                    { new Guid("00000000-0000-0000-0001-000000000037"), new Guid("00000000-0000-0000-0000-000000000004"), "Service en ligne", null },
                    { new Guid("00000000-0000-0000-0001-000000000038"), new Guid("00000000-0000-0000-0000-000000000004"), "Impressions", null },
                    { new Guid("00000000-0000-0000-0001-000000000039"), new Guid("00000000-0000-0000-0000-000000000004"), "Services professionnels", null },
                    { new Guid("00000000-0000-0000-0001-000000000040"), new Guid("00000000-0000-0000-0000-000000000004"), "Salaire", null },
                    { new Guid("00000000-0000-0000-0001-000000000041"), new Guid("00000000-0000-0000-0000-000000000004"), "Frais d'envois", null },
                    { new Guid("00000000-0000-0000-0001-000000000042"), new Guid("00000000-0000-0000-0000-000000000005"), "Achat divers", null },
                    { new Guid("00000000-0000-0000-0001-000000000043"), new Guid("00000000-0000-0000-0000-000000000005"), "I don't know", null },
                    { new Guid("00000000-0000-0000-0001-000000000044"), new Guid("00000000-0000-0000-0000-000000000006"), "Dépôts en espèces", null },
                    { new Guid("00000000-0000-0000-0001-000000000045"), new Guid("00000000-0000-0000-0000-000000000006"), "Dépôts de chèques", null },
                    { new Guid("00000000-0000-0000-0001-000000000046"), new Guid("00000000-0000-0000-0000-000000000006"), "Retraits", null },
                    { new Guid("00000000-0000-0000-0001-000000000047"), new Guid("00000000-0000-0000-0000-000000000007"), "Autres abonnements", null },
                    { new Guid("00000000-0000-0000-0001-000000000048"), new Guid("00000000-0000-0000-0000-000000000007"), "Électricité", null },
                    { new Guid("00000000-0000-0000-0001-000000000049"), new Guid("00000000-0000-0000-0000-000000000007"), "Chauffage", null },
                    { new Guid("00000000-0000-0000-0001-000000000050"), new Guid("00000000-0000-0000-0000-000000000007"), "Internet", null },
                    { new Guid("00000000-0000-0000-0001-000000000051"), new Guid("00000000-0000-0000-0000-000000000007"), "Téléphone portable", null },
                    { new Guid("00000000-0000-0000-0001-000000000052"), new Guid("00000000-0000-0000-0000-000000000007"), "Déchets et Recyclage", null },
                    { new Guid("00000000-0000-0000-0001-000000000053"), new Guid("00000000-0000-0000-0000-000000000007"), "Eau", null },
                    { new Guid("00000000-0000-0000-0001-000000000054"), new Guid("00000000-0000-0000-0000-000000000008"), "Frais bancaires", null },
                    { new Guid("00000000-0000-0000-0001-000000000055"), new Guid("00000000-0000-0000-0000-000000000008"), "Frais de retard", null },
                    { new Guid("00000000-0000-0000-0001-000000000056"), new Guid("00000000-0000-0000-0000-000000000008"), "Frais de service", null },
                    { new Guid("00000000-0000-0000-0001-000000000057"), new Guid("00000000-0000-0000-0000-000000000009"), "Impôts sur les sociétés", null },
                    { new Guid("00000000-0000-0000-0001-000000000058"), new Guid("00000000-0000-0000-0000-000000000009"), "Impôts sur les plus-values", null },
                    { new Guid("00000000-0000-0000-0001-000000000059"), new Guid("00000000-0000-0000-0000-000000000009"), "Impôts sur le revenu", null },
                    { new Guid("00000000-0000-0000-0001-000000000060"), new Guid("00000000-0000-0000-0000-000000000009"), "Impôts fonciers", null },
                    { new Guid("00000000-0000-0000-0001-000000000061"), new Guid("00000000-0000-0000-0000-000000000009"), "Cotisations sociales", null },
                    { new Guid("00000000-0000-0000-0001-000000000062"), new Guid("00000000-0000-0000-0000-000000000009"), "TVA", null },
                    { new Guid("00000000-0000-0000-0001-000000000063"), new Guid("00000000-0000-0000-0000-000000000010"), "Obligations", null },
                    { new Guid("00000000-0000-0000-0001-000000000064"), new Guid("00000000-0000-0000-0000-000000000010"), "Crowdfunding", null },
                    { new Guid("00000000-0000-0000-0001-000000000065"), new Guid("00000000-0000-0000-0000-000000000010"), "Cryptos", null },
                    { new Guid("00000000-0000-0000-0001-000000000066"), new Guid("00000000-0000-0000-0000-000000000010"), "Immobilier", null },
                    { new Guid("00000000-0000-0000-0001-000000000067"), new Guid("00000000-0000-0000-0000-000000000010"), "Retraite", null },
                    { new Guid("00000000-0000-0000-0001-000000000068"), new Guid("00000000-0000-0000-0000-000000000010"), "Épargne de sécurité", null },
                    { new Guid("00000000-0000-0000-0001-000000000069"), new Guid("00000000-0000-0000-0000-000000000010"), "Actions", null },
                    { new Guid("00000000-0000-0000-0001-000000000070"), new Guid("00000000-0000-0000-0000-000000000010"), "ETF", null },
                    { new Guid("00000000-0000-0000-0001-000000000071"), new Guid("00000000-0000-0000-0000-000000000010"), "Private Equity", null },
                    { new Guid("00000000-0000-0000-0001-000000000072"), new Guid("00000000-0000-0000-0000-000000000011"), "Art et Musées", null },
                    { new Guid("00000000-0000-0000-0001-000000000073"), new Guid("00000000-0000-0000-0000-000000000011"), "Film", null },
                    { new Guid("00000000-0000-0000-0001-000000000074"), new Guid("00000000-0000-0000-0000-000000000011"), "Livre", null },
                    { new Guid("00000000-0000-0000-0001-000000000075"), new Guid("00000000-0000-0000-0000-000000000011"), "Hobbies", null },
                    { new Guid("00000000-0000-0000-0001-000000000076"), new Guid("00000000-0000-0000-0000-000000000011"), "Jeux", null },
                    { new Guid("00000000-0000-0000-0001-000000000077"), new Guid("00000000-0000-0000-0000-000000000011"), "Cinéma et Théâtre", null },
                    { new Guid("00000000-0000-0000-0001-000000000078"), new Guid("00000000-0000-0000-0000-000000000011"), "Activité", null },
                    { new Guid("00000000-0000-0000-0001-000000000079"), new Guid("00000000-0000-0000-0000-000000000011"), "Sports", null },
                    { new Guid("00000000-0000-0000-0001-000000000080"), new Guid("00000000-0000-0000-0000-000000000011"), "Abonnements", null },
                    { new Guid("00000000-0000-0000-0001-000000000081"), new Guid("00000000-0000-0000-0000-000000000011"), "Vacances", null },
                    { new Guid("00000000-0000-0000-0001-000000000082"), new Guid("00000000-0000-0000-0000-000000000011"), "Boîte de nuit", null },
                    { new Guid("00000000-0000-0000-0001-000000000083"), new Guid("00000000-0000-0000-0000-000000000011"), "Festival", null },
                    { new Guid("00000000-0000-0000-0001-000000000084"), new Guid("00000000-0000-0000-0000-000000000012"), "Paiements d'assurances", null },
                    { new Guid("00000000-0000-0000-0001-000000000085"), new Guid("00000000-0000-0000-0000-000000000012"), "Retours produits", null },
                    { new Guid("00000000-0000-0000-0001-000000000086"), new Guid("00000000-0000-0000-0000-000000000012"), "Remboursements d'impôts", null },
                    { new Guid("00000000-0000-0000-0001-000000000087"), new Guid("00000000-0000-0000-0000-000000000012"), "Réclamation et Garanties", null },
                    { new Guid("00000000-0000-0000-0001-000000000088"), new Guid("00000000-0000-0000-0000-000000000012"), "Remboursement proche", null },
                    { new Guid("00000000-0000-0000-0001-000000000089"), new Guid("00000000-0000-0000-0000-000000000013"), "Résidence principale", null },
                    { new Guid("00000000-0000-0000-0001-000000000090"), new Guid("00000000-0000-0000-0000-000000000013"), "Crédit auto", null },
                    { new Guid("00000000-0000-0000-0001-000000000091"), new Guid("00000000-0000-0000-0000-000000000014"), "Bonus", null },
                    { new Guid("00000000-0000-0000-0001-000000000092"), new Guid("00000000-0000-0000-0000-000000000014"), "Cadeaux", null },
                    { new Guid("00000000-0000-0000-0001-000000000093"), new Guid("00000000-0000-0000-0000-000000000014"), "Investissements", null },
                    { new Guid("00000000-0000-0000-0001-000000000094"), new Guid("00000000-0000-0000-0000-000000000014"), "Revenus locatifs", null },
                    { new Guid("00000000-0000-0000-0001-000000000095"), new Guid("00000000-0000-0000-0000-000000000014"), "Salaire", null },
                    { new Guid("00000000-0000-0000-0001-000000000096"), new Guid("00000000-0000-0000-0000-000000000015"), "Dentiste", null },
                    { new Guid("00000000-0000-0000-0001-000000000097"), new Guid("00000000-0000-0000-0000-000000000015"), "Médecin", null },
                    { new Guid("00000000-0000-0000-0001-000000000098"), new Guid("00000000-0000-0000-0000-000000000015"), "Assurance maladie", null },
                    { new Guid("00000000-0000-0000-0001-000000000099"), new Guid("00000000-0000-0000-0000-000000000015"), "Matériel médical", null },
                    { new Guid("00000000-0000-0000-0001-000000000100"), new Guid("00000000-0000-0000-0000-000000000015"), "Médicaments", null },
                    { new Guid("00000000-0000-0000-0001-000000000101"), new Guid("00000000-0000-0000-0000-000000000015"), "Opticien", null },
                    { new Guid("00000000-0000-0000-0001-000000000102"), new Guid("00000000-0000-0000-0000-000000000015"), "Dermatologue", null },
                    { new Guid("00000000-0000-0000-0001-000000000103"), new Guid("00000000-0000-0000-0000-000000000016"), "Paiement des factures", null },
                    { new Guid("00000000-0000-0000-0001-000000000104"), new Guid("00000000-0000-0000-0000-000000000016"), "Transferts externes", null },
                    { new Guid("00000000-0000-0000-0001-000000000105"), new Guid("00000000-0000-0000-0000-000000000016"), "Dons", null },
                    { new Guid("00000000-0000-0000-0001-000000000106"), new Guid("00000000-0000-0000-0000-000000000016"), "Transferts internes", null },
                    { new Guid("00000000-0000-0000-0001-000000000107"), new Guid("00000000-0000-0000-0000-000000000016"), "Transferts internationaux", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryId",
                table: "SubCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_Name_CategoryId_UserId",
                table: "SubCategories",
                columns: new[] { "Name", "CategoryId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_UserId",
                table: "SubCategories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
