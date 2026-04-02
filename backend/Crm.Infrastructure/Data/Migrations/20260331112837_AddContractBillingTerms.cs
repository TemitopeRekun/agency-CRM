using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContractBillingTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasBeenViewed",
                table: "Offers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "QuoteOpenedAt",
                table: "Offers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuoteTemplateId",
                table: "Offers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseRetainer",
                table: "Contracts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsWaitingSignature",
                table: "Contracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastInvoicedAt",
                table: "Contracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureStatus",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SignedAt",
                table: "Contracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SuccessFeeType",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SuccessFeeValue",
                table: "Contracts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBeenViewed",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "QuoteOpenedAt",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "QuoteTemplateId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "BaseRetainer",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IsWaitingSignature",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "LastInvoicedAt",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SignatureStatus",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SignedAt",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SuccessFeeType",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SuccessFeeValue",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Contracts");
        }
    }
}
