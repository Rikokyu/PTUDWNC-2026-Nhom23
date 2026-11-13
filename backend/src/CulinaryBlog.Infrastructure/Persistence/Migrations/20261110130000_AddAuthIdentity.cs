using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20261110130000_AddAuthIdentity")]
public sealed class AddAuthIdentity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>("AccessFailedCount", "ApplicationUsers", "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<DateTime>("LockoutEnd", "ApplicationUsers", "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<string>("GoogleSubject", "ApplicationUsers", "character varying(255)", maxLength: 255, nullable: true);
        migrationBuilder.CreateIndex("IX_ApplicationUsers_GoogleSubject", "ApplicationUsers", "GoogleSubject", unique: true);
        migrationBuilder.AddColumn<Guid>("FamilyId", "RefreshTokens", "uuid", nullable: false, defaultValue: Guid.Empty);
        migrationBuilder.AddColumn<string>("ReplacedByTokenHash", "RefreshTokens", "character varying(64)", maxLength: 64, nullable: true);
        migrationBuilder.Sql("UPDATE \"RefreshTokens\" SET \"FamilyId\" = \"Id\" WHERE \"FamilyId\" = '00000000-0000-0000-0000-000000000000'");
        migrationBuilder.CreateIndex("IX_RefreshTokens_FamilyId", "RefreshTokens", "FamilyId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_RefreshTokens_FamilyId", "RefreshTokens");
        migrationBuilder.DropColumn("ReplacedByTokenHash", "RefreshTokens");
        migrationBuilder.DropColumn("FamilyId", "RefreshTokens");
        migrationBuilder.DropIndex("IX_ApplicationUsers_GoogleSubject", "ApplicationUsers");
        migrationBuilder.DropColumn("GoogleSubject", "ApplicationUsers");
        migrationBuilder.DropColumn("LockoutEnd", "ApplicationUsers");
        migrationBuilder.DropColumn("AccessFailedCount", "ApplicationUsers");
    }
}
