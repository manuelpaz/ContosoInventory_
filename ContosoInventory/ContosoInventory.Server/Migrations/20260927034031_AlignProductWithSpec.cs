using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoInventory.Server.Migrations
{
    /// <summary>
    /// Aligns the Product table with the specification: renames LastModifiedDate to a
    /// non-nullable LastUpdatedDate (preserving data), makes Description optional
    /// (max 1000), reduces Sku to max 50, normalizes existing SKUs, and removes the
    /// out-of-scope IsActive and ReorderLevel columns.
    /// </summary>
    public partial class AlignProductWithSpec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename (not drop + add) so existing timestamps are preserved.
            migrationBuilder.RenameColumn(
                name: "LastModifiedDate",
                table: "Products",
                newName: "LastUpdatedDate");

            // Backfill rows that were never modified before making the column non-nullable.
            migrationBuilder.Sql(
                "UPDATE \"Products\" SET \"LastUpdatedDate\" = \"CreatedDate\" WHERE \"LastUpdatedDate\" IS NULL;");

            // SKUs are now stored normalized (trimmed, upper case) for case-insensitive uniqueness.
            migrationBuilder.Sql(
                "UPDATE \"Products\" SET \"Sku\" = UPPER(TRIM(\"Sku\"));");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                table: "Products");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "TEXT",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "Products",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Products\" SET \"Description\" = '' WHERE \"Description\" IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "Products",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "Products",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "ReorderLevel",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.RenameColumn(
                name: "LastUpdatedDate",
                table: "Products",
                newName: "LastModifiedDate");
        }
    }
}
