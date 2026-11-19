using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealtorApi.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261119185758_AddPropertiesPersistence : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "properties",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                BedroomCount = table.Column<int>(type: "integer", nullable: false),
                BathroomCount = table.Column<int>(type: "integer", nullable: false),
                AreaSquareMeters = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_properties", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_properties_Status",
            table: "properties",
            column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "properties");
    }
}
