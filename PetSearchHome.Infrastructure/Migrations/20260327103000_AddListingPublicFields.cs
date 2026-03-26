using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetSearchHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddListingPublicFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'listings' AND column_name = 'domain_id'
                    ) THEN
                        ALTER TABLE listings ADD COLUMN domain_id uuid DEFAULT gen_random_uuid();
                        UPDATE listings SET domain_id = gen_random_uuid() WHERE domain_id IS NULL;
                        ALTER TABLE listings ALTER COLUMN domain_id SET NOT NULL;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'listings' AND column_name = 'is_urgent'
                    ) THEN
                        ALTER TABLE listings ADD COLUMN is_urgent boolean NOT NULL DEFAULT FALSE;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'listings' AND column_name = 'location'
                    ) THEN
                        ALTER TABLE listings ADD COLUMN location character varying(256) NOT NULL DEFAULT '';
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'listings' AND column_name = 'title'
                    ) THEN
                        ALTER TABLE listings ADD COLUMN title character varying(128) NOT NULL DEFAULT '';
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                UPDATE listings
                SET title = COALESCE(NULLIF(title, ''), NULLIF(breed, ''), animal_type),
                    location = COALESCE(NULLIF(location, ''), NULLIF(city, ''), 'Unknown'),
                    is_urgent = COALESCE(is_urgent, false);
                """);

            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS IX_listings_domain_id ON listings (domain_id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_listings_domain_id\";");
            migrationBuilder.Sql("ALTER TABLE listings DROP COLUMN IF EXISTS domain_id;");
            migrationBuilder.Sql("ALTER TABLE listings DROP COLUMN IF EXISTS is_urgent;");
            migrationBuilder.Sql("ALTER TABLE listings DROP COLUMN IF EXISTS location;");
            migrationBuilder.Sql("ALTER TABLE listings DROP COLUMN IF EXISTS title;");
        }
    }
}
