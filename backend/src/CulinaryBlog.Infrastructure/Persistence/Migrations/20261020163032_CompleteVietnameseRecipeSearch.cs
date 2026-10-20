using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteVietnameseRecipeSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SearchVector and its GIN index were introduced in AddRecipeSearchVector.
            // This migration adds the EF mapping and upgrades existing vectors in place.
            migrationBuilder.Sql("""
                CREATE EXTENSION IF NOT EXISTS unaccent WITH SCHEMA public;
                CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_ts_config c
                        JOIN pg_namespace n ON n.oid = c.cfgnamespace
                        WHERE c.cfgname = 'vietnamese' AND n.nspname = 'public'
                    ) THEN
                        CREATE TEXT SEARCH CONFIGURATION public.vietnamese (COPY = pg_catalog.simple);
                    END IF;
                END $$;

                ALTER TEXT SEARCH CONFIGURATION public.vietnamese
                    ALTER MAPPING FOR hword, hword_part, word, asciihword, hword_asciipart, asciiword
                    WITH public.unaccent, pg_catalog.simple;

                CREATE OR REPLACE FUNCTION public."UpdateRecipeSearchVector"()
                RETURNS trigger AS $$
                BEGIN
                    NEW."SearchVector" =
                        setweight(to_tsvector('public.vietnamese', COALESCE(NEW."Title", '')), 'A') ||
                        setweight(to_tsvector('public.vietnamese', COALESCE(NEW."Description", '')), 'B') ||
                        setweight(to_tsvector('public.vietnamese', COALESCE(NEW."Instructions", '')), 'C');
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                DROP TRIGGER IF EXISTS "TR_Recipes_SearchVector" ON "Recipes";
                CREATE TRIGGER "TR_Recipes_SearchVector"
                    BEFORE INSERT OR UPDATE OF "Title", "Description", "Instructions" ON "Recipes"
                    FOR EACH ROW EXECUTE FUNCTION public."UpdateRecipeSearchVector"();

                UPDATE "Recipes" SET "SearchVector" =
                    setweight(to_tsvector('public.vietnamese', COALESCE("Title", '')), 'A') ||
                    setweight(to_tsvector('public.vietnamese', COALESCE("Description", '')), 'B') ||
                    setweight(to_tsvector('public.vietnamese', COALESCE("Instructions", '')), 'C');

                CREATE INDEX IF NOT EXISTS "IX_Recipes_SearchVector" ON "Recipes" USING GIN ("SearchVector");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Keep shared extensions/configuration and the pre-existing vector/index.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION public."UpdateRecipeSearchVector"()
                RETURNS trigger AS $$
                BEGIN
                    NEW."SearchVector" = to_tsvector('simple', public.unaccent(
                        COALESCE(NEW."Title", '') || ' ' || COALESCE(NEW."Description", '') || ' ' || COALESCE(NEW."Instructions", '')));
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                UPDATE "Recipes" SET "SearchVector" = to_tsvector('simple', public.unaccent(
                    COALESCE("Title", '') || ' ' || COALESCE("Description", '') || ' ' || COALESCE("Instructions", '')));
                """);
        }
    }
}
