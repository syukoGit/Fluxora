using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class FixData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000012"),
                column: "Name",
                value: "Remboursement emprunt");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000013"),
                column: "Name",
                value: "Remboursements");

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000084"),
                column: "Name",
                value: "Résidence principale");

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000085"),
                column: "Name",
                value: "Crédit auto");

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000086"),
                columns: new[] { "CategoryId", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000013"), "Paiements d'assurances" });

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000087"),
                columns: new[] { "CategoryId", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000013"), "Retours produits" });

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000088"),
                columns: new[] { "CategoryId", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000013"), "Remboursements d'impôts" });

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000089"),
                column: "Name",
                value: "Réclamation et Garanties");

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000090"),
                column: "Name",
                value: "Remboursement proche");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_Name_CategoryId",
                table: "SubCategories",
                columns: new[] { "Name", "CategoryId" },
                unique: true,
                filter: "\"UserId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubCategories_Name_CategoryId",
                table: "SubCategories");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000012"),
                column: "Name",
                value: "Remboursements");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000013"),
                column: "Name",
                value: "Rembousement emprunt");

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000084"),
                column: "Name",
                value: "Paiements d'assurances");

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000085"),
                column: "Name",
                value: "Retours produits");

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000086"),
                columns: new[] { "CategoryId", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000012"), "Remboursements d'impôts" });

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000087"),
                columns: new[] { "CategoryId", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000012"), "Réclamation et Garanties" });

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000088"),
                columns: new[] { "CategoryId", "Name" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000012"), "Remboursement proche" });

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000089"),
                column: "Name",
                value: "Résidence principale");

            migrationBuilder.UpdateData(
                table: "SubCategories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000090"),
                column: "Name",
                value: "Crédit auto");
        }
    }
}
