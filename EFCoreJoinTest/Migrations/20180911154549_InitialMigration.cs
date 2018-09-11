using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCoreJoinTest.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseStudy",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaseStudyName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseStudy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommonData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaseStudyId = table.Column<int>(nullable: false),
                    CommonText = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommonData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommonData_CaseStudy_CaseStudyId",
                        column: x => x.CaseStudyId,
                        principalTable: "CaseStudy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "V1Extended",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CommonDataId = table.Column<int>(nullable: false),
                    ExtendedData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_V1Extended", x => x.Id);
                    table.ForeignKey(
                        name: "FK_V1Extended_CommonData_CommonDataId",
                        column: x => x.CommonDataId,
                        principalTable: "CommonData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseStudy_CaseStudyName",
                table: "CaseStudy",
                column: "CaseStudyName",
                unique: true,
                filter: "[CaseStudyName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CommonData_CaseStudyId",
                table: "CommonData",
                column: "CaseStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_V1Extended_CommonDataId",
                table: "V1Extended",
                column: "CommonDataId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "V1Extended");

            migrationBuilder.DropTable(
                name: "CommonData");

            migrationBuilder.DropTable(
                name: "CaseStudy");
        }
    }
}
