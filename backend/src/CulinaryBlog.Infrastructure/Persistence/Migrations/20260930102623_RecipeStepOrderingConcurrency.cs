using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RecipeStepOrderingConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecipeSteps_RecipeId_StepNumber",
                table: "RecipeSteps");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeId_StepNumber",
                table: "RecipeSteps",
                columns: new[] { "RecipeId", "StepNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecipeSteps_RecipeId_StepNumber",
                table: "RecipeSteps");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeId_StepNumber",
                table: "RecipeSteps",
                columns: new[] { "RecipeId", "StepNumber" },
                unique: true);
        }
    }
}
