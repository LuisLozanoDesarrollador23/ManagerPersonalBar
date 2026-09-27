using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagerPersonalBar.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffAvailabilityAndKitchen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShiftRequirements_BarId_Day_ArrivalTime",
                table: "ShiftRequirements");

            migrationBuilder.DropIndex(
                name: "IX_ShiftAssignments_BarId_Day_ArrivalTime_StaffMemberId",
                table: "ShiftAssignments");

            migrationBuilder.AddColumn<bool>(
                name: "CanWorkAtBothBars",
                table: "StaffMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Department",
                table: "StaffMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeBarId",
                table: "StaffMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeeklyTargetHours",
                table: "StaffMembers",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Department",
                table: "ShiftRequirements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "WeekStartDate",
                table: "ShiftAssignments",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StaffUnavailableDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffMemberId = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffUnavailableDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffUnavailableDays_StaffMembers_StaffMemberId",
                        column: x => x.StaffMemberId,
                        principalTable: "StaffMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VacationPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffMemberId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacationPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VacationPeriods_StaffMembers_StaffMemberId",
                        column: x => x.StaffMemberId,
                        principalTable: "StaffMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_HomeBarId",
                table: "StaffMembers",
                column: "HomeBarId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequirements_BarId_Day_ArrivalTime_Department",
                table: "ShiftRequirements",
                columns: new[] { "BarId", "Day", "ArrivalTime", "Department" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_BarId_Day_ArrivalTime_WeekStartDate_StaffMemberId",
                table: "ShiftAssignments",
                columns: new[] { "BarId", "Day", "ArrivalTime", "WeekStartDate", "StaffMemberId" },
                unique: true,
                filter: "[WeekStartDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StaffUnavailableDays_StaffMemberId_Day",
                table: "StaffUnavailableDays",
                columns: new[] { "StaffMemberId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VacationPeriods_StaffMemberId_StartDate_EndDate",
                table: "VacationPeriods",
                columns: new[] { "StaffMemberId", "StartDate", "EndDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_StaffMembers_Bars_HomeBarId",
                table: "StaffMembers",
                column: "HomeBarId",
                principalTable: "Bars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffMembers_Bars_HomeBarId",
                table: "StaffMembers");

            migrationBuilder.DropTable(
                name: "StaffUnavailableDays");

            migrationBuilder.DropTable(
                name: "VacationPeriods");

            migrationBuilder.DropIndex(
                name: "IX_StaffMembers_HomeBarId",
                table: "StaffMembers");

            migrationBuilder.DropIndex(
                name: "IX_ShiftRequirements_BarId_Day_ArrivalTime_Department",
                table: "ShiftRequirements");

            migrationBuilder.DropIndex(
                name: "IX_ShiftAssignments_BarId_Day_ArrivalTime_WeekStartDate_StaffMemberId",
                table: "ShiftAssignments");

            migrationBuilder.DropColumn(
                name: "CanWorkAtBothBars",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "HomeBarId",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "WeeklyTargetHours",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "ShiftRequirements");

            migrationBuilder.DropColumn(
                name: "WeekStartDate",
                table: "ShiftAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequirements_BarId_Day_ArrivalTime",
                table: "ShiftRequirements",
                columns: new[] { "BarId", "Day", "ArrivalTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_BarId_Day_ArrivalTime_StaffMemberId",
                table: "ShiftAssignments",
                columns: new[] { "BarId", "Day", "ArrivalTime", "StaffMemberId" },
                unique: true);
        }
    }
}
