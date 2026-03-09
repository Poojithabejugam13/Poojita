using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HartfordInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToPlanTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PolicyCategory",
                table: "CustomerPolicies",
                newName: "PlanType");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PlanTiers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "PlanTiers");

            migrationBuilder.RenameColumn(
                name: "PlanType",
                table: "CustomerPolicies",
                newName: "PolicyCategory");
        }
    }
}
