using Microsoft.EntityFrameworkCore.Migrations;

namespace travelus.Data.Migrations
{
    public partial class AddPaymentStatusToOrderHeaderDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "OrderHeader",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OrderHeader",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrderHeader");
        }
    }
}
