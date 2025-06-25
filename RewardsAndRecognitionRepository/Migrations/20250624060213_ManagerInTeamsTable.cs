using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RewardsAndRecognitionRepository.Migrations
{
    /// <inheritdoc />
    public partial class ManagerInTeamsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "Teams",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Discriminator", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "ManagerId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TeamId", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "2a98c848-7437-4537-94f4-eb6c7a9991b6", 0, "5c7ba577-232b-44c9-95e5-a6ddc5ca614d", new DateTime(2025, 6, 24, 6, 2, 11, 527, DateTimeKind.Utc).AddTicks(9683), "User", "manager@company.com", false, true, false, null, null, "Manager User", null, null, "hashed_password_here", null, false, "d266a3a6-1af7-419b-aff2-441fa51502a9", null, false, null },
                    { "48f87d4d-873d-4b1c-9214-eaad1b3e8ff6", 0, "a91d3224-ade9-4d01-b80b-e3cb57df2f13", new DateTime(2025, 6, 24, 6, 2, 11, 527, DateTimeKind.Utc).AddTicks(9541), "User", "admin@company.com", false, true, false, null, null, "Admin User", null, null, "hashed_password_here", null, false, "3a79a451-3bb1-46b8-bde2-d2f5947a4b7f", null, false, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[] { new Guid("b61d3fd2-dcca-467e-8984-743931f32f13"), new DateTime(2025, 6, 24, 6, 2, 11, 528, DateTimeKind.Utc).AddTicks(125), "Awarded to best overall performer", "Best Performer" });

            migrationBuilder.InsertData(
                table: "YearQuarters",
                columns: new[] { "Id", "EndDate", "IsActive", "Quarter", "StartDate", "Year" },
                values: new object[] { new Guid("5de6e40c-28da-4440-a2be-fb2260544c31"), new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Q2", new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2025 });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Discriminator", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "ManagerId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TeamId", "TwoFactorEnabled", "UserName" },
                values: new object[] { "dd6526eb-50d3-4916-9598-bad83997ec92", 0, "cf80c6d8-3471-40fe-87d7-76b461a8ad06", new DateTime(2025, 6, 24, 6, 2, 11, 527, DateTimeKind.Utc).AddTicks(9714), "User", "lead@company.com", false, true, false, null, "2a98c848-7437-4537-94f4-eb6c7a9991b6", "Team Lead User", null, null, "hashed_password_here", null, false, "38981046-af1c-4211-81c6-99360f3879f8", null, false, null });

            migrationBuilder.InsertData(
                table: "Teams",
                columns: new[] { "Id", "ManagerId", "Name", "TeamLeadId" },
                values: new object[] { new Guid("95b15c20-8c96-4eea-a4d4-2763df66382f"), null, "Alpha Team", "dd6526eb-50d3-4916-9598-bad83997ec92" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Discriminator", "Email", "EmailConfirmed", "IsActive", "LockoutEnabled", "LockoutEnd", "ManagerId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TeamId", "TwoFactorEnabled", "UserName" },
                values: new object[] { "b5bef854-6b8b-4d93-9cbb-47ec8677ec22", 0, "c45bd313-ecde-4e8d-8853-8a2c475dedba", new DateTime(2025, 6, 24, 6, 2, 11, 527, DateTimeKind.Utc).AddTicks(9741), "User", "employee@company.com", false, true, false, null, "2a98c848-7437-4537-94f4-eb6c7a9991b6", "Employee One", null, null, "hashed_password_here", null, false, "538da704-7037-41ba-82a9-02495d3582d8", new Guid("95b15c20-8c96-4eea-a4d4-2763df66382f"), false, null });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ManagerId",
                table: "Teams",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_ManagerId",
                table: "Teams",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_ManagerId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_ManagerId",
                table: "Teams");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "48f87d4d-873d-4b1c-9214-eaad1b3e8ff6");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b5bef854-6b8b-4d93-9cbb-47ec8677ec22");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b61d3fd2-dcca-467e-8984-743931f32f13"));

            migrationBuilder.DeleteData(
                table: "YearQuarters",
                keyColumn: "Id",
                keyValue: new Guid("5de6e40c-28da-4440-a2be-fb2260544c31"));

            migrationBuilder.DeleteData(
                table: "Teams",
                keyColumn: "Id",
                keyValue: new Guid("95b15c20-8c96-4eea-a4d4-2763df66382f"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "dd6526eb-50d3-4916-9598-bad83997ec92");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2a98c848-7437-4537-94f4-eb6c7a9991b6");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Teams");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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
        }
    }
}
