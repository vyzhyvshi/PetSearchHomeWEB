using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetSearchHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "individual_profiles",
                columns: table => new
                {
                    individual_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    last_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    city = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    district = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    additional_info = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_individual_profiles", x => x.individual_id);
                    table.ForeignKey(
                        name: "FK_individual_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "listings",
                columns: table => new
                {
                    listing_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    animal_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    breed = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    age_months = table.Column<int>(type: "integer", nullable: false),
                    sex = table.Column<int>(type: "integer", nullable: false),
                    size = table.Column<int>(type: "integer", nullable: false),
                    color = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    city = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    district = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listings", x => x.listing_id);
                    table.ForeignKey(
                        name: "FK_listings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reporter_id = table.Column<int>(type: "integer", nullable: false),
                    reported_type = table.Column<int>(type: "integer", nullable: false),
                    reported_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_reports_users_reporter_id",
                        column: x => x.reporter_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reviewer_id = table.Column<int>(type: "integer", nullable: false),
                    reviewed_id = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_reviews_users_reviewed_id",
                        column: x => x.reviewed_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shelter_profiles",
                columns: table => new
                {
                    shelter_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    contact_person = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    rating = table.Column<float>(type: "real", nullable: false, defaultValue: 0f)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shelter_profiles", x => x.shelter_id);
                    table.ForeignKey(
                        name: "FK_shelter_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    favorite_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    listing_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => x.favorite_id);
                    table.ForeignKey(
                        name: "FK_favorites_listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listings",
                        principalColumn: "listing_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favorites_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_infos",
                columns: table => new
                {
                    health_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    listing_id = table.Column<int>(type: "integer", nullable: false),
                    vaccinations = table.Column<string>(type: "text", nullable: true),
                    sterilized = table.Column<bool>(type: "boolean", nullable: false),
                    chronic_diseases = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_infos", x => x.health_id);
                    table.ForeignKey(
                        name: "FK_health_infos_listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listings",
                        principalColumn: "listing_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "photos",
                columns: table => new
                {
                    photo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    listing_id = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_photos", x => x.photo_id);
                    table.ForeignKey(
                        name: "FK_photos_listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listings",
                        principalColumn: "listing_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_favorites_listing_id",
                table: "favorites",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_user_id_listing_id",
                table: "favorites",
                columns: new[] { "user_id", "listing_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_health_infos_listing_id",
                table: "health_infos",
                column: "listing_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_individual_profiles_user_id",
                table: "individual_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_listings_city",
                table: "listings",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "IX_listings_status",
                table: "listings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_listings_user_id",
                table: "listings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_photos_listing_id",
                table: "photos",
                column: "listing_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_reporter_id",
                table: "reports",
                column: "reporter_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_reviewed_id",
                table: "reviews",
                column: "reviewed_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_reviewer_id",
                table: "reviews",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_shelter_profiles_user_id",
                table: "shelter_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "health_infos");

            migrationBuilder.DropTable(
                name: "individual_profiles");

            migrationBuilder.DropTable(
                name: "photos");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "shelter_profiles");

            migrationBuilder.DropTable(
                name: "listings");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
