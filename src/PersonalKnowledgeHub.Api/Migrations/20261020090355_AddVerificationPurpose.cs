using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalKnowledgeHub.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationPurpose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "VerificationTokens",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "VerificationTokens");
        }
    }
}
