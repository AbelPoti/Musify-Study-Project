using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Musify.Migrations
{
    /// <inheritdoc />
    public partial class AddLastPasswordResetSentOffsetToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastPasswordResetSent",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPasswordResetSent",
                table: "AspNetUsers");
        }
    }
}
