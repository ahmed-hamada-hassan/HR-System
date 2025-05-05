using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IEEE.Migrations
{
    /// <inheritdoc />
    public partial class setHeadIdNullInCommittee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Committees_AspNetUsers_HeadId",
                table: "Committees");

            migrationBuilder.AlterColumn<int>(
                name: "HeadId",
                table: "Committees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Committees_AspNetUsers_HeadId",
                table: "Committees",
                column: "HeadId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Committees_AspNetUsers_HeadId",
                table: "Committees");

            migrationBuilder.AlterColumn<int>(
                name: "HeadId",
                table: "Committees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Committees_AspNetUsers_HeadId",
                table: "Committees",
                column: "HeadId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
