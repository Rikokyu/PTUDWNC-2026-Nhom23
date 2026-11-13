using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace CulinaryBlog.Infrastructure.Persistence.Migrations;
[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20261110060000_AddVietnameseSearch")]
public sealed class AddVietnameseSearch : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE TEXT SEARCH CONFIGURATION public.vietnamese (COPY = pg_catalog.simple);
ALTER TEXT SEARCH CONFIGURATION public.vietnamese ALTER MAPPING FOR hword, hword_part, word WITH public.unaccent, pg_catalog.simple;
ALTER TABLE "Recipes" ADD COLUMN "SearchVector" tsvector;
CREATE FUNCTION public.culinary_recipe_search() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 NEW."SearchVector" :=
 setweight(to_tsvector('public.vietnamese', normalize(coalesce(NEW."Title", ''))), 'A') ||
 setweight(to_tsvector('public.vietnamese', normalize(coalesce(NEW."Description", ''))), 'B') ||
 setweight(to_tsvector('public.vietnamese', normalize(coalesce(NEW."Instructions", ''))), 'C') ||
 setweight(to_tsvector('public.vietnamese', normalize(coalesce((SELECT string_agg("Name", ' ') FROM "RecipeIngredients" WHERE "RecipeId" = NEW."Id"), ''))), 'B');
 RETURN NEW;
END $$;
CREATE TRIGGER culinary_recipe_search BEFORE INSERT OR UPDATE OF "Title", "Description", "Instructions" ON "Recipes" FOR EACH ROW EXECUTE FUNCTION public.culinary_recipe_search();
CREATE FUNCTION public.culinary_ingredient_search() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF TG_OP <> 'INSERT' THEN UPDATE "Recipes" SET "Title" = "Title" WHERE "Id" = OLD."RecipeId"; END IF;
 IF TG_OP <> 'DELETE' THEN UPDATE "Recipes" SET "Title" = "Title" WHERE "Id" = NEW."RecipeId"; END IF;
 RETURN NULL;
END $$;
CREATE TRIGGER culinary_ingredient_search AFTER INSERT OR UPDATE OR DELETE ON "RecipeIngredients" FOR EACH ROW EXECUTE FUNCTION public.culinary_ingredient_search();
UPDATE "Recipes" SET "Title" = "Title";
ALTER TABLE "Recipes" ALTER COLUMN "SearchVector" SET NOT NULL;
CREATE INDEX "IX_Recipes_SearchVector" ON "Recipes" USING GIN ("SearchVector");
""");
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
DROP TRIGGER culinary_ingredient_search ON "RecipeIngredients";
DROP FUNCTION public.culinary_ingredient_search();
DROP TRIGGER culinary_recipe_search ON "Recipes";
DROP FUNCTION public.culinary_recipe_search();
ALTER TABLE "Recipes" DROP COLUMN "SearchVector";
DROP TEXT SEARCH CONFIGURATION public.vietnamese;
""");
}
