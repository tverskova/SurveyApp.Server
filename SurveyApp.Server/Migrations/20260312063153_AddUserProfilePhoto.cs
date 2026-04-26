using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfilePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                table: "UserProfiles",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoContentType",
                table: "UserProfiles",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PhotoContentType",
                table: "UserProfiles");
        }
    }
}
