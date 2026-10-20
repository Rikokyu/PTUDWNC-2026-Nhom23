using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(CulinaryBlogDbContext))]
    [Migration("20261014173100_AddRecipeSearchVector")]
    public partial class AddRecipeSearchVector : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Recipes",
                type: "tsvector",
                nullable: true);

            migrationBuilder.Sql(@"
CREATE INDEX ""IX_Recipes_SearchVector""
ON ""Recipes""
USING GIN (""SearchVector"");
");

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION ""UpdateRecipeSearchVector""()
RETURNS trigger AS $$
BEGIN
    NEW.""SearchVector"" = to_tsvector(
        'simple',
        unaccent(COALESCE(NEW.""Title"", '') || ' ' || COALESCE(NEW.""Description"", '') || ' ' || COALESCE(NEW.""Instructions"", ''))
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
");

            migrationBuilder.Sql(@"
CREATE TRIGGER ""TR_Recipes_SearchVector""
BEFORE INSERT OR UPDATE OF ""Title"", ""Description"", ""Instructions""
ON ""Recipes""
FOR EACH ROW
EXECUTE FUNCTION ""UpdateRecipeSearchVector""();
");

            migrationBuilder.Sql(@"
UPDATE ""Recipes""
SET ""SearchVector"" = to_tsvector(
    'simple',
    unaccent(COALESCE(""Title"", '') || ' ' || COALESCE(""Description"", '') || ' ' || COALESCE(""Instructions"", ''))
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS ""TR_Recipes_SearchVector"" ON ""Recipes"";
");

            migrationBuilder.Sql(@"
DROP FUNCTION IF EXISTS ""UpdateRecipeSearchVector""();
");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_SearchVector",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Recipes");
        }
    }
}
