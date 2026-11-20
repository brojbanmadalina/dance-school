using DanceSchool.DataAccess.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DanceSchool.DataAccess.Migrations
{
    [DbContext(typeof(DanceSchoolDbContext))]
    [Migration("20260927090000_DropAdjustStudentPointsFunction")]
    public partial class DropAdjustStudentPointsFunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS adjust_student_points(uuid, integer, timestamptz);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION adjust_student_points(
                    p_student_id uuid,
                    p_delta integer,
                    p_now timestamptz)
                RETURNS void
                LANGUAGE sql
                AS $$
                    INSERT INTO student_points (id, student_id, points, updated_at)
                    VALUES (gen_random_uuid(), p_student_id, GREATEST(0, p_delta), p_now)
                    ON CONFLICT (student_id) DO UPDATE
                    SET points = GREATEST(0, student_points.points + p_delta),
                        updated_at = p_now;
                $$;
                """
            );
        }
    }
}
