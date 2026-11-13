using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20261110193000_AddCacheEpoch")]
public sealed class AddCacheEpoch : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE "CacheEpochs" (
                "Id" integer PRIMARY KEY CHECK ("Id" = 1),
                "Version" bigint NOT NULL
            );
            INSERT INTO "CacheEpochs" ("Id", "Version") VALUES (1, 1);

            CREATE FUNCTION culinary_bump_cache_epoch() RETURNS trigger
            LANGUAGE plpgsql AS $$
            BEGIN
                UPDATE "CacheEpochs"
                SET "Version" = "Version" + 1
                WHERE "Id" = 1;
                RETURN NULL;
            END;
            $$;

            CREATE TRIGGER culinary_cache_recipes
                AFTER INSERT OR UPDATE OR DELETE ON "Recipes"
                FOR EACH STATEMENT EXECUTE FUNCTION culinary_bump_cache_epoch();
            CREATE TRIGGER culinary_cache_categories
                AFTER INSERT OR UPDATE OR DELETE ON "Categories"
                FOR EACH STATEMENT EXECUTE FUNCTION culinary_bump_cache_epoch();
            CREATE TRIGGER culinary_cache_ingredients
                AFTER INSERT OR UPDATE OR DELETE ON "RecipeIngredients"
                FOR EACH STATEMENT EXECUTE FUNCTION culinary_bump_cache_epoch();
            CREATE TRIGGER culinary_cache_steps
                AFTER INSERT OR UPDATE OR DELETE ON "RecipeSteps"
                FOR EACH STATEMENT EXECUTE FUNCTION culinary_bump_cache_epoch();
            CREATE TRIGGER culinary_cache_images
                AFTER INSERT OR UPDATE OR DELETE ON "RecipeImages"
                FOR EACH STATEMENT EXECUTE FUNCTION culinary_bump_cache_epoch();
            CREATE TRIGGER culinary_cache_author_profile
                AFTER UPDATE OF "DisplayName", "Email" ON "ApplicationUsers"
                FOR EACH STATEMENT EXECUTE FUNCTION culinary_bump_cache_epoch();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TRIGGER culinary_cache_author_profile ON "ApplicationUsers";
            DROP TRIGGER culinary_cache_images ON "RecipeImages";
            DROP TRIGGER culinary_cache_steps ON "RecipeSteps";
            DROP TRIGGER culinary_cache_ingredients ON "RecipeIngredients";
            DROP TRIGGER culinary_cache_categories ON "Categories";
            DROP TRIGGER culinary_cache_recipes ON "Recipes";
            DROP FUNCTION culinary_bump_cache_epoch();
            DROP TABLE "CacheEpochs";
            """);
    }
}
