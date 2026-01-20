using System;
using System.Collections.Generic;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Data;

public partial class CinemaManagementContext : DbContext
{
    public CinemaManagementContext()
    {
    }

    public CinemaManagementContext(DbContextOptions<CinemaManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Cinema> Cinemas { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<ShowTime> ShowTimes { get; set; }

    public virtual DbSet<ShowTimeSeat> ShowTimeSeats { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=CinemaManagement;Username=postgres;Password=dat2305");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(e => new { e.ShowTimeId, e.CreatedAt }, "IX_Bookings_ShowTime_CreatedAt");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_Bookings_User_CreatedAt");

            entity.HasIndex(e => e.BookingCode, "UQ_Bookings_BookingCode").IsUnique();

            entity.Property(e => e.BookingId).ValueGeneratedNever();
            entity.Property(e => e.BookingCode).HasMaxLength(30);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(6)
                .HasDefaultValueSql("now()");
            entity.Property(e => e.ExpireAt).HasPrecision(6);
            entity.Property(e => e.Status).HasDefaultValue((short)0);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(d => d.ShowTime).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ShowTimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_ShowTimes");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Users");
        });

        modelBuilder.Entity<Cinema>(entity =>
        {
            entity.Property(e => e.CinemaId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(6)
                .HasDefaultValueSql("now()");
            entity.Property(e => e.LastUpdatedAt).HasPrecision(6);
            entity.Property(e => e.Name).HasMaxLength(120);
            entity.Property(e => e.Status).HasDefaultValue(1);
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(e => e.MovieId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasPrecision(6)
                .HasDefaultValueSql("now()");
            entity.Property(e => e.LastUpdatedAt).HasPrecision(6);
            entity.Property(e => e.PosterUrl).HasMaxLength(500);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.BookingId, "UQ_Payments_Booking").IsUnique();

            entity.Property(e => e.PaymentId).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Method).HasMaxLength(30);
            entity.Property(e => e.PaidAt).HasPrecision(6);
            entity.Property(e => e.ProviderRef).HasMaxLength(100);
            entity.Property(e => e.Status).HasDefaultValue((short)0);

            entity.HasOne(d => d.Booking).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Bookings");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();

            entity.Property(e => e.RoleId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(e => e.CinemaId, "IX_Rooms_CinemaId");

            entity.Property(e => e.RoomId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasPrecision(6)
                .HasDefaultValueSql("now()");
            entity.Property(e => e.LastUpdatedAt).HasPrecision(6);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.Cinema).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.CinemaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rooms_Cinemas");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasIndex(e => e.RoomId, "IX_Seats_RoomId");

            entity.HasIndex(e => new { e.RoomId, e.SeatCode }, "UQ_Seats_Room_SeatCode").IsUnique();

            entity.Property(e => e.SeatId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasPrecision(6)
                .HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastUpdatedAt).HasPrecision(6);
            entity.Property(e => e.SeatCode).HasMaxLength(10);
            entity.Property(e => e.SeatType).HasMaxLength(20);

            entity.HasOne(d => d.Room).WithMany(p => p.Seats)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seats_Rooms");
        });

        modelBuilder.Entity<ShowTime>(entity =>
        {
            entity.HasIndex(e => new { e.MovieId, e.StartAt }, "IX_ShowTimes_Movie_StartAt");

            entity.HasIndex(e => new { e.RoomId, e.StartAt }, "IX_ShowTimes_Room_StartAt");

            entity.Property(e => e.ShowTimeId).ValueGeneratedNever();
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(6)
                .HasDefaultValueSql("now()");
            entity.Property(e => e.EndAt).HasPrecision(6);
            entity.Property(e => e.LastUpdatedAt).HasPrecision(6);
            entity.Property(e => e.StartAt).HasPrecision(6);
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasOne(d => d.Movie).WithMany(p => p.ShowTimes)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShowTimes_Movies");

            entity.HasOne(d => d.Room).WithMany(p => p.ShowTimes)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShowTimes_Rooms");
        });

        modelBuilder.Entity<ShowTimeSeat>(entity =>
        {
            entity.HasKey(e => new { e.ShowTimeId, e.SeatId });

            entity.HasIndex(e => new { e.ShowTimeId, e.Status, e.HoldUntil }, "IX_ShowTimeSeats_Query");

            entity.Property(e => e.HoldSessionId).HasMaxLength(100);
            entity.Property(e => e.HoldUntil).HasPrecision(6);
            entity.Property(e => e.PriceOverride).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasDefaultValue((short)0);

            entity.HasOne(d => d.HoldByUser).WithMany(p => p.ShowTimeSeats)
                .HasForeignKey(d => d.HoldByUserId)
                .HasConstraintName("FK_ShowTimeSeats_Users");

            entity.HasOne(d => d.Seat).WithMany(p => p.ShowTimeSeats)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShowTimeSeats_Seats");

            entity.HasOne(d => d.ShowTime).WithMany(p => p.ShowTimeSeats)
                .HasForeignKey(d => d.ShowTimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShowTimeSeats_ShowTimes");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasIndex(e => e.BookingId, "IX_Tickets_BookingId");

            entity.HasIndex(e => new { e.ShowTimeId, e.SeatId }, "UQ_Tickets_ShowTime_Seat").IsUnique();

            entity.HasIndex(e => e.TicketCode, "UQ_Tickets_TicketCode").IsUnique();

            entity.Property(e => e.TicketId).ValueGeneratedNever();
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.TicketCode).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Booking).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Bookings");

            entity.HasOne(d => d.Seat).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Seats");

            entity.HasOne(d => d.ShowTime).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ShowTimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_ShowTimes");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasPrecision(6)
                .HasDefaultValueSql("now()");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.LastUpdatedAt).HasPrecision(6);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status).HasDefaultValue(1);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRoles_Roles"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRoles_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("UserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_UserRoles_RoleId");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
