using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrectAnswerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCorrectAnswer",
                table: "Questions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "QuestionOptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCorrectAnswer",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "QuestionOptions");
        }
    }
}
