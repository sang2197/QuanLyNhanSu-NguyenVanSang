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
                name: "HrEmployee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    JoinDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrEmployee", x => x.Id);
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
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryScale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HrSalaryDecision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewPeriodId = table.Column<int>(type: "int", nullable: false),
                    DecisionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DecisionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SignerEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryDecision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecision_HrEmployee_SignerEmployeeId",
                        column: x => x.SignerEmployeeId,
                        principalTable: "HrEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    Coefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
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
                    SalaryScaleId = table.Column<int>(type: "int", nullable: false),
                    SalaryGradeId = table.Column<int>(type: "int", nullable: false),
                    Coefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DecisionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                        name: "FK_HrEmployeeSalary_HrSalaryDecision_DecisionId",
                        column: x => x.DecisionId,
                        principalTable: "HrSalaryDecision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrEmployeeSalary_HrSalaryGrade_SalaryGradeId",
                        column: x => x.SalaryGradeId,
                        principalTable: "HrSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrEmployeeSalary_HrSalaryScale_SalaryScaleId",
                        column: x => x.SalaryScaleId,
                        principalTable: "HrSalaryScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HrSalaryDecisionDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DecisionId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    OldSalaryId = table.Column<int>(type: "int", nullable: true),
                    OldGradeId = table.Column<int>(type: "int", nullable: true),
                    OldCoefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    NewSalaryGradeId = table.Column<int>(type: "int", nullable: false),
                    NewCoefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryDecisionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecisionDetail_HrEmployeeSalary_OldSalaryId",
                        column: x => x.OldSalaryId,
                        principalTable: "HrEmployeeSalary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecisionDetail_HrEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "HrEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecisionDetail_HrSalaryDecision_DecisionId",
                        column: x => x.DecisionId,
                        principalTable: "HrSalaryDecision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecisionDetail_HrSalaryGrade_NewSalaryGradeId",
                        column: x => x.NewSalaryGradeId,
                        principalTable: "HrSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryDecisionDetail_HrSalaryGrade_OldGradeId",
                        column: x => x.OldGradeId,
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
                    CurrentSalaryId = table.Column<int>(type: "int", nullable: false),
                    CurrentGradeId = table.Column<int>(type: "int", nullable: false),
                    ProposedGradeId = table.Column<int>(type: "int", nullable: true),
                    CurrentCoefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ProposedCoefficient = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    EligibilityStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EligibilityReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrSalaryReviewEmployee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrSalaryReviewEmployee_HrEmployeeSalary_CurrentSalaryId",
                        column: x => x.CurrentSalaryId,
                        principalTable: "HrEmployeeSalary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryReviewEmployee_HrEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "HrEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryReviewEmployee_HrSalaryGrade_CurrentGradeId",
                        column: x => x.CurrentGradeId,
                        principalTable: "HrSalaryGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HrSalaryReviewEmployee_HrSalaryGrade_ProposedGradeId",
                        column: x => x.ProposedGradeId,
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
                name: "IX_HrEmployee_EmployeeCode",
                table: "HrEmployee",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployeeSalary_DecisionId",
                table: "HrEmployeeSalary",
                column: "DecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployeeSalary_EmployeeId",
                table: "HrEmployeeSalary",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployeeSalary_SalaryGradeId",
                table: "HrEmployeeSalary",
                column: "SalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrEmployeeSalary_SalaryScaleId",
                table: "HrEmployeeSalary",
                column: "SalaryScaleId");

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
                name: "IX_HrSalaryDecision_SignerEmployeeId",
                table: "HrSalaryDecision",
                column: "SignerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecisionDetail_DecisionId",
                table: "HrSalaryDecisionDetail",
                column: "DecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecisionDetail_EmployeeId",
                table: "HrSalaryDecisionDetail",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecisionDetail_NewSalaryGradeId",
                table: "HrSalaryDecisionDetail",
                column: "NewSalaryGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecisionDetail_OldGradeId",
                table: "HrSalaryDecisionDetail",
                column: "OldGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryDecisionDetail_OldSalaryId",
                table: "HrSalaryDecisionDetail",
                column: "OldSalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryGrade_SalaryScaleId_GradeNumber",
                table: "HrSalaryGrade",
                columns: new[] { "SalaryScaleId", "GradeNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewEmployee_CurrentGradeId",
                table: "HrSalaryReviewEmployee",
                column: "CurrentGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewEmployee_CurrentSalaryId",
                table: "HrSalaryReviewEmployee",
                column: "CurrentSalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewEmployee_EmployeeId",
                table: "HrSalaryReviewEmployee",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HrSalaryReviewEmployee_ProposedGradeId",
                table: "HrSalaryReviewEmployee",
                column: "ProposedGradeId");

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
                name: "IX_HrSalaryScale_Code",
                table: "HrSalaryScale",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrSalaryDecisionDetail");

            migrationBuilder.DropTable(
                name: "HrSalaryReviewEmployee");

            migrationBuilder.DropTable(
                name: "HrEmployeeSalary");

            migrationBuilder.DropTable(
                name: "HrSalaryDecision");

            migrationBuilder.DropTable(
                name: "HrSalaryGrade");

            migrationBuilder.DropTable(
                name: "HrEmployee");

            migrationBuilder.DropTable(
                name: "HrSalaryReviewPeriod");

            migrationBuilder.DropTable(
                name: "HrSalaryScale");
        }
    }
}
