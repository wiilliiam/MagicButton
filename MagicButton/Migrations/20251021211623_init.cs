using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicButton.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device_config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ButtonPin = table.Column<int>(type: "INTEGER", nullable: false),
                    ButtonPullUp = table.Column<bool>(type: "INTEGER", nullable: false),
                    DebounceMs = table.Column<int>(type: "INTEGER", nullable: false),
                    DoublePressWindowMs = table.Column<int>(type: "INTEGER", nullable: false),
                    LongPressThresholdMs = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DefaultHeaders = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultExtraPayload = table.Column<string>(type: "TEXT", nullable: false),
                    RetriesMaxAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    RetriesBaseDelayMs = table.Column<int>(type: "INTEGER", nullable: false),
                    RetriesJitter = table.Column<bool>(type: "INTEGER", nullable: false),
                    QueueEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    QueuePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_config", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "action_config",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceConfigId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Method = table.Column<int>(type: "INTEGER", nullable: true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Headers = table.Column<string>(type: "TEXT", nullable: true),
                    ExtraPayload = table.Column<string>(type: "TEXT", nullable: true),
                    RetriesMaxAttempts = table.Column<int>(type: "INTEGER", nullable: true),
                    RetriesBaseDelayMs = table.Column<int>(type: "INTEGER", nullable: true),
                    RetriesJitter = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_action_config", x => x.Id);
                    table.ForeignKey(
                        name: "FK_action_config_device_config_DeviceConfigId",
                        column: x => x.DeviceConfigId,
                        principalTable: "device_config",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "led",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceConfigId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Color = table.Column<int>(type: "INTEGER", nullable: false),
                    Pin = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveLow = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_led", x => x.Id);
                    table.ForeignKey(
                        name: "FK_led_device_config_DeviceConfigId",
                        column: x => x.DeviceConfigId,
                        principalTable: "device_config",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "button_press",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceConfigId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionConfigId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    PressedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Simulated = table.Column<bool>(type: "INTEGER", nullable: false),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Queued = table.Column<bool>(type: "INTEGER", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    TransportError = table.Column<bool>(type: "INTEGER", nullable: false),
                    DurationMs = table.Column<int>(type: "INTEGER", nullable: false),
                    LedId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LedPattern = table.Column<int>(type: "INTEGER", nullable: true),
                    LedDurationMs = table.Column<int>(type: "INTEGER", nullable: true),
                    ResponseBodySnippet = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Error = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_button_press", x => x.Id);
                    table.ForeignKey(
                        name: "FK_button_press_action_config_ActionConfigId",
                        column: x => x.ActionConfigId,
                        principalTable: "action_config",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_button_press_device_config_DeviceConfigId",
                        column: x => x.DeviceConfigId,
                        principalTable: "device_config",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_button_press_led_LedId",
                        column: x => x.LedId,
                        principalTable: "led",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "response_mapping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceConfigId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LedId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CodeStart = table.Column<int>(type: "INTEGER", nullable: true),
                    CodeEnd = table.Column<int>(type: "INTEGER", nullable: true),
                    TransportError = table.Column<bool>(type: "INTEGER", nullable: false),
                    Pattern = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationMs = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 2000),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_response_mapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_response_mapping_device_config_DeviceConfigId",
                        column: x => x.DeviceConfigId,
                        principalTable: "device_config",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_response_mapping_led_LedId",
                        column: x => x.LedId,
                        principalTable: "led",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_action_config_DeviceConfigId_Kind",
                table: "action_config",
                columns: new[] { "DeviceConfigId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_button_press_ActionConfigId",
                table: "button_press",
                column: "ActionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_button_press_DeviceConfigId",
                table: "button_press",
                column: "DeviceConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_button_press_DeviceId_PressedAtUtc",
                table: "button_press",
                columns: new[] { "DeviceId", "PressedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_button_press_LedId",
                table: "button_press",
                column: "LedId");

            migrationBuilder.CreateIndex(
                name: "IX_button_press_PressedAtUtc",
                table: "button_press",
                column: "PressedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_device_config_DeviceId",
                table: "device_config",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_led_DeviceConfigId_Pin",
                table: "led",
                columns: new[] { "DeviceConfigId", "Pin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_response_mapping_DeviceConfigId",
                table: "response_mapping",
                column: "DeviceConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_response_mapping_DeviceConfigId_TransportError_CodeStart_CodeEnd_Priority",
                table: "response_mapping",
                columns: new[] { "DeviceConfigId", "TransportError", "CodeStart", "CodeEnd", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_response_mapping_LedId",
                table: "response_mapping",
                column: "LedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "button_press");

            migrationBuilder.DropTable(
                name: "response_mapping");

            migrationBuilder.DropTable(
                name: "action_config");

            migrationBuilder.DropTable(
                name: "led");

            migrationBuilder.DropTable(
                name: "device_config");
        }
    }
}
