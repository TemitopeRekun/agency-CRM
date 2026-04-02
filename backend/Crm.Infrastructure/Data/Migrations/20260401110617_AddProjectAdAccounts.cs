using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectAdAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Contracts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PortalToken",
                table: "Contracts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SignatureData",
                table: "Contracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AdAccountId",
                table: "AdMetrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectAdAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    ExternalAccountId = table.Column<string>(type: "text", nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAdAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAdAccounts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AdMetrics_AdAccountId",
                table: "AdMetrics",
                column: "AdAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAdAccounts_ProjectId",
                table: "ProjectAdAccounts",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdMetrics_ProjectAdAccounts_AdAccountId",
                table: "AdMetrics",
                column: "AdAccountId",
                principalTable: "ProjectAdAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdMetrics_ProjectAdAccounts_AdAccountId",
                table: "AdMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "ProjectAdAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_AdMetrics_AdAccountId",
                table: "AdMetrics");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "PortalToken",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SignatureData",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "AdAccountId",
                table: "AdMetrics");
        }
    }
}
