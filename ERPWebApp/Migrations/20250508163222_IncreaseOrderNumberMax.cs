using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPWebApp.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseOrderNumberMax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex("IX_Orders_orderNumber","Orders");
            migrationBuilder.AlterColumn<string>(
                name: "orderNumber",
                table: "Orders",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(35)",
                oldMaxLength: 35);
            migrationBuilder.CreateIndex("IX_Orders_orderNumber", "Orders","orderNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "orderNumber",
                table: "Orders",
                type: "nvarchar(35)",
                maxLength: 35,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(45)",
                oldMaxLength: 45);
        }
    }
}
