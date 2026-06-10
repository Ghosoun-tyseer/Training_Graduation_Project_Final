using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SMEMS.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Accessory> Accessories { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceAccessory> DeviceAccessories { get; set; }

    public virtual DbSet<DeviceWarranty> DeviceWarranties { get; set; }

    public virtual DbSet<Engineer> Engineers { get; set; }

    public virtual DbSet<MaintenancePart> MaintenanceParts { get; set; }

    public virtual DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

    public virtual DbSet<MaintenanceType> MaintenanceTypes { get; set; }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQL2019;Database=SMEMS_V3;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accessory>(entity =>
        {
            entity.HasKey(e => e.AccessoryId).HasName("PK__Accessor__09C3F09BD724C454");

            entity.HasIndex(e => e.Name, "UQ__Accessor__737584F6D6FFDD0D").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admins__719FE488D7C1401D");

            entity.HasIndex(e => e.Username, "UQ__Admins__536C85E40FF3D028").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Admins__A9D10534C3B5E9A0").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BED677C8B5F");

            entity.HasIndex(e => e.Name, "UQ__Departme__737584F6B5172367").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("PK__Devices__49E1231107174EBE");

            entity.HasIndex(e => e.DepartmentId, "IX_Devices_DepartmentId");

            entity.HasIndex(e => e.RiskLevel, "IX_Devices_RiskLevel");

            entity.HasIndex(e => e.Status, "IX_Devices_Status");

            entity.HasIndex(e => e.SerialNumber, "UQ__Devices__048A000842AA1F77").IsUnique();

            entity.HasIndex(e => e.DeviceCode, "UQ__Devices__AFFB3E95E294C242").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DeviceCode).HasMaxLength(50);
            entity.Property(e => e.ExpectedLifespan).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.ModelNumber).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.RiskLevel)
                .HasMaxLength(50)
                .HasDefaultValue("Medium");
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Operational");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Department).WithMany(p => p.Devices)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Devices__Departm__5165187F");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Devices)
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("FK__Devices__Manufac__4F7CD00D");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Devices)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__Devices__Supplie__5070F446");
        });

        modelBuilder.Entity<DeviceAccessory>(entity =>
        {
            entity.HasKey(e => e.DeviceAccessoryId).HasName("PK__DeviceAc__018393A51B65E3DE");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Accessory).WithMany(p => p.DeviceAccessories)
                .HasForeignKey(d => d.AccessoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DeviceAcc__Acces__619B8048");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceAccessories)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DeviceAcc__Devic__60A75C0F");
        });

        modelBuilder.Entity<DeviceWarranty>(entity =>
        {
            entity.HasKey(e => e.WarrantyId).HasName("PK__DeviceWa__2ED318137B434574");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.WarrantyProvider).HasMaxLength(150);

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceWarranties)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DeviceWar__Devic__05D8E0BE");
        });

        modelBuilder.Entity<Engineer>(entity =>
        {
            entity.HasKey(e => e.EngineerId).HasName("PK__Engineer__1FA0F1CE9B2B5847");

            entity.HasIndex(e => e.Username, "UQ__Engineer__536C85E4DB66C038").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Engineer__A9D10534F6BB9850").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<MaintenancePart>(entity =>
        {
            entity.HasKey(e => e.MaintenancePartId).HasName("PK__Maintena__A6092A6E4D6C9443");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.QuantityUsed).HasDefaultValue(1);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaintenanceRequest).WithMany(p => p.MaintenanceParts)
                .HasForeignKey(d => d.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__Maint__7F2BE32F");

            entity.HasOne(d => d.Part).WithMany(p => p.MaintenanceParts)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__PartI__00200768");
        });

        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Maintena__33A8517A125B2753");

            entity.HasIndex(e => e.DeviceId, "IX_MaintenanceRequests_DeviceId");

            entity.HasIndex(e => e.Status, "IX_MaintenanceRequests_Status");

            entity.HasIndex(e => e.RequestCode, "UQ__Maintena__CBAB82F6E19E3E63").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IssueTitle).HasMaxLength(300);
            entity.Property(e => e.RequestCode).HasMaxLength(50);
            entity.Property(e => e.RequestDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RiskLevel)
                .HasMaxLength(50)
                .HasDefaultValue("Medium");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.AssignedEngineer).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.AssignedEngineerId)
                .HasConstraintName("FK__Maintenan__Assig__6C190EBB");

            entity.HasOne(d => d.Device).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Maintenan__Devic__6A30C649");

            entity.HasOne(d => d.MaintenanceType).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.MaintenanceTypeId)
                .HasConstraintName("FK__Maintenan__Maint__6D0D32F4");

            entity.HasOne(d => d.ReportedByStaff).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.ReportedByStaffId)
                .HasConstraintName("FK__Maintenan__Repor__6B24EA82");
        });

        modelBuilder.Entity<MaintenanceType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__Maintena__516F03B589F51259");

            entity.HasIndex(e => e.Name, "UQ__Maintena__737584F6C8C84027").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.ManufacturerId).HasName("PK__Manufact__357E5CC1E2130A1C");

            entity.HasIndex(e => e.Name, "UQ__Manufact__737584F6A06300E2").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.ContactPerson).HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Website).HasMaxLength(200);
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("PK__Parts__7C3F0D50346BD493");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PartNumber).HasMaxLength(100);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Parts)
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("FK__Parts__Manufactu__76969D2E");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Reports__D5BD48056F870FE0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Format).HasMaxLength(50);
            entity.Property(e => e.ReportType).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.GeneratedByAdmin).WithMany(p => p.Reports)
                .HasForeignKey(d => d.GeneratedByAdminId)
                .HasConstraintName("FK__Reports__Generat__09A971A2");

            entity.HasOne(d => d.GeneratedByEngineer).WithMany(p => p.Reports)
                .HasForeignKey(d => d.GeneratedByEngineerId)
                .HasConstraintName("FK__Reports__Generat__0A9D95DB");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staff__96D4AB17733699CA");

            entity.HasIndex(e => e.DepartmentId, "IX_Staff_DepartmentId");

            entity.HasIndex(e => e.Username, "UQ__Staff__536C85E43E408E91").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Staff__A9D10534126A0EF5").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Department).WithMany(p => p.Staff)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Staff__Departmen__3B75D760");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE666B49559DA3E");

            entity.HasIndex(e => e.Name, "UQ__Supplier__737584F641EA3D12").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.ContactPerson).HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Website).HasMaxLength(200);
        });



        OnModelCreatingPartial(modelBuilder);


    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
