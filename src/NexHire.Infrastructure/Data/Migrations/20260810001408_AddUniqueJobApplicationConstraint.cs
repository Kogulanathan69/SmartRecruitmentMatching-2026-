using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexHire.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueJobApplicationConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CandidateId_VacancyId",
                table: "JobApplications",
                columns: new[] { "CandidateId", "VacancyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_CandidateId_VacancyId",
                table: "JobApplications");
        }
    }
}
