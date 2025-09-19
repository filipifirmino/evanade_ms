using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderEntity",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderEntity", x => x.OrderId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderEntity_CustomerId",
                table: "OrderEntity",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEntity_CustomerId_Status",
                table: "OrderEntity",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderEntity_Status",
                table: "OrderEntity",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEntity_TotalAmount",
                table: "OrderEntity",
                column: "TotalAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderEntity");
        }
    }
}
