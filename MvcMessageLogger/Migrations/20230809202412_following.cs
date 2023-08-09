﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcMessageLogger.Migrations
{
    /// <inheritdoc />
    public partial class following : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_user",
                columns: table => new
                {
                    followers_id = table.Column<int>(type: "integer", nullable: false),
                    following_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_user", x => new { x.followers_id, x.following_id });
                    table.ForeignKey(
                        name: "fk_user_user_users_followers_id",
                        column: x => x.followers_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_user_users_following_id",
                        column: x => x.following_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_user_following_id",
                table: "user_user",
                column: "following_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_user");
        }
    }
}
