using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HartfordInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPEDAndDecisionReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InsurancePlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DecisionReason",
                table: "CustomerPolicies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreExistingDiseases",
                table: "CustomerPolicies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalReason",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "InsurancePlans");

            migrationBuilder.DropColumn(
                name: "DecisionReason",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "PreExistingDiseases",
                table: "CustomerPolicies");

            migrationBuilder.DropColumn(
                name: "ApprovalReason",
                table: "Claims");
        }
    }
}
