using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicButton.Migrations
{
    /// <inheritdoc />
    public partial class pass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "device_config",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "device_config");
        }
    }
}
