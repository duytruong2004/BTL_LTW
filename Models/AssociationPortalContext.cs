using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW.Models;

public partial class AssociationPortalContext : DbContext
{
    public AssociationPortalContext()
    {
    }

    public AssociationPortalContext(DbContextOptions<AssociationPortalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MemberPermision> MemberPermisions { get; set; }

    public virtual DbSet<Permision> Permisions { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A2BA8DBDB30");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(200);
            entity.Property(e => e.CategoryStatus).HasDefaultValue(0);
            entity.Property(e => e.Decription).HasMaxLength(500);
            entity.Property(e => e.MemberId).HasColumnName("MemberID");

            entity.HasOne(d => d.Member).WithMany(p => p.Categories)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK__Category__Member__5535A963");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MembersId).HasName("PK__Members__5457C04B56B0F03C");

            entity.HasIndex(e => e.Emai, "UQ__Members__DCB4F34A5BA36B37").IsUnique();

            entity.Property(e => e.MembersId).HasColumnName("MembersID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Emai)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MemberPermision>(entity =>
        {
            entity.HasKey(e => e.MemberPermisionId).HasName("PK__MemberPe__B308D7CC8F0958D3");

            entity.ToTable("MemberPermision");

            entity.Property(e => e.MemberPermisionId).HasColumnName("MemberPermisionID");
            entity.Property(e => e.Licensed).HasDefaultValue(false);
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.PermisionId).HasColumnName("PermisionID");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberPermisions)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK__MemberPer__Membe__5165187F");

            entity.HasOne(d => d.Permision).WithMany(p => p.MemberPermisions)
                .HasForeignKey(d => d.PermisionId)
                .HasConstraintName("FK__MemberPer__Permi__5070F446");
        });

        modelBuilder.Entity<Permision>(entity =>
        {
            entity.HasKey(e => e.PermisionId).HasName("PK__Permisio__18284E4D256502A8");

            entity.ToTable("Permision");

            entity.Property(e => e.PermisionId).HasColumnName("PermisionID");
            entity.Property(e => e.PermisionName).HasMaxLength(100);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Post__AA126038C7BAE74B");

            entity.ToTable("Post");

            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.ApproveStatus).HasDefaultValue(0);
            entity.Property(e => e.ApprovedDate).HasColumnType("datetime");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.PostStatus).HasDefaultValue(0);
            entity.Property(e => e.PostThumbnailUrl).HasColumnName("PostThumbnailURL");
            entity.Property(e => e.PostTitle).HasMaxLength(300);
            entity.Property(e => e.RejectedReason).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.ViewCount).HasDefaultValue(0);

            entity.HasOne(d => d.ApproveByNavigation).WithMany(p => p.PostApproveByNavigations)
                .HasForeignKey(d => d.ApproveBy)
                .HasConstraintName("FK__Post__ApproveBy__5DCAEF64");

            entity.HasOne(d => d.Category).WithMany(p => p.Posts)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Post__CategoryID__5BE2A6F2");

            entity.HasOne(d => d.Member).WithMany(p => p.PostMembers)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK__Post__MemberID__5CD6CB2B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
