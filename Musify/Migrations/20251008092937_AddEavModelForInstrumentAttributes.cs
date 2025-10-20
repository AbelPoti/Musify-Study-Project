using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Musify.Migrations
{
    /// <inheritdoc />
    public partial class AddEavModelForInstrumentAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttributeDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeDefinition_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentAttributeValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AttributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentAttributeValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentAttributeValue_AttributeDefinition_AttributeDefinitionId",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "AttributeDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstrumentAttributeValue_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instruments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinition_CategoryId",
                table: "AttributeDefinition",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentAttributeValue_AttributeDefinitionId",
                table: "InstrumentAttributeValue",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentAttributeValue_InstrumentId",
                table: "InstrumentAttributeValue",
                column: "InstrumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstrumentAttributeValue");

            migrationBuilder.DropTable(
                name: "AttributeDefinition");
        }
    }
}
