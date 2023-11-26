using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace sfa.sale.generator.core.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sfa");

            migrationBuilder.CreateTable(
                name: "SfaContextClientAddress",
                schema: "sfa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CP4 = table.Column<int>(type: "int", nullable: true),
                    CP3 = table.Column<int>(type: "int", nullable: true),
                    PoliceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Floor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fraction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfaContextClientAddress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SfaContextMasterUser",
                schema: "sfa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NIF = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfaContextMasterUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SfaContextOffer",
                schema: "sfa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoId = table.Column<long>(type: "bigint", nullable: false),
                    SimpleId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Campaign = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CampaignPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    TreeNodeSelection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MacroSegment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfaContextOffer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SfaLog",
                schema: "sfa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SfaContextId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageDump = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfaLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SfaLogType",
                schema: "sfa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfaLogType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SfaContext",
                schema: "sfa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoginId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientIdType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientIdValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientAddressId = table.Column<int>(type: "int", nullable: true),
                    OfferId = table.Column<int>(type: "int", nullable: false),
                    MasterUserId = table.Column<int>(type: "int", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfaContext", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SfaContext_SfaContextClientAddress_ClientAddressId",
                        column: x => x.ClientAddressId,
                        principalSchema: "sfa",
                        principalTable: "SfaContextClientAddress",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SfaContext_SfaContextMasterUser_MasterUserId",
                        column: x => x.MasterUserId,
                        principalSchema: "sfa",
                        principalTable: "SfaContextMasterUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SfaContext_SfaContextOffer_OfferId",
                        column: x => x.OfferId,
                        principalSchema: "sfa",
                        principalTable: "SfaContextOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SfaContextOfferFamily",
                schema: "sfa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SfaContextOfferId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfaContextOfferFamily", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SfaContextOfferFamily_SfaContextOffer_SfaContextOfferId",
                        column: x => x.SfaContextOfferId,
                        principalSchema: "sfa",
                        principalTable: "SfaContextOffer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SfaSale",
                schema: "sfa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Guid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: true),
                    SfaContextId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SfaSale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SfaSale_SfaContext_SfaContextId",
                        column: x => x.SfaContextId,
                        principalSchema: "sfa",
                        principalTable: "SfaContext",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "sfa",
                table: "SfaLogType",
                columns: new[] { "Id", "CreatedOn", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 11, 17, 16, 36, 58, 928, DateTimeKind.Local).AddTicks(6142), "", "ERROR" },
                    { 2, new DateTime(2023, 11, 17, 16, 36, 58, 929, DateTimeKind.Local).AddTicks(3148), "", "WARNING" },
                    { 3, new DateTime(2023, 11, 17, 16, 36, 58, 929, DateTimeKind.Local).AddTicks(3233), "", "INFO" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SfaContext_ClientAddressId",
                schema: "sfa",
                table: "SfaContext",
                column: "ClientAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SfaContext_MasterUserId",
                schema: "sfa",
                table: "SfaContext",
                column: "MasterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SfaContext_OfferId",
                schema: "sfa",
                table: "SfaContext",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_SfaContextOfferFamily_SfaContextOfferId",
                schema: "sfa",
                table: "SfaContextOfferFamily",
                column: "SfaContextOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_SfaSale_SfaContextId",
                schema: "sfa",
                table: "SfaSale",
                column: "SfaContextId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SfaContextOfferFamily",
                schema: "sfa");

            migrationBuilder.DropTable(
                name: "SfaLog",
                schema: "sfa");

            migrationBuilder.DropTable(
                name: "SfaLogType",
                schema: "sfa");

            migrationBuilder.DropTable(
                name: "SfaSale",
                schema: "sfa");

            migrationBuilder.DropTable(
                name: "SfaContext",
                schema: "sfa");

            migrationBuilder.DropTable(
                name: "SfaContextClientAddress",
                schema: "sfa");

            migrationBuilder.DropTable(
                name: "SfaContextMasterUser",
                schema: "sfa");

            migrationBuilder.DropTable(
                name: "SfaContextOffer",
                schema: "sfa");
        }
    }
}
