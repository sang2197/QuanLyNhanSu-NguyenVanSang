CREATE TABLE [HrEmployee] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [EmployeeCode] varchar(50) UNIQUE NOT NULL,
  [FullName] varchar(255) NOT NULL,
  [DepartmentId] int,
  [PositionId] int,
  [JoinDate] date,
  [Status] varchar(30),
  [CreatedAt] datetime,
  [UpdatedAt] datetime
)
GO

CREATE TABLE [HrSalaryScale] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [Code] varchar(50) UNIQUE NOT NULL,
  [Name] varchar(255) NOT NULL,
  [Description] varchar(500),
  [EffectiveFrom] date NOT NULL,
  [EffectiveTo] date,
  [Status] varchar(30),
  [CreatedAt] datetime,
  [UpdatedAt] datetime
)
GO

CREATE TABLE [HrSalaryGrade] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [SalaryScaleId] int NOT NULL,
  [GradeNumber] int NOT NULL,
  [Coefficient] decimal(5,2) NOT NULL,
  [EffectiveFrom] date NOT NULL,
  [EffectiveTo] date,
  [Status] varchar(30),
  [CreatedAt] datetime,
  [UpdatedAt] datetime
)
GO

CREATE TABLE [HrEmployeeSalary] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [EmployeeId] int NOT NULL,
  [SalaryScaleId] int NOT NULL,
  [SalaryGradeId] int NOT NULL,
  [Coefficient] decimal(5,2) NOT NULL,
  [EffectiveFrom] date NOT NULL,
  [EffectiveTo] date,
  [Reason] varchar(255),
  [DecisionId] int,
  [CreatedAt] datetime,
  [UpdatedAt] datetime
)
GO

CREATE TABLE [HrSalaryReviewPeriod] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [Code] varchar(50) UNIQUE NOT NULL,
  [Name] varchar(255) NOT NULL,
  [ReviewType] varchar(30) NOT NULL,
  [ReviewDate] date NOT NULL,
  [EffectiveDate] date,
  [Status] varchar(30) NOT NULL,
  [Description] varchar(500),
  [CreatedAt] datetime,
  [UpdatedAt] datetime
)
GO

CREATE TABLE [HrSalaryReviewEmployee] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [ReviewPeriodId] int NOT NULL,
  [EmployeeId] int NOT NULL,
  [CurrentSalaryId] int NOT NULL,
  [CurrentGradeId] int NOT NULL,
  [ProposedGradeId] int,
  [CurrentCoefficient] decimal(5,2),
  [ProposedCoefficient] decimal(5,2),
  [EligibilityStatus] varchar(30),
  [EligibilityReason] varchar(500),
  [ReviewStatus] varchar(30) NOT NULL,
  [Reason] varchar(500),
  [ApprovedAt] datetime,
  [ApprovedBy] int,
  [CreatedAt] datetime,
  [UpdatedAt] datetime
)
GO

CREATE TABLE [HrSalaryDecision] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [DecisionNumber] varchar(100) UNIQUE NOT NULL,
  [DecisionDate] date NOT NULL,
  [EffectiveDate] date NOT NULL,
  [DecisionType] varchar(30) NOT NULL,
  [Status] varchar(30) NOT NULL,
  [SignerEmployeeId] int,
  [Description] varchar(500),
  [FileUrl] varchar(500),
  [CreatedAt] datetime,
  [UpdatedAt] datetime
)
GO

CREATE TABLE [HrSalaryDecisionDetail] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [DecisionId] int NOT NULL,
  [EmployeeId] int NOT NULL,
  [OldSalaryId] int,
  [OldGradeId] int,
  [OldCoefficient] decimal(5,2),
  [NewSalaryGradeId] int NOT NULL,
  [NewCoefficient] decimal(5,2) NOT NULL,
  [EffectiveFrom] date NOT NULL,
  [Reason] varchar(500),
  [CreatedAt] datetime,
  [UpdatedAt] datetime
)
GO

CREATE UNIQUE INDEX [HrSalaryGrade_index_0] ON [HrSalaryGrade] ("SalaryScaleId", "GradeNumber")
GO

CREATE UNIQUE INDEX [HrSalaryReviewEmployee_index_1] ON [HrSalaryReviewEmployee] ("ReviewPeriodId", "EmployeeId")
GO

ALTER TABLE [HrSalaryGrade] ADD FOREIGN KEY ([SalaryScaleId]) REFERENCES [HrSalaryScale] ([Id])
GO

ALTER TABLE [HrEmployeeSalary] ADD FOREIGN KEY ([EmployeeId]) REFERENCES [HrEmployee] ([Id])
GO

ALTER TABLE [HrEmployeeSalary] ADD FOREIGN KEY ([SalaryScaleId]) REFERENCES [HrSalaryScale] ([Id])
GO

ALTER TABLE [HrEmployeeSalary] ADD FOREIGN KEY ([SalaryGradeId]) REFERENCES [HrSalaryGrade] ([Id])
GO

ALTER TABLE [HrEmployeeSalary] ADD FOREIGN KEY ([DecisionId]) REFERENCES [HrSalaryDecision] ([Id])
GO

ALTER TABLE [HrSalaryReviewEmployee] ADD FOREIGN KEY ([ReviewPeriodId]) REFERENCES [HrSalaryReviewPeriod] ([Id])
GO

ALTER TABLE [HrSalaryReviewEmployee] ADD FOREIGN KEY ([EmployeeId]) REFERENCES [HrEmployee] ([Id])
GO

ALTER TABLE [HrSalaryReviewEmployee] ADD FOREIGN KEY ([CurrentSalaryId]) REFERENCES [HrEmployeeSalary] ([Id])
GO

ALTER TABLE [HrSalaryReviewEmployee] ADD FOREIGN KEY ([CurrentGradeId]) REFERENCES [HrSalaryGrade] ([Id])
GO

ALTER TABLE [HrSalaryReviewEmployee] ADD FOREIGN KEY ([ProposedGradeId]) REFERENCES [HrSalaryGrade] ([Id])
GO

ALTER TABLE [HrSalaryDecision] ADD FOREIGN KEY ([SignerEmployeeId]) REFERENCES [HrEmployee] ([Id])
GO

ALTER TABLE [HrSalaryDecisionDetail] ADD FOREIGN KEY ([DecisionId]) REFERENCES [HrSalaryDecision] ([Id])
GO

ALTER TABLE [HrSalaryDecisionDetail] ADD FOREIGN KEY ([EmployeeId]) REFERENCES [HrEmployee] ([Id])
GO

ALTER TABLE [HrSalaryDecisionDetail] ADD FOREIGN KEY ([OldSalaryId]) REFERENCES [HrEmployeeSalary] ([Id])
GO

ALTER TABLE [HrSalaryDecisionDetail] ADD FOREIGN KEY ([OldGradeId]) REFERENCES [HrSalaryGrade] ([Id])
GO

ALTER TABLE [HrSalaryDecisionDetail] ADD FOREIGN KEY ([NewSalaryGradeId]) REFERENCES [HrSalaryGrade] ([Id])
GO
