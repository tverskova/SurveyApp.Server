using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionBankItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    QuestionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasCorrectAnswer = table.Column<bool>(type: "INTEGER", nullable: false),
                    RatingMin = table.Column<int>(type: "INTEGER", nullable: true),
                    RatingMax = table.Column<int>(type: "INTEGER", nullable: true),
                    Image = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBankItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionBankOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestionBankItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBankOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionBankOptions_QuestionBankItems_QuestionBankItemId",
                        column: x => x.QuestionBankItemId,
                        principalTable: "QuestionBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankOptions_QuestionBankItemId",
                table: "QuestionBankOptions",
                column: "QuestionBankItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionBankOptions");

            migrationBuilder.DropTable(
                name: "QuestionBankItems");
        }
    }
}
