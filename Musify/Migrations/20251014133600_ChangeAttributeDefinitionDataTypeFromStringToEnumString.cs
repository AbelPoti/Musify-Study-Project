using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Musify.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAttributeDefinitionDataTypeFromStringToEnumString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeDefinition_Categories_CategoryId",
                table: "AttributeDefinition");

            migrationBuilder.DropForeignKey(
                name: "FK_InstrumentAttributeValue_AttributeDefinition_AttributeDefinitionId",
                table: "InstrumentAttributeValue");

            migrationBuilder.DropForeignKey(
                name: "FK_InstrumentAttributeValue_Instruments_InstrumentId",
                table: "InstrumentAttributeValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InstrumentAttributeValue",
                table: "InstrumentAttributeValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttributeDefinition",
                table: "AttributeDefinition");

            migrationBuilder.RenameTable(
                name: "InstrumentAttributeValue",
                newName: "InstrumentAttributeValues");

            migrationBuilder.RenameTable(
                name: "AttributeDefinition",
                newName: "AttributeDefinitions");

            migrationBuilder.RenameIndex(
                name: "IX_InstrumentAttributeValue_InstrumentId",
                table: "InstrumentAttributeValues",
                newName: "IX_InstrumentAttributeValues_InstrumentId");

            migrationBuilder.RenameIndex(
                name: "IX_InstrumentAttributeValue_AttributeDefinitionId",
                table: "InstrumentAttributeValues",
                newName: "IX_InstrumentAttributeValues_AttributeDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeDefinition_CategoryId",
                table: "AttributeDefinitions",
                newName: "IX_AttributeDefinitions_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstrumentAttributeValues",
                table: "InstrumentAttributeValues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttributeDefinitions",
                table: "AttributeDefinitions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeDefinitions_Categories_CategoryId",
                table: "AttributeDefinitions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstrumentAttributeValues_AttributeDefinitions_AttributeDefinitionId",
                table: "InstrumentAttributeValues",
                column: "AttributeDefinitionId",
                principalTable: "AttributeDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstrumentAttributeValues_Instruments_InstrumentId",
                table: "InstrumentAttributeValues",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeDefinitions_Categories_CategoryId",
                table: "AttributeDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_InstrumentAttributeValues_AttributeDefinitions_AttributeDefinitionId",
                table: "InstrumentAttributeValues");

            migrationBuilder.DropForeignKey(
                name: "FK_InstrumentAttributeValues_Instruments_InstrumentId",
                table: "InstrumentAttributeValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InstrumentAttributeValues",
                table: "InstrumentAttributeValues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttributeDefinitions",
                table: "AttributeDefinitions");

            migrationBuilder.RenameTable(
                name: "InstrumentAttributeValues",
                newName: "InstrumentAttributeValue");

            migrationBuilder.RenameTable(
                name: "AttributeDefinitions",
                newName: "AttributeDefinition");

            migrationBuilder.RenameIndex(
                name: "IX_InstrumentAttributeValues_InstrumentId",
                table: "InstrumentAttributeValue",
                newName: "IX_InstrumentAttributeValue_InstrumentId");

            migrationBuilder.RenameIndex(
                name: "IX_InstrumentAttributeValues_AttributeDefinitionId",
                table: "InstrumentAttributeValue",
                newName: "IX_InstrumentAttributeValue_AttributeDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_AttributeDefinitions_CategoryId",
                table: "AttributeDefinition",
                newName: "IX_AttributeDefinition_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstrumentAttributeValue",
                table: "InstrumentAttributeValue",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttributeDefinition",
                table: "AttributeDefinition",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeDefinition_Categories_CategoryId",
                table: "AttributeDefinition",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstrumentAttributeValue_AttributeDefinition_AttributeDefinitionId",
                table: "InstrumentAttributeValue",
                column: "AttributeDefinitionId",
                principalTable: "AttributeDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstrumentAttributeValue_Instruments_InstrumentId",
                table: "InstrumentAttributeValue",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id");
        }
    }
}
