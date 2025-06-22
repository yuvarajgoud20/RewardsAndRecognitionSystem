using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RewardsAndRecognitionRepository.Migrations
{
    /// <inheritdoc />
    public partial class ExtendingIdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Approvals_Users_ApproverId",
                table: "Approvals");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Users_NominatorId",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Users_NomineeId",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Users_TeamLeadId",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2c587b55-34f2-4280-8555-f099c2a70fdd"));

            migrationBuilder.DeleteData(
                table: "Teams",
                keyColumn: "Id",
                keyValue: new Guid("28baad43-4001-403b-9795-b64f935cdc09"));

            migrationBuilder.DeleteData(
                table: "YearQuarters",
                keyColumn: "Id",
                keyValue: new Guid("9d0d3fab-781c-4144-a90e-f92dfb4ae6cc"));

            migrationBuilder.AlterColumn<string>(
                name: "TeamLeadId",
                table: "Teams",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "NomineeId",
                table: "Nominations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "NominatorId",
                table: "Nominations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApproverId",
                table: "Approvals",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Discriminator", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "ManagerId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TeamId", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "3dd8963f-d85b-49e9-8f10-8c93c87e7043", 0, "4e9a1a32-608e-4e77-bb65-f34f09bfe9cd", new DateTime(2025, 6, 22, 14, 34, 7, 174, DateTimeKind.Utc).AddTicks(4848), "User", "manager@company.com", false, true, false, null, null, "Manager User", null, null, "hashed_password_here", null, false, "e8f527bd-9e1b-460e-a373-0d18ba9cc1d6", null, false, null },
                    { "d3d25e3e-907e-4ea7-937d-20781ae46855", 0, "6d733ce3-d632-4fd3-a9fc-54ac6656062d", new DateTime(2025, 6, 22, 14, 34, 7, 174, DateTimeKind.Utc).AddTicks(4836), "User", "admin@company.com", false, true, false, null, null, "Admin User", null, null, "hashed_password_here", null, false, "4b38b476-764b-4c0e-9e16-95bf6a26c762", null, false, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[] { new Guid("ef6ec628-330e-459f-99c2-00a99aed48f7"), new DateTime(2025, 6, 22, 14, 34, 7, 174, DateTimeKind.Utc).AddTicks(5091), "Awarded to best overall performer", "Best Performer" });

            migrationBuilder.InsertData(
                table: "YearQuarters",
                columns: new[] { "Id", "EndDate", "IsActive", "Quarter", "StartDate", "Year" },
                values: new object[] { new Guid("8d965e32-b5eb-44bf-9795-ad687895c1f7"), new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Q2", new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2025 });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Discriminator", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "ManagerId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TeamId", "TwoFactorEnabled", "UserName" },
                values: new object[] { "956bd6b0-c62b-4e3a-9b97-963b96947499", 0, "0c768cf7-e62f-4117-8b93-32de804b0ea8", new DateTime(2025, 6, 22, 14, 34, 7, 174, DateTimeKind.Utc).AddTicks(4862), "User", "lead@company.com", false, true, false, null, "3dd8963f-d85b-49e9-8f10-8c93c87e7043", "Team Lead User", null, null, "hashed_password_here", null, false, "2395a56e-3703-4190-8e24-5c0a48395666", null, false, null });

            migrationBuilder.InsertData(
                table: "Teams",
                columns: new[] { "Id", "Name", "TeamLeadId" },
                values: new object[] { new Guid("12fc0201-bb3d-4c52-a12f-971e2437c670"), "Alpha Team", "956bd6b0-c62b-4e3a-9b97-963b96947499" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Discriminator", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "ManagerId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TeamId", "TwoFactorEnabled", "UserName" },
                values: new object[] { "eb6ace7f-6b53-450f-a6ed-8e6018927900", 0, "795ba296-132e-4882-bf41-fd4af4593eab", new DateTime(2025, 6, 22, 14, 34, 7, 174, DateTimeKind.Utc).AddTicks(4876), "User", "employee@company.com", false, true, false, null, "3dd8963f-d85b-49e9-8f10-8c93c87e7043", "Employee One", null, null, "hashed_password_here", null, false, "f05ec19b-d750-451d-abeb-123d55557a5a", new Guid("12fc0201-bb3d-4c52-a12f-971e2437c670"), false, null });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagerId",
                table: "AspNetUsers",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TeamId",
                table: "AspNetUsers",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Approvals_AspNetUsers_ApproverId",
                table: "Approvals",
                column: "ApproverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerId",
                table: "AspNetUsers",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Teams_TeamId",
                table: "AspNetUsers",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_AspNetUsers_NominatorId",
                table: "Nominations",
                column: "NominatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_AspNetUsers_NomineeId",
                table: "Nominations",
                column: "NomineeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_TeamLeadId",
                table: "Teams",
                column: "TeamLeadId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Approvals_AspNetUsers_ApproverId",
                table: "Approvals");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Teams_TeamId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_AspNetUsers_NominatorId",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_AspNetUsers_NomineeId",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_TeamLeadId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TeamId",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d3d25e3e-907e-4ea7-937d-20781ae46855");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "eb6ace7f-6b53-450f-a6ed-8e6018927900");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ef6ec628-330e-459f-99c2-00a99aed48f7"));

            migrationBuilder.DeleteData(
                table: "YearQuarters",
                keyColumn: "Id",
                keyValue: new Guid("8d965e32-b5eb-44bf-9795-ad687895c1f7"));

            migrationBuilder.DeleteData(
                table: "Teams",
                keyColumn: "Id",
                keyValue: new Guid("12fc0201-bb3d-4c52-a12f-971e2437c670"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "956bd6b0-c62b-4e3a-9b97-963b96947499");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3dd8963f-d85b-49e9-8f10-8c93c87e7043");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "TeamLeadId",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "NomineeId",
                table: "Nominations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "NominatorId",
                table: "Nominations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "ApproverId",
                table: "Approvals",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[] { new Guid("2c587b55-34f2-4280-8555-f099c2a70fdd"), new DateTime(2025, 6, 21, 9, 33, 29, 121, DateTimeKind.Utc).AddTicks(9150), "Awarded to best overall performer", "Best Performer" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "ManagerId", "Name", "PasswordHash", "Role", "TeamId" },
                values: new object[,]
                {
                    { new Guid("846398bf-21c1-4437-b410-28cef56ea3d6"), new DateTime(2025, 6, 21, 9, 33, 29, 121, DateTimeKind.Utc).AddTicks(8909), "manager@company.com", true, null, "Manager User", "hashed_password_here", "Manager", null },
                    { new Guid("a0b04559-ffcb-44a8-a399-108dbb0e748d"), new DateTime(2025, 6, 21, 9, 33, 29, 121, DateTimeKind.Utc).AddTicks(8907), "admin@company.com", true, null, "Admin User", "hashed_password_here", "Admin", null }
                });

            migrationBuilder.InsertData(
                table: "YearQuarters",
                columns: new[] { "Id", "EndDate", "IsActive", "Quarter", "StartDate", "Year" },
                values: new object[] { new Guid("9d0d3fab-781c-4144-a90e-f92dfb4ae6cc"), new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Q2", new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2025 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "ManagerId", "Name", "PasswordHash", "Role", "TeamId" },
                values: new object[] { new Guid("24141765-5527-4fca-90e2-87355b3a0ea0"), new DateTime(2025, 6, 21, 9, 33, 29, 121, DateTimeKind.Utc).AddTicks(8915), "lead@company.com", true, new Guid("846398bf-21c1-4437-b410-28cef56ea3d6"), "Team Lead User", "hashed_password_here", "TeamLead", null });

            migrationBuilder.InsertData(
                table: "Teams",
                columns: new[] { "Id", "Name", "TeamLeadId" },
                values: new object[] { new Guid("28baad43-4001-403b-9795-b64f935cdc09"), "Alpha Team", new Guid("24141765-5527-4fca-90e2-87355b3a0ea0") });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "ManagerId", "Name", "PasswordHash", "Role", "TeamId" },
                values: new object[] { new Guid("d46aa680-08a1-4cbb-b077-c7d912f45e24"), new DateTime(2025, 6, 21, 9, 33, 29, 121, DateTimeKind.Utc).AddTicks(8918), "employee@company.com", true, new Guid("846398bf-21c1-4437-b410-28cef56ea3d6"), "Employee One", "hashed_password_here", "Employee", new Guid("28baad43-4001-403b-9795-b64f935cdc09") });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerId",
                table: "Users",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TeamId",
                table: "Users",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Approvals_Users_ApproverId",
                table: "Approvals",
                column: "ApproverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_Users_NominatorId",
                table: "Nominations",
                column: "NominatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_Users_NomineeId",
                table: "Nominations",
                column: "NomineeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Users_TeamLeadId",
                table: "Teams",
                column: "TeamLeadId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
