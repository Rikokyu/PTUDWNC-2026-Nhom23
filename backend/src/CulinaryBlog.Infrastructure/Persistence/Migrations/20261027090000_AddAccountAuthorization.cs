using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CulinaryBlogDbContext))]
[Migration("20261027090000_AddAccountAuthorization")]
public sealed class AddAccountAuthorization : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "ApplicationUsers",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<int>(
            name: "TokenVersion",
            table: "ApplicationUsers",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "ApplicationUsers");

        migrationBuilder.DropColumn(
            name: "TokenVersion",
            table: "ApplicationUsers");
    }
}
