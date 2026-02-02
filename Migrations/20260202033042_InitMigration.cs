using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cinemas",
                columns: table => new
                {
                    CinemaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cinemas", x => x.CinemaId);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DurationMin = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PosterUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AgeRating = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.MovieId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    CinemaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    TotalCols = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Rooms_Cinemas",
                        column: x => x.CinemaId,
                        principalTable: "Cinemas",
                        principalColumn: "CinemaId");
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    TokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OTPCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId");
                    table.ForeignKey(
                        name: "FK_UserRoles_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    SeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RowLabel = table.Column<int>(type: "integer", nullable: false),
                    ColNumber = table.Column<int>(type: "integer", nullable: false),
                    SeatType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.SeatId);
                    table.ForeignKey(
                        name: "FK_Seats_Rooms",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId");
                });

            migrationBuilder.CreateTable(
                name: "ShowTimes",
                columns: table => new
                {
                    ShowTimeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false, defaultValueSql: "now()"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowTimes", x => x.ShowTimeId);
                    table.ForeignKey(
                        name: "FK_ShowTimes_Movies",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "MovieId");
                    table.ForeignKey(
                        name: "FK_ShowTimes_Rooms",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId");
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShowTimeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: false, defaultValueSql: "now()"),
                    ExpireAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_ShowTimes",
                        column: x => x.ShowTimeId,
                        principalTable: "ShowTimes",
                        principalColumn: "ShowTimeId");
                    table.ForeignKey(
                        name: "FK_Bookings_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ShowTimeSeats",
                columns: table => new
                {
                    ShowTimeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    HoldByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    HoldSessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HoldUntil = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    PriceOverride = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowTimeSeats", x => new { x.ShowTimeId, x.SeatId });
                    table.ForeignKey(
                        name: "FK_ShowTimeSeats_Seats",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "SeatId");
                    table.ForeignKey(
                        name: "FK_ShowTimeSeats_ShowTimes",
                        column: x => x.ShowTimeId,
                        principalTable: "ShowTimes",
                        principalColumn: "ShowTimeId");
                    table.ForeignKey(
                        name: "FK_ShowTimeSeats_Users",
                        column: x => x.HoldByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp(6) with time zone", precision: 6, nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ProviderRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShowTimeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TicketCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_Tickets_Bookings",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                    table.ForeignKey(
                        name: "FK_Tickets_Seats",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "SeatId");
                    table.ForeignKey(
                        name: "FK_Tickets_ShowTimes",
                        column: x => x.ShowTimeId,
                        principalTable: "ShowTimes",
                        principalColumn: "ShowTimeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ShowTime_CreatedAt",
                table: "Bookings",
                columns: new[] { "ShowTimeId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_User_CreatedAt",
                table: "Bookings",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UQ_Bookings_BookingCode",
                table: "Bookings",
                column: "BookingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_ExpiryTime",
                table: "PasswordResetTokens",
                column: "ExpiryTime");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_User_OTP",
                table: "PasswordResetTokens",
                columns: new[] { "UserId", "OTPCode" });

            migrationBuilder.CreateIndex(
                name: "UQ_Payments_Booking",
                table: "Payments",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CinemaId",
                table: "Rooms",
                column: "CinemaId");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_RoomId",
                table: "Seats",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "UQ_Seats_Room_SeatCode",
                table: "Seats",
                columns: new[] { "RoomId", "SeatCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowTimes_Movie_StartAt",
                table: "ShowTimes",
                columns: new[] { "MovieId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowTimes_Room_StartAt",
                table: "ShowTimes",
                columns: new[] { "RoomId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowTimeSeats_HoldByUserId",
                table: "ShowTimeSeats",
                column: "HoldByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowTimeSeats_Query",
                table: "ShowTimeSeats",
                columns: new[] { "ShowTimeId", "Status", "HoldUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowTimeSeats_SeatId",
                table: "ShowTimeSeats",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BookingId",
                table: "Tickets",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SeatId",
                table: "Tickets",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "UQ_Tickets_ShowTime_Seat",
                table: "Tickets",
                columns: new[] { "ShowTimeId", "SeatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Tickets_TicketCode",
                table: "Tickets",
                column: "TicketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ShowTimeSeats");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ShowTimes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Cinemas");
        }
    }
}
