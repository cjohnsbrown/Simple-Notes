using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleNotes.Api.Migrations
{
    public partial class AddUserKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
             name: "SecretKey",
             table: "AspNetUsers",
             nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
