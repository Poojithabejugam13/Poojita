using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HartfordInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProposerDetailsToPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "CustomerPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "CustomerPolicies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "CustomerPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "CustomerPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "HeightCm",
                table: "CustomerPolicies",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSmoker",
                table: "CustomerPolicies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "CustomerPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PolicyCategory",
                table: "CustomerPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PolicyStartDate",
                table: "CustomerPolicies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "CustomerPolicies",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "IsSmoker",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "PolicyCategory",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "PolicyStartDate",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "CustomerPolicies");
        }
    }
}
