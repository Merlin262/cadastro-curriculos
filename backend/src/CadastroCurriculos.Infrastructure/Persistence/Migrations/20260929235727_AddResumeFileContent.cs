using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadastroCurriculos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeFileContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ResumeFileContent",
                table: "Candidates",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumeFileContent",
                table: "Candidates");
        }
    }
}
