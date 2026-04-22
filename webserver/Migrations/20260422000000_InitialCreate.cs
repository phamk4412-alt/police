#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PoliceWebServer.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Action = table.Column<string>(maxLength: 80, nullable: false),
                EntityType = table.Column<string>(maxLength: 80, nullable: false),
                EntityId = table.Column<string>(maxLength: 120, nullable: false),
                ActorUsername = table.Column<string>(maxLength: 120, nullable: false),
                ActorDisplayName = table.Column<string>(maxLength: 160, nullable: false),
                ActorRole = table.Column<string>(maxLength: 32, nullable: false),
                Summary = table.Column<string>(maxLength: 280, nullable: false),
                Detail = table.Column<string>(maxLength: 2000, nullable: false),
                IpAddress = table.Column<string>(maxLength: 64, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Incidents",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Title = table.Column<string>(maxLength: 160, nullable: false),
                Detail = table.Column<string>(maxLength: 4000, nullable: false),
                Category = table.Column<string>(maxLength: 120, nullable: false),
                Level = table.Column<string>(maxLength: 24, nullable: false),
                UrgencyScore = table.Column<int>(nullable: false),
                ClassificationReason = table.Column<string>(maxLength: 500, nullable: false),
                Latitude = table.Column<double>(nullable: false),
                Longitude = table.Column<double>(nullable: false),
                District = table.Column<string>(maxLength: 80, nullable: false),
                TimeLabel = table.Column<string>(maxLength: 16, nullable: false),
                Status = table.Column<string>(maxLength: 64, nullable: false),
                Source = table.Column<string>(maxLength: 32, nullable: false),
                ReporterName = table.Column<string>(maxLength: 120, nullable: false),
                LastUpdatedBy = table.Column<string>(maxLength: 120, nullable: false),
                InternalNote = table.Column<string>(maxLength: 2000, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Incidents", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Username = table.Column<string>(maxLength: 120, nullable: false),
                NormalizedUsername = table.Column<string>(maxLength: 120, nullable: false),
                PasswordHash = table.Column<string>(maxLength: 512, nullable: false),
                Email = table.Column<string>(maxLength: 256, nullable: false),
                DisplayName = table.Column<string>(maxLength: 160, nullable: false),
                Role = table.Column<string>(maxLength: 32, nullable: false),
                IsLocked = table.Column<bool>(nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                LastLoginAt = table.Column<DateTimeOffset>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateIndex(name: "IX_AuditLogs_Action", table: "AuditLogs", column: "Action");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_ActorRole", table: "AuditLogs", column: "ActorRole");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_CreatedAt", table: "AuditLogs", column: "CreatedAt");
        migrationBuilder.CreateIndex(name: "IX_Incidents_CreatedAt", table: "Incidents", column: "CreatedAt");
        migrationBuilder.CreateIndex(name: "IX_Incidents_District", table: "Incidents", column: "District");
        migrationBuilder.CreateIndex(name: "IX_Incidents_Level", table: "Incidents", column: "Level");
        migrationBuilder.CreateIndex(name: "IX_Incidents_Status", table: "Incidents", column: "Status");
        migrationBuilder.CreateIndex(name: "IX_Users_NormalizedUsername", table: "Users", column: "NormalizedUsername", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditLogs");
        migrationBuilder.DropTable(name: "Incidents");
        migrationBuilder.DropTable(name: "Users");
    }
}