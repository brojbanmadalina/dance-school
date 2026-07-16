using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceSchool.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ConversationMemberRemoveId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_conversation_members",
                table: "conversation_members");

            migrationBuilder.DropIndex(
                name: "ix_conversation_members_conversation_id_user_id",
                table: "conversation_members");

            migrationBuilder.DropColumn(
                name: "id",
                table: "conversation_members");

            migrationBuilder.AddPrimaryKey(
                name: "pk_conversation_members",
                table: "conversation_members",
                columns: new[] { "conversation_id", "user_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_conversation_members",
                table: "conversation_members");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "conversation_members",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "pk_conversation_members",
                table: "conversation_members",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_conversation_members_conversation_id_user_id",
                table: "conversation_members",
                columns: new[] { "conversation_id", "user_id" },
                unique: true);
        }
    }
}
