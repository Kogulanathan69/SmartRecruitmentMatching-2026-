using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NexHire.Infrastructure.Data;

#nullable disable

namespace NexHire.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260810114500_SeedResumeTemplates")]
public partial class SeedResumeTemplates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "ResumeTemplate",
            columns: new[]
            {
                "Id",
                "Code",
                "Name",
                "Description",
                "TemplateUrl",
                "PreviewImageUrl",
                "IsAtsFriendly",
                "IsActive"
            },
            columnTypes: new[] { "uniqueidentifier", "nvarchar(max)", "nvarchar(max)", "nvarchar(max)", "nvarchar(max)", "nvarchar(max)", "bit", "bit" },
            values: new object[,]
            {
                {
                    new Guid("0f868e44-3cb4-4d66-b4f7-68e082b4b401"),
                    "classic",
                    "Classic",
                    "A traditional, readable layout for broad professional use.",
                    "builtin://classic",
                    null,
                    true,
                    true
                },
                {
                    new Guid("2e5e7a95-cc1b-49b5-a430-9ab04f7c7402"),
                    "modern",
                    "Modern",
                    "A clean contemporary layout with a strong green accent.",
                    "builtin://modern",
                    null,
                    true,
                    true
                },
                {
                    new Guid("4df82df4-fd42-437d-9812-e81a72b68203"),
                    "minimal",
                    "Minimal",
                    "A restrained monochrome layout focused on content density.",
                    "builtin://minimal",
                    null,
                    true,
                    true
                },
                {
                    new Guid("6bcb38fc-d819-477c-b5c9-ef45a773b804"),
                    "executive",
                    "Executive",
                    "A formal centered layout for senior and leadership profiles.",
                    "builtin://executive",
                    null,
                    true,
                    true
                },
                {
                    new Guid("8f4d155d-f481-43e2-8b73-c9d8db725d05"),
                    "technical",
                    "Technical",
                    "A structured layout that gives technical skills clear emphasis.",
                    "builtin://technical",
                    null,
                    true,
                    true
                }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "ResumeTemplate",
            keyColumn: "Id",
            keyColumnType: "uniqueidentifier",
            keyValue: new Guid("0f868e44-3cb4-4d66-b4f7-68e082b4b401"));
        migrationBuilder.DeleteData(
            table: "ResumeTemplate",
            keyColumn: "Id",
            keyColumnType: "uniqueidentifier",
            keyValue: new Guid("2e5e7a95-cc1b-49b5-a430-9ab04f7c7402"));
        migrationBuilder.DeleteData(
            table: "ResumeTemplate",
            keyColumn: "Id",
            keyColumnType: "uniqueidentifier",
            keyValue: new Guid("4df82df4-fd42-437d-9812-e81a72b68203"));
        migrationBuilder.DeleteData(
            table: "ResumeTemplate",
            keyColumn: "Id",
            keyColumnType: "uniqueidentifier",
            keyValue: new Guid("6bcb38fc-d819-477c-b5c9-ef45a773b804"));
        migrationBuilder.DeleteData(
            table: "ResumeTemplate",
            keyColumn: "Id",
            keyColumnType: "uniqueidentifier",
            keyValue: new Guid("8f4d155d-f481-43e2-8b73-c9d8db725d05"));
    }
}
