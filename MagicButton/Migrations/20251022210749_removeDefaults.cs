using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicButton.Migrations
{
    /// <inheritdoc />
    public partial class removeDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultExtraPayload",
                table: "device_config");

            migrationBuilder.DropColumn(
                name: "DefaultHeaders",
                table: "device_config");

            migrationBuilder.DropColumn(
                name: "DefaultMethod",
                table: "device_config");

            migrationBuilder.DropColumn(
                name: "DefaultUrl",
                table: "device_config");

            migrationBuilder.DropColumn(
                name: "QueueEnabled",
                table: "device_config");

            migrationBuilder.DropColumn(
                name: "QueuePath",
                table: "device_config");

            migrationBuilder.DropColumn(
                name: "RetriesJitter",
                table: "device_config");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultExtraPayload",
                table: "device_config",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultHeaders",
                table: "device_config",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DefaultMethod",
                table: "device_config",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DefaultUrl",
                table: "device_config",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "QueueEnabled",
                table: "device_config",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QueuePath",
                table: "device_config",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RetriesJitter",
                table: "device_config",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
