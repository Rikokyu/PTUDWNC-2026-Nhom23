using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20261107090000_AddSavedRecipes")]
public sealed class AddSavedRecipes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SavedRecipes",
            columns: table => new
            {
                UserId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                RecipeId = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),
                CreatedAt = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_SavedRecipes",
                    x => new { x.UserId, x.RecipeId });
                table.ForeignKey(
                    name: "FK_SavedRecipes_ApplicationUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "ApplicationUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SavedRecipes_Recipes_RecipeId",
                    column: x => x.RecipeId,
                    principalTable: "Recipes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SavedRecipes_UserId_CreatedAt",
            table: "SavedRecipes",
            columns: new[] { "UserId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_SavedRecipes_RecipeId",
            table: "SavedRecipes",
            column: "RecipeId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SavedRecipes");
    }
}
