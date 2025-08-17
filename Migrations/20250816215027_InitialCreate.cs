using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Genealogy.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GivenName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    BirthPlace = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DeathDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    DeathPlace = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marriages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpouseAId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpouseBId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MarriageDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    MarriagePlace = table.Column<string>(type: "TEXT", nullable: true),
                    DivorceDate = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marriages", x => x.Id);
                    table.CheckConstraint("CK_Marriage_DistinctSpouses", "SpouseAId <> SpouseBId");
                    table.ForeignKey(
                        name: "FK_Marriages_People_SpouseAId",
                        column: x => x.SpouseAId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Marriages_People_SpouseBId",
                        column: x => x.SpouseBId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParentChildren",
                columns: table => new
                {
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChildId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentChildren", x => new { x.ParentId, x.ChildId });
                    table.ForeignKey(
                        name: "FK_ParentChildren_People_ChildId",
                        column: x => x.ChildId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentChildren_People_ParentId",
                        column: x => x.ParentId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Marriages_SpouseAId_SpouseBId",
                table: "Marriages",
                columns: new[] { "SpouseAId", "SpouseBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marriages_SpouseBId",
                table: "Marriages",
                column: "SpouseBId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentChildren_ChildId",
                table: "ParentChildren",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_People_Surname_GivenName",
                table: "People",
                columns: new[] { "Surname", "GivenName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Marriages");

            migrationBuilder.DropTable(
                name: "ParentChildren");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
