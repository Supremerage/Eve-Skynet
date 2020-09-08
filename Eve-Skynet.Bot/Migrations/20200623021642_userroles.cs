using Microsoft.EntityFrameworkCore.Migrations;

namespace Eve_Skynet.Bot.Migrations
{
    public partial class userroles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManagedUserRoles",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManagedRoleGuildId = table.Column<decimal>(nullable: true),
                    ManagedRoleRoleId = table.Column<decimal>(nullable: true),
                    UserId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagedUserRoles", x => x.Key);
                    table.ForeignKey(
                        name: "FK_ManagedUserRoles_ManagedRoles_ManagedRoleGuildId_ManagedRoleRoleId",
                        columns: x => new { x.ManagedRoleGuildId, x.ManagedRoleRoleId },
                        principalTable: "ManagedRoles",
                        principalColumns: new[] { "GuildId", "RoleId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagedUserRoles_ManagedRoleGuildId_ManagedRoleRoleId",
                table: "ManagedUserRoles",
                columns: new[] { "ManagedRoleGuildId", "ManagedRoleRoleId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagedUserRoles");
        }
    }
}
