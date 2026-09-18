using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HrBaseSalaryRate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rate = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrBaseSalaryRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrJobTitle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrJobTitle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrOrganizationalUnit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    UnitType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrOrganizationalUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrOrganizationalUnit_HrOrganizationalUnit_ParentId",
                        column: x => x.ParentId,
                        principalTable: "HrOrganizationalUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrSalaryReviewPeriod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ReviewType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReviewDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryReviewPeriod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrSalaryScale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryScale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrEmployee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OrganizationalUnitId = table.Column<int>(type: "int", nullable: false),
                    JobTitleId = table.Column<int>(type: "int", nullable: false),
                    JoinDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrEmployee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrEmployee_HrJobTitle_JobTitleId",
                        column: x => x.JobTitleId,
                        principalTable: "HrJobTitle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrEmployee_HrOrganizationalUnit_OrganizationalUnitId",
                        column: x => x.OrganizationalUnitId,
                        principalTable: "HrOrganizationalUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrSalaryDecision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewPeriodId = table.Column<int>(type: "int", nullable: false),
                    DecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryDecision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecision_HrSalaryReviewPeriod_ReviewPeriodId",
                        column: x => x.ReviewPeriodId,
                        principalTable: "HrSalaryReviewPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrSalaryGrade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryScaleId = table.Column<int>(type: "int", nullable: false),
                    GradeNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryGrade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrSalaryGrade_HrSalaryScale_SalaryScaleId",
                        column: x => x.SalaryScaleId,
                        principalTable: "HrSalaryScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrEmployeeSalary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SalaryGradeId = table.Column<int>(type: "int", nullable: false),
                    Coefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SalaryDecisionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrEmployeeSalary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrEmployeeSalary_HrEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "HrEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrEmployeeSalary_HrSalaryDecision_SalaryDecisionId",
                        column: x => x.SalaryDecisionId,
                        principalTable: "HrSalaryDecision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrEmployeeSalary_HrSalaryGrade_SalaryGradeId",
                        column: x => x.SalaryGradeId,
                        principalTable: "HrSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrSalaryDecisionDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryDecisionId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    BaselineSalaryGradeId = table.Column<int>(type: "int", nullable: false),
                    BaselineCoefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    NewSalaryGradeId = table.Column<int>(type: "int", nullable: false),
                    NewCoefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryDecisionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecisionDetail_HrEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "HrEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecisionDetail_HrSalaryDecision_SalaryDecisionId",
                        column: x => x.SalaryDecisionId,
                        principalTable: "HrSalaryDecision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecisionDetail_HrSalaryGrade_BaselineSalaryGradeId",
                        column: x => x.BaselineSalaryGradeId,
                        principalTable: "HrSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecisionDetail_HrSalaryGrade_NewSalaryGradeId",
                        column: x => x.NewSalaryGradeId,
                        principalTable: "HrSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrSalaryGradeCoefficient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryGradeId = table.Column<int>(type: "int", nullable: false),
                    Coefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryGradeCoefficient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrSalaryGradeCoefficient_HrSalaryGrade_SalaryGradeId",
                        column: x => x.SalaryGradeId,
                        principalTable: "HrSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrSalaryReviewEmployee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewPeriodId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CurrentSalaryGradeId = table.Column<int>(type: "int", nullable: false),
                    CurrentCoefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Eligible = table.Column<bool>(type: "bit", nullable: false),
                    ProposedSalaryGradeId = table.Column<int>(type: "int", nullable: true),
                    ProposedCoefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IneligibleReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryReviewEmployee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrSalaryReviewEmployee_HrEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "HrEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryReviewEmployee_HrSalaryGrade_CurrentSalaryGradeId",
                        column: x => x.CurrentSalaryGradeId,
                        principalTable: "HrSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryReviewEmployee_HrSalaryGrade_ProposedSalaryGradeId",
                        column: x => x.ProposedSalaryGradeId,
                        principalTable: "HrSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryReviewEmployee_HrSalaryReviewPeriod_ReviewPeriodId",
                        column: x => x.ReviewPeriodId,
                        principalTable: "HrSalaryReviewPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HrBaseSalaryRate_EffectiveDate",
                table: "HrBaseSalaryRate",
                column: "EffectiveDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployee_EmployeeCode",
                table: "HrEmployee",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployee_JobTitleId",
                table: "HrEmployee",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployee_OrganizationalUnitId",
                table: "HrEmployee",
                column: "OrganizationalUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployeeSalary_EmployeeId_EffectiveDate",
                table: "HrEmployeeSalary",
                columns: new[] { "EmployeeId", "EffectiveDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployeeSalary_SalaryDecisionId",
                table: "HrEmployeeSalary",
                column: "SalaryDecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployeeSalary_SalaryGradeId",
                table: "HrEmployeeSalary",
                column: "SalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrJobTitle_Name",
                table: "HrJobTitle",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrOrganizationalUnit_ParentId_Name",
                table: "HrOrganizationalUnit",
                columns: new[] { "ParentId", "Name" },
                unique: true,
                filter: "[ParentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "HrSalaryDecision_index_noncancelled_period",
                table: "HrSalaryDecision",
                column: "ReviewPeriodId",
                unique: true,
                filter: "[Status] <> 'CANCELLED'");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecision_DecisionNumber",
                table: "HrSalaryDecision",
                column: "DecisionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecisionDetail_BaselineSalaryGradeId",
                table: "HrSalaryDecisionDetail",
                column: "BaselineSalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecisionDetail_EmployeeId",
                table: "HrSalaryDecisionDetail",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecisionDetail_NewSalaryGradeId",
                table: "HrSalaryDecisionDetail",
                column: "NewSalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecisionDetail_SalaryDecisionId_EmployeeId",
                table: "HrSalaryDecisionDetail",
                columns: new[] { "SalaryDecisionId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryGrade_SalaryScaleId_GradeNumber",
                table: "HrSalaryGrade",
                columns: new[] { "SalaryScaleId", "GradeNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryGradeCoefficient_SalaryGradeId_EffectiveDate",
                table: "HrSalaryGradeCoefficient",
                columns: new[] { "SalaryGradeId", "EffectiveDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewEmployee_CurrentSalaryGradeId",
                table: "HrSalaryReviewEmployee",
                column: "CurrentSalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewEmployee_EmployeeId",
                table: "HrSalaryReviewEmployee",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewEmployee_ProposedSalaryGradeId",
                table: "HrSalaryReviewEmployee",
                column: "ProposedSalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewEmployee_ReviewPeriodId_EmployeeId",
                table: "HrSalaryReviewEmployee",
                columns: new[] { "ReviewPeriodId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewPeriod_Code",
                table: "HrSalaryReviewPeriod",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewPeriod_Name",
                table: "HrSalaryReviewPeriod",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryScale_Code",
                table: "HrSalaryScale",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryScale_Name",
                table: "HrSalaryScale",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrBaseSalaryRate");

            migrationBuilder.DropTable(
                name: "HrEmployeeSalary");

            migrationBuilder.DropTable(
                name: "HrSalaryDecisionDetail");

            migrationBuilder.DropTable(
                name: "HrSalaryGradeCoefficient");

            migrationBuilder.DropTable(
                name: "HrSalaryReviewEmployee");

            migrationBuilder.DropTable(
                name: "HrSalaryDecision");

            migrationBuilder.DropTable(
                name: "HrEmployee");

            migrationBuilder.DropTable(
                name: "HrSalaryGrade");

            migrationBuilder.DropTable(
                name: "HrSalaryReviewPeriod");

            migrationBuilder.DropTable(
                name: "HrJobTitle");

            migrationBuilder.DropTable(
                name: "HrOrganizationalUnit");

            migrationBuilder.DropTable(
                name: "HrSalaryScale");
        }
    }
}
