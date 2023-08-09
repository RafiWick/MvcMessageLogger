﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MvcMessageLogger.DataAccess;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MvcMessageLogger.Migrations
{
    [DbContext(typeof(MvcMessageLoggerContext))]
    partial class MvcMessageLoggerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MvcMessageLogger.Models.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer")
                        .HasColumnName("author_id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<bool>("Edited")
                        .HasColumnType("boolean")
                        .HasColumnName("edited");

                    b.HasKey("Id")
                        .HasName("pk_messages");

                    b.HasIndex("AuthorId")
                        .HasDatabaseName("ix_messages_author_id");

                    b.ToTable("messages", (string)null);
                });

            modelBuilder.Entity("MvcMessageLogger.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<bool>("LoggedIn")
                        .HasColumnType("boolean")
                        .HasColumnName("logged_in");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("UserUser", b =>
                {
                    b.Property<int>("FollowersId")
                        .HasColumnType("integer")
                        .HasColumnName("followers_id");

                    b.Property<int>("FollowingId")
                        .HasColumnType("integer")
                        .HasColumnName("following_id");

                    b.HasKey("FollowersId", "FollowingId")
                        .HasName("pk_user_user");

                    b.HasIndex("FollowingId")
                        .HasDatabaseName("ix_user_user_following_id");

                    b.ToTable("user_user", (string)null);
                });

            modelBuilder.Entity("MvcMessageLogger.Models.Message", b =>
                {
                    b.HasOne("MvcMessageLogger.Models.User", "Author")
                        .WithMany("Messages")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_messages_users_author_id");

                    b.Navigation("Author");
                });

            modelBuilder.Entity("UserUser", b =>
                {
                    b.HasOne("MvcMessageLogger.Models.User", null)
                        .WithMany()
                        .HasForeignKey("FollowersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_user_users_followers_id");

                    b.HasOne("MvcMessageLogger.Models.User", null)
                        .WithMany()
                        .HasForeignKey("FollowingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_user_users_following_id");
                });

            modelBuilder.Entity("MvcMessageLogger.Models.User", b =>
                {
                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
