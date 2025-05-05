using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IEEE.Migrations
{
    /// <inheritdoc />
    public partial class _ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Committees_CommitteeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "CommitteeId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Committees_CommitteeId",
                table: "AspNetUsers",
                column: "CommitteeId",
                principalTable: "Committees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Committees_CommitteeId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "CommitteeId",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Committees_CommitteeId",
                table: "AspNetUsers",
                column: "CommitteeId",
                principalTable: "Committees",
                principalColumn: "Id");
        }
    }
}
