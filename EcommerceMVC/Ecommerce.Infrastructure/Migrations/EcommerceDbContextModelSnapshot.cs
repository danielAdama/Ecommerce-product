﻿// <auto-generated />
using System;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations
{
    [DbContext(typeof(EcommerceDbContext))]
    partial class EcommerceDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "uuid-ossp");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EcommerceMVC.Data.Category", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("TimeCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("TimeUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            DisplayOrder = 1,
                            Name = "Laptop",
                            TimeCreated = new DateTimeOffset(new DateTime(2022, 12, 21, 6, 40, 4, 835, DateTimeKind.Unspecified).AddTicks(6981), new TimeSpan(0, 0, 0, 0, 0)),
                            TimeUpdated = new DateTimeOffset(new DateTime(2022, 12, 21, 6, 40, 4, 835, DateTimeKind.Unspecified).AddTicks(6979), new TimeSpan(0, 0, 0, 0, 0))
                        });
                });

            modelBuilder.Entity("EcommerceMVC.Data.Product", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("CategoryId")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<double>("Price")
                        .HasColumnType("double precision");

                    b.Property<DateTimeOffset>("TimeCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("TimeUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("EcommerceMVC.Data.Product", b =>
                {
                    b.HasOne("EcommerceMVC.Data.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });
#pragma warning restore 612, 618
        }
    }
}
