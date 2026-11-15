using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceRecipeChildInvariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RecipeSteps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RecipeIngredients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RecipeImages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RecipeSteps_StepNumber_Positive",
                table: "RecipeSteps",
                sql: "\"StepNumber\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RecipeSteps_TimerMinutes_NonNegative",
                table: "RecipeSteps",
                sql: "\"TimerMinutes\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeImages_RecipeId_Primary",
                table: "RecipeImages",
                column: "RecipeId",
                unique: true,
                filter: "\"IsPrimary\" = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RecipeSteps_StepNumber_Positive",
                table: "RecipeSteps");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RecipeSteps_TimerMinutes_NonNegative",
                table: "RecipeSteps");

            migrationBuilder.DropIndex(
                name: "IX_RecipeImages_RecipeId_Primary",
                table: "RecipeImages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RecipeSteps");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RecipeImages");
        }
    }
}
