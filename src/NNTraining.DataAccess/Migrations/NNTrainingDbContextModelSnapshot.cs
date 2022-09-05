﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NNTraining.DataAccess;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NNTraining.DataAccess.Migrations
{
    [DbContext(typeof(NNTrainingDbContext))]
    partial class NNTrainingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("NNTraining.Domain.Models.File", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Extension")
                        .HasColumnType("text");

                    b.Property<int>("FileType")
                        .HasColumnType("integer");

                    b.Property<string>("OriginalName")
                        .HasColumnType("text");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("NNTraining.Domain.Models.Model", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ModelStatus")
                        .HasColumnType("integer");

                    b.Property<int>("ModelType")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("PairFieldType")
                        .HasColumnType("text");

                    b.Property<string>("Parameters")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Models");
                });

            modelBuilder.Entity("NNTraining.Domain.Models.ModelFile", b =>
                {
                    b.Property<Guid>("ModelId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("FileId")
                        .HasColumnType("uuid");

                    b.HasKey("ModelId", "FileId");

                    b.ToTable("ModelFiles");
                });
#pragma warning restore 612, 618
        }
    }
}
