CREATE DATABASE SMEMS_V3;
GO

USE SMEMS_V3;
GO

-- ============================================================
-- DEPARTMENTS
-- ============================================================
CREATE TABLE Departments
(
    DepartmentId INT PRIMARY KEY IDENTITY(1,1),
    Name         NVARCHAR(100)  NOT NULL UNIQUE,
    Description  NVARCHAR(500),
    IsDeleted    BIT            NOT NULL DEFAULT 0,
    CreatedAt    DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- ADMINS (بناءً على طلب المشرفة - منفصل)
-- ============================================================
CREATE TABLE Admins
(
    AdminId       INT PRIMARY KEY IDENTITY(1,1),
    FullName      NVARCHAR(150) NOT NULL,
    Username      NVARCHAR(100) NOT NULL UNIQUE,
    Email         NVARCHAR(200) NOT NULL UNIQUE,
    Phone         NVARCHAR(50),
    PasswordHash  NVARCHAR(500) NOT NULL,
    Avatar        NVARCHAR(300),
    Position      NVARCHAR(100),
    IsActive      BIT           NOT NULL DEFAULT 1,
    LastLoginAt   DATETIME2,
    IsDeleted     BIT           NOT NULL DEFAULT 0,
    CreatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- ENGINEERS (منفصل)
-- ============================================================
CREATE TABLE Engineers
(
    EngineerId    INT PRIMARY KEY IDENTITY(1,1),
    FullName      NVARCHAR(150) NOT NULL,
    Username      NVARCHAR(100) NOT NULL UNIQUE,
    Email         NVARCHAR(200) NOT NULL UNIQUE,
    Phone         NVARCHAR(50),
    PasswordHash  NVARCHAR(500) NOT NULL,
    Avatar        NVARCHAR(300),
    Position      NVARCHAR(100),
    IsActive      BIT           NOT NULL DEFAULT 1,
    LastLoginAt   DATETIME2,
    IsDeleted     BIT           NOT NULL DEFAULT 0,
    CreatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- STAFF (منفصل ويتبع لقسم)
-- ============================================================
CREATE TABLE Staff
(
    StaffId       INT PRIMARY KEY IDENTITY(1,1),
    FullName      NVARCHAR(150) NOT NULL,
    Username      NVARCHAR(100) NOT NULL UNIQUE,
    Email         NVARCHAR(200) NOT NULL UNIQUE,
    Phone         NVARCHAR(50),
    PasswordHash  NVARCHAR(500) NOT NULL,
    Avatar        NVARCHAR(300),
    Position      NVARCHAR(100),
    DepartmentId  INT           NULL
        REFERENCES Departments(DepartmentId),
    IsActive      BIT           NOT NULL DEFAULT 1,
    LastLoginAt   DATETIME2,
    IsDeleted     BIT           NOT NULL DEFAULT 0,
    CreatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- MANUFACTURERS
-- ============================================================
CREATE TABLE Manufacturers
(
    ManufacturerId INT PRIMARY KEY IDENTITY(1,1),
    Name           NVARCHAR(150) NOT NULL UNIQUE,
    ContactPerson  NVARCHAR(150),
    Email          NVARCHAR(150),
    Phone          NVARCHAR(50),
    Address        NVARCHAR(300),
    Website        NVARCHAR(200),
    Notes          NVARCHAR(MAX),
    IsDeleted      BIT           NOT NULL DEFAULT 0,
    CreatedAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- SUPPLIERS
-- ============================================================
CREATE TABLE Suppliers
(
    SupplierId    INT PRIMARY KEY IDENTITY(1,1),
    Name          NVARCHAR(150) NOT NULL UNIQUE,
    ContactPerson NVARCHAR(150),
    Email         NVARCHAR(150),
    Phone         NVARCHAR(50),
    Address       NVARCHAR(300),
    Website       NVARCHAR(200),
    Notes         NVARCHAR(MAX),
    IsDeleted     BIT           NOT NULL DEFAULT 0,
    CreatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- DEVICES (تم حذف حقول الضمان وحقول الـ CSS)
-- ============================================================
CREATE TABLE Devices
(
    DeviceId            INT PRIMARY KEY IDENTITY(1,1),
    DeviceCode          NVARCHAR(50)  NOT NULL UNIQUE,
    Name                NVARCHAR(200) NOT NULL,
    ModelNumber         NVARCHAR(100),
    SerialNumber        NVARCHAR(100) NOT NULL UNIQUE,
    ManufacturerId      INT NULL
        REFERENCES Manufacturers(ManufacturerId),
    SupplierId          INT NULL
        REFERENCES Suppliers(SupplierId),
    DepartmentId        INT NOT NULL
        REFERENCES Departments(DepartmentId),
    Status              NVARCHAR(50)  NOT NULL DEFAULT N'Operational',
    RiskLevel           NVARCHAR(50)  NOT NULL DEFAULT N'Medium',
    Location            NVARCHAR(200),
    PurchaseDate        DATE,
    ExpectedLifespan    NVARCHAR(100),
    FailureCount        INT           NOT NULL DEFAULT 0,
    NextMaintenanceDate DATE,	
    Notes               NVARCHAR(MAX),
    IsDeleted           BIT           NOT NULL DEFAULT 0,
    CreatedAt           DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- ACCESSORIES
-- ============================================================
CREATE TABLE Accessories
(
    AccessoryId     INT PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(150) NOT NULL UNIQUE,
    Description     NVARCHAR(500),
    QuantityInStock INT           NOT NULL DEFAULT 0,
    IsDeleted       BIT           NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- DEVICE ACCESSORIES
-- ============================================================
CREATE TABLE DeviceAccessories
(
    DeviceAccessoryId INT PRIMARY KEY IDENTITY(1,1),
    DeviceId          INT NOT NULL
        REFERENCES Devices(DeviceId),
    AccessoryId       INT NOT NULL
        REFERENCES Accessories(AccessoryId),
    Quantity          INT           NOT NULL DEFAULT 1,
    Notes             NVARCHAR(500),
    CreatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- MAINTENANCE TYPES
-- ============================================================
CREATE TABLE MaintenanceTypes
(
    TypeId INT PRIMARY KEY IDENTITY(1,1),
    Name   NVARCHAR(100) NOT NULL UNIQUE
);

-- ============================================================
-- MAINTENANCE REQUESTS (تم حذف حقول الـ CSS)
-- ============================================================
CREATE TABLE MaintenanceRequests
(
    RequestId              INT PRIMARY KEY IDENTITY(1,1),
    RequestCode            NVARCHAR(50)  NOT NULL UNIQUE,
    DeviceId               INT           NOT NULL
        REFERENCES Devices(DeviceId),
    ReportedByStaffId      INT           NULL
        REFERENCES Staff(StaffId),
    AssignedEngineerId     INT           NULL
        REFERENCES Engineers(EngineerId),
    MaintenanceTypeId      INT           NULL
        REFERENCES MaintenanceTypes(TypeId),
    Status                 NVARCHAR(50)  NOT NULL DEFAULT N'Pending',
    RiskLevel              NVARCHAR(50)  NOT NULL DEFAULT N'Medium',
    IssueTitle             NVARCHAR(300) NOT NULL,
    IssueDescription       NVARCHAR(MAX),
    EngineerNotes          NVARCHAR(MAX),
    CompletionNotes        NVARCHAR(MAX),
    HasAlternative         BIT           NOT NULL DEFAULT 0,
    RequestDate            DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    StartedAt              DATETIME2,
    CompletedAt            DATETIME2,
    IsDeleted              BIT           NOT NULL DEFAULT 0,
    CreatedAt              DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt              DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- PARTS 
-- ============================================================
CREATE TABLE Parts
(
    PartId            INT PRIMARY KEY IDENTITY(1,1),
    Name              NVARCHAR(150) NOT NULL,
    PartNumber        NVARCHAR(100),
    ManufacturerId    INT NULL
        REFERENCES Manufacturers(ManufacturerId),
    QuantityInStock   INT           NOT NULL DEFAULT 0,
    UnitPrice         DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    MinimumStockLevel INT           NOT NULL DEFAULT 0,
    Notes             NVARCHAR(MAX),
    IsDeleted         BIT           NOT NULL DEFAULT 0,
    CreatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- MAINTENANCE PARTS
-- ============================================================
CREATE TABLE MaintenanceParts
(
    MaintenancePartId    INT PRIMARY KEY IDENTITY(1,1),
    MaintenanceRequestId INT NOT NULL
        REFERENCES MaintenanceRequests(RequestId),
    PartId               INT NOT NULL
        REFERENCES Parts(PartId),
    QuantityUsed         INT           NOT NULL DEFAULT 1,
    UnitPrice            DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Notes                NVARCHAR(500),
    CreatedAt            DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- DEVICE WARRANTIES (تم الإبقاء عليه كما طلبت لتسجيل تواريخ الضمان المتعددة)
-- ============================================================
CREATE TABLE DeviceWarranties
(
    WarrantyId       INT PRIMARY KEY IDENTITY(1,1),
    DeviceId         INT NOT NULL
        REFERENCES Devices(DeviceId),
    StartDate        DATE          NOT NULL,
    EndDate          DATE          NOT NULL,
    WarrantyProvider NVARCHAR(150),
    Terms            NVARCHAR(MAX),
    CreatedAt        DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- REPORTS
-- ============================================================
CREATE TABLE Reports
(
    ReportId              INT PRIMARY KEY IDENTITY(1,1),
    Title                 NVARCHAR(200) NOT NULL,
    ReportType            NVARCHAR(100),
    Format                NVARCHAR(50),
    Parameters            NVARCHAR(MAX),
    GeneratedByAdminId    INT NULL
        REFERENCES Admins(AdminId),
    GeneratedByEngineerId INT NULL
        REFERENCES Engineers(EngineerId),
    CreatedAt             DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- INDEXES
-- ============================================================
CREATE INDEX IX_Devices_DepartmentId     ON Devices(DepartmentId);
CREATE INDEX IX_Devices_Status           ON Devices(Status);
CREATE INDEX IX_Devices_RiskLevel        ON Devices(RiskLevel);

CREATE INDEX IX_MaintenanceRequests_DeviceId  ON MaintenanceRequests(DeviceId);
CREATE INDEX IX_MaintenanceRequests_Status    ON MaintenanceRequests(Status);

CREATE INDEX IX_Staff_DepartmentId       ON Staff(DepartmentId);



INSERT INTO Departments (Name, Description) VALUES
('Radiology',              'Medical imaging and diagnostic services'),
('ICU',                    'Critical intensive care unit'),
('Emergency',              'Emergency and trauma response'),
('Cardiology',             'Heart and cardiovascular treatment'),
('Laboratory',             'Diagnostic and pathology laboratory'),
('Surgery',                'Operating rooms and surgical care'),
('Pediatrics',             'Child healthcare services'),
('NICU',                   'Neonatal intensive care'),
('Oncology',               'Cancer treatment center'),
('Dialysis',               'Renal dialysis treatment'),
('Respiratory Therapy',    'Respiratory support services'),
('Internal Medicine',      'General medical care'),
('Sterilization',          'Medical equipment sterilization'),
('Pharmacy',               'Medication dispensing services');




INSERT INTO Admins (FullName, Username, Email, Phone, PasswordHash, Position, LastLoginAt) VALUES
('Heba Nasser', 'heba.nasser1', 'heba.nasser1@smems.jo', '+962795416585',
'$2a$11$3euPcmQFCiblsZeEu5s7p.9KQ2eX6pYQjM0f4b5v6d7n8o9p0q1r2', 
'Hospital System Administrator', DATEADD(day, -30, GETUTCDATE()));



INSERT INTO Engineers (FullName, Username, Email, Phone, PasswordHash, Position, LastLoginAt) VALUES
('Sarah Mustafa',  'sarah.mustafa6',   'sarah.mustafa6@smems.jo',   '+962787973495', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-26,GETUTCDATE())),
('Khaled Nasser',  'khaled.nasser7',   'khaled.nasser7@smems.jo',   '+962779639838', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-21,GETUTCDATE())),
('Fadi AbuZaid',   'fadi.abuzaid8',    'fadi.abuzaid8@smems.jo',    '+962772551103', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Maintenance Engineer',  DATEADD(day,-5, GETUTCDATE())),
('Dana AbuZaid',   'dana.abuzaid9',    'dana.abuzaid9@smems.jo',    '+962781862764', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-18,GETUTCDATE())),
('Khaled Khalil',  'khaled.khalil10',  'khaled.khalil10@smems.jo',  '+962795036849', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Maintenance Engineer',  DATEADD(day,-29,GETUTCDATE())),
('Omar Saleh',     'omar.saleh11',     'omar.saleh11@smems.jo',     '+962794733382', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-23,GETUTCDATE())),
('Omar Qudah',     'omar.qudah12',     'omar.qudah12@smems.jo',     '+962776938339', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-26,GETUTCDATE())),
('Omar Shraim',    'omar.shraim13',    'omar.shraim13@smems.jo',    '+962781946296', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-3, GETUTCDATE())),
('Mohammad Haddad','mohammad.haddad14','mohammad.haddad14@smems.jo','+962792676491', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Maintenance Engineer',  DATEADD(day,-19,GETUTCDATE())),
('Ahmad Mustafa',  'ahmad.mustafa15',  'ahmad.mustafa15@smems.jo',  '+962771418115', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Clinical Engineer',     DATEADD(day,-19,GETUTCDATE())),
('Sarah Saleh',    'sarah.saleh16',    'sarah.saleh16@smems.jo',    '+962799561026', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Maintenance Engineer',  DATEADD(day,-8, GETUTCDATE())),
('Dana Khalil',    'dana.khalil17',    'dana.khalil17@smems.jo',    '+962795644388', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Maintenance Engineer',  DATEADD(day,-12,GETUTCDATE())),
('Omar Shraim',    'omar.shraim18',    'omar.shraim18@smems.jo',    '+962774303220', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-25,GETUTCDATE())),
('Lina Saleh',     'lina.saleh19',     'lina.saleh19@smems.jo',     '+962788145161', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-11,GETUTCDATE())),
('Lina Khalil',    'lina.khalil20',    'lina.khalil20@smems.jo',    '+962785340953', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-16,GETUTCDATE())),
('Dana AbuZaid',   'dana.abuzaid21',   'dana.abuzaid21@smems.jo',   '+962793126068', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-21,GETUTCDATE())),
('Omar Hamdan',    'omar.hamdan22',    'omar.hamdan22@smems.jo',    '+962789495400', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-11,GETUTCDATE())),
('Noor Qudah',     'noor.qudah23',     'noor.qudah23@smems.jo',     '+962776279793', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Maintenance Engineer',  DATEADD(day,-7, GETUTCDATE())),
('Khaled Saleh',   'khaled.saleh24',   'khaled.saleh24@smems.jo',   '+962790571686', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Clinical Engineer',     DATEADD(day,-5, GETUTCDATE())),
('Dana Hamdan',    'dana.hamdan25',    'dana.hamdan25@smems.jo',    '+962777064297', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Clinical Engineer',     DATEADD(day,-24,GETUTCDATE())),
('Sarah AbuZaid',  'sarah.abuzaid26',  'sarah.abuzaid26@smems.jo',  '+962798666426', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Clinical Engineer',     DATEADD(day,-14,GETUTCDATE())),
('Yousef Shraim',  'yousef.shraim27',  'yousef.shraim27@smems.jo',  '+962774400300', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Maintenance Engineer',  DATEADD(day,-7, GETUTCDATE())),
('Maya Haddad',    'maya.haddad28',    'maya.haddad28@smems.jo',    '+962794186553', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Clinical Engineer',     DATEADD(day,-7, GETUTCDATE())),
('Rana Hamdan',    'rana.hamdan29',    'rana.hamdan29@smems.jo',    '+962798732953', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Clinical Engineer',     DATEADD(day,-4, GETUTCDATE())),
('Heba Khalil',    'heba.khalil30',    'heba.khalil30@smems.jo',    '+962785870810', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Biomedical Engineer',   DATEADD(day,-13,GETUTCDATE())),
('Dana Khalil',    'dana.khalil31',    'dana.khalil31@smems.jo',    '+962799973181', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Maintenance Engineer',  DATEADD(day,-17,GETUTCDATE())),
('Fadi AbuZaid',   'fadi.abuzaid32',   'fadi.abuzaid32@smems.jo',   '+962796351400', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Clinical Engineer',     DATEADD(day,-8, GETUTCDATE())),
('Rana Shraim',    'rana.shraim33',    'rana.shraim33@smems.jo',    '+962771600858', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Clinical Engineer',     DATEADD(day,-22,GETUTCDATE())),
('Yousef Almasri', 'yousef.almasri34', 'yousef.almasri34@smems.jo', '+962777229883', '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Clinical Engineer',     DATEADD(day,-26,GETUTCDATE())),
('Samer Haddad',   'samer.haddad35',   'samer.haddad35@smems.jo',   '+96285094622',  '$2a$11$8kP7M2x6M6L0Y0M8uTt1ye7XbHfTzV1d1sX9m0Y2q8jR4uE1zT8lK', 'Maintenance Engineer',  DATEADD(day,-8, GETUTCDATE()));



INSERT INTO Staff (FullName, Username, Email, Phone, PasswordHash, Position, DepartmentId, LastLoginAt) VALUES
('Sarah AbuZaid',  'sarah.abuzaid36',  'sarah.abuzaid36@smems.jo',  '+962788802227', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'ICU Nurse',            6,  DATEADD(day,  0,GETUTCDATE())),
('Lina Haddad',    'lina.haddad37',    'lina.haddad37@smems.jo',    '+962781780641', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 10, DATEADD(day,-18,GETUTCDATE())),
('Samer Mustafa',  'samer.mustafa38',  'samer.mustafa38@smems.jo',  '+962775856551', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'ICU Nurse',            6,  DATEADD(day,-10,GETUTCDATE())),
('Samer Mustafa',  'samer.mustafa39',  'samer.mustafa39@smems.jo',  '+962786357575', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 14, DATEADD(day, -2,GETUTCDATE())),
('Lina Shraim',    'lina.shraim40',    'lina.shraim40@smems.jo',    '+962779614135', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 11, DATEADD(day,-30,GETUTCDATE())),
('Yousef Khalil',  'yousef.khalil41',  'yousef.khalil41@smems.jo',  '+962791494319', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'ICU Nurse',            7,  DATEADD(day,-20,GETUTCDATE())),
('Noor Shraim',    'noor.shraim42',    'noor.shraim42@smems.jo',    '+962783581918', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Registered Nurse',     2,  DATEADD(day, -5,GETUTCDATE())),
('Sarah Nasser',   'sarah.nasser43',   'sarah.nasser43@smems.jo',   '+962780518712', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'ICU Nurse',            11, DATEADD(day, -9,GETUTCDATE())),
('Fadi Qudah',     'fadi.qudah44',     'fadi.qudah44@smems.jo',     '+962790313348', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 5,  DATEADD(day,-28,GETUTCDATE())),
('Dana AbuZaid',   'dana.abuzaid45',   'dana.abuzaid45@smems.jo',   '+962795095208', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 7,  DATEADD(day, -5,GETUTCDATE())),
('Yousef Almasri', 'yousef.almasri46', 'yousef.almasri46@smems.jo', '+962774195643', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 11, DATEADD(day, -8,GETUTCDATE())),
('Fadi Haddad',    'fadi.haddad47',    'fadi.haddad47@smems.jo',    '+962777065606', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 7,  DATEADD(day, -3,GETUTCDATE())),
('Mohammad Haddad','mohammad.haddad48','mohammad.haddad48@smems.jo','+962799483976', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 9,  DATEADD(day, -1,GETUTCDATE())),
('Ahmad Hamdan',   'ahmad.hamdan49',   'ahmad.hamdan49@smems.jo',   '+962787581944', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Registered Nurse',     2,  DATEADD(day, -8,GETUTCDATE())),
('Noor Khalil',    'noor.khalil50',    'noor.khalil50@smems.jo',    '+962772829016', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 5,  DATEADD(day, -7,GETUTCDATE())),
('Omar Qudah',     'omar.qudah51',     'omar.qudah51@smems.jo',     '+962784869996', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 3,  DATEADD(day,-16,GETUTCDATE())),
('Maya Khalil',    'maya.khalil52',    'maya.khalil52@smems.jo',    '+962770958693', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Registered Nurse',     11, DATEADD(day,-11,GETUTCDATE())),
('Maya AbuZaid',   'maya.abuzaid53',   'maya.abuzaid53@smems.jo',   '+962798999120', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 8,  DATEADD(day,-27,GETUTCDATE())),
('Rana Qudah',     'rana.qudah54',     'rana.qudah54@smems.jo',     '+962788406125', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 3,  DATEADD(day,-30,GETUTCDATE())),
('Fadi Mustafa',   'fadi.mustafa55',   'fadi.mustafa55@smems.jo',   '+962775940363', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 11, DATEADD(day,-23,GETUTCDATE())),
('Samer Shraim',   'samer.shraim56',   'samer.shraim56@smems.jo',   '+962784595418', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'ICU Nurse',            12, DATEADD(day,-11,GETUTCDATE())),
('Dana Mustafa',   'dana.mustafa57',   'dana.mustafa57@smems.jo',   '+962777621165', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Registered Nurse',     8,  DATEADD(day, -9,GETUTCDATE())),
('Sarah Hamdan',   'sarah.hamdan58',   'sarah.hamdan58@smems.jo',   '+962778717677', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 5,  DATEADD(day,  0,GETUTCDATE())),
('Ahmad Mustafa',  'ahmad.mustafa59',  'ahmad.mustafa59@smems.jo',  '+962797359605', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 6,  DATEADD(day, -9,GETUTCDATE())),
('Dana Mustafa',   'dana.mustafa60',   'dana.mustafa60@smems.jo',   '+962770279075', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 13, DATEADD(day,-15,GETUTCDATE())),
('Khaled Almasri', 'khaled.almasri61', 'khaled.almasri61@smems.jo', '+962795698306', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 13, DATEADD(day, -9,GETUTCDATE())),
('Noor Nasser',    'noor.nasser62',    'noor.nasser62@smems.jo',    '+962796735077', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Registered Nurse',     9,  DATEADD(day,-11,GETUTCDATE())),
('Omar Hamdan',    'omar.hamdan63',    'omar.hamdan63@smems.jo',    '+962799600447', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 7,  DATEADD(day, -7,GETUTCDATE())),
('Yousef Hamdan',  'yousef.hamdan64',  'yousef.hamdan64@smems.jo',  '+962797797392', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 4,  DATEADD(day, -8,GETUTCDATE())),
('Mohammad Hamdan','mohammad.hamdan65','mohammad.hamdan65@smems.jo', '+962790982358', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 9,  DATEADD(day,-24,GETUTCDATE())),
('Heba Shraim',    'heba.shraim66',    'heba.shraim66@smems.jo',    '+962797097718', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'ICU Nurse',            5,  DATEADD(day,-20,GETUTCDATE())),
('Fadi Mustafa',   'fadi.mustafa67',   'fadi.mustafa67@smems.jo',   '+962797139114', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'ICU Nurse',            7,  DATEADD(day, -6,GETUTCDATE())),
('Maya Haddad',    'maya.haddad68',    'maya.haddad68@smems.jo',    '+962776120569', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 5,  DATEADD(day, -3,GETUTCDATE())),
('Samer Saleh',    'samer.saleh69',    'samer.saleh69@smems.jo',    '+962790727559', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 5,  DATEADD(day,-22,GETUTCDATE())),
('Noor Saleh',     'noor.saleh70',     'noor.saleh70@smems.jo',     '+962776301168', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'ICU Nurse',            1,  DATEADD(day, -4,GETUTCDATE())),
('Ahmad Mustafa',  'ahmad.mustafa71',  'ahmad.mustafa71@smems.jo',  '+962770742589', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 2,  DATEADD(day, -9,GETUTCDATE())),
('Khaled Khalil',  'khaled.khalil72',  'khaled.khalil72@smems.jo',  '+962793997993', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 12, DATEADD(day,  0,GETUTCDATE())),
('Omar Haddad',    'omar.haddad73',    'omar.haddad73@smems.jo',    '+962775056105', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Registered Nurse',     8,  DATEADD(day, -6,GETUTCDATE())),
('Sarah Hamdan',   'sarah.hamdan74',   'sarah.hamdan74@smems.jo',   '+962780454778', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 6,  DATEADD(day,-14,GETUTCDATE())),
('Fadi Almasri',   'fadi.almasri75',   'fadi.almasri75@smems.jo',   '+962781081305', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Registered Nurse',     14, DATEADD(day,-24,GETUTCDATE())),
('Yousef Almasri', 'yousef.almasri76', 'yousef.almasri76@smems.jo', '+962778942376', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Radiology Technician', 2,  DATEADD(day,-17,GETUTCDATE())),
('Dana Khalil',    'dana.khalil77',    'dana.khalil77@smems.jo',    '+962776448903', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'ICU Nurse',            14, DATEADD(day,-12,GETUTCDATE())),
('Maya Almasri',   'maya.almasri78',   'maya.almasri78@smems.jo',   '+962774092350', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 7,  DATEADD(day,-24,GETUTCDATE())),
('Dana Nasser',    'dana.nasser79',    'dana.nasser79@smems.jo',    '+962794750386', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Registered Nurse',     4,  DATEADD(day,-14,GETUTCDATE())),
('Fadi Haddad',    'fadi.haddad80',    'fadi.haddad80@smems.jo',    '+962787486527', '$2a$11$w3JxV7nC9eR2L0aXbM8pQu3fV9sYkR5tD1uB6mN7pQ2zL4xT5vK7e', 'Consultant Physician', 8,  DATEADD(day,-12,GETUTCDATE()));



INSERT INTO Manufacturers (Name, ContactPerson, Email, Phone, Address, Website, Notes) VALUES
('GE Healthcare',        'Michelle Velez',       'contact@gehealthcare.com',       '+1-897-6236326', '69531 Hawkins Pine New Emily, AK 16177',                          'https://www.gehealthcare.com',       'Authorized medical equipment manufacturer'),
('Siemens Healthineers','Tracey Brown',           'contact@siemenshealthineers.com', '+1-530-3661058', 'PSC 4203, Box 2529 APO AP 00501',                                 'https://www.siemenshealthineers.com', 'Authorized medical equipment manufacturer'),
('Philips Medical',      'Matthew Brown',         'contact@philipsmedical.com',       '+1-844-1707762', '27897 Steven Summit Apt. 718 New Robertchester, IA 76873',          'https://www.philipsmedical.com',     'Authorized medical equipment manufacturer'),
('Drager',               'Logan Brown',           'contact@drager.com',               '+1-962-9739692', '54489 Suzanne Mews Kennethton, OR 99004',                          'https://www.drager.com',              'Authorized medical equipment manufacturer'),
('Mindray',              'Christina Flores',      'contact@mindray.com',              '+1-325-5249882', 'USS Williams FPO AA 23208',                                        'https://www.mindray.com',             'Authorized medical equipment manufacturer'),
('Medtronic',            'James Knapp',           'contact@medtronic.com',            '+1-692-6013251', '32654 Williams Green Port Eric, MA 67232',                         'https://www.medtronic.com',           'Authorized medical equipment manufacturer'),
('Fujifilm Healthcare',  'Dr. Robert Butler',     'contact@fujifilmhealthcare.com',   '+1-403-7046955', '61149 Young Meadows Suite 803 New Benjamin, WA 16591',              'https://www.fujifilmhealthcare.com', 'Authorized medical equipment manufacturer'),
('Canon Medical',        'Melinda Farrell',       'contact@canonmedical.com',        '+1-253-4653880', '38149 Stephen Mountain Apt. 889 Joshuamouth, DE 64559',             'https://www.canonmedical.com',        'Authorized medical equipment manufacturer'),
('Baxter',               'Ruth Green',            'contact@baxter.com',               '+1-315-4958849', '34588 Michael Port Christopherberg, WY 75582',                      'https://www.baxter.com',              'Authorized medical equipment manufacturer'),
('Welch Allyn',          'Justin Velez',          'contact@welchallyn.com',           '+1-848-8284023', '2075 Amanda Run West Samantha, NM 81894',                          'https://www.welchallyn.com',          'Authorized medical equipment manufacturer'),
('Nihon Kohden',         'Jacob Turner',          'contact@nihonkohden.com',          '+1-532-1252794', '88387 Sarah Hill Suite 823 Krystalhaven, NM 52165',                 'https://www.nihonkohden.com',         'Authorized medical equipment manufacturer'),
('B. Braun',             'Samantha Spencer',      'contact@bbraun.com',               '+1-371-5632190', '51403 Rios Centers Port Devin, AK 41436',                          'https://www.bbraun.com',              'Authorized medical equipment manufacturer'),
('Abbott Diagnostics',   'Victor Roberts',        'contact@abbottdiagnostics.com',   '+1-798-3007387', '822 Sharp Ville Apt. 781 North David, WY 24002',                   'https://www.abbottdiagnostics.com',  'Authorized medical equipment manufacturer'),
('Olympus Medical',      'William Alexander',     'contact@olympusmedical.com',       '+1-605-5078362', '0275 Reed Row Apt. 377 Clarktown, TN 66812',                       'https://www.olympusmedical.com',      'Authorized medical equipment manufacturer'),
('Stryker',              'Krystal Garrett DVM',   'contact@stryker.com',              '+1-209-1704531', '3907 Guerrero Villages Lake Natalie, IN 70387',                     'https://www.stryker.com',             'Authorized medical equipment manufacturer'),
('Zimmer Biomet',        'Robin Bentley',         'contact@zimmerbiomet.com',         '+1-795-8973718', '65346 Nicholas Street East Josemouth, DC 64699',                   'https://www.zimmerbiomet.com',        'Authorized medical equipment manufacturer'),
('Hitachi Medical',      'Chad Harvey',           'contact@hitachimedical.com',       '+1-904-3600333', '582 Aaron Vista Richardshire, NV 19586',                            'https://www.hitachimedical.com',      'Authorized medical equipment manufacturer'),
('Carestream',           'Sean Carr',             'contact@carestream.com',           '+1-580-2501223', 'USNV Smith FPO AA 66307',                                          'https://www.carestream.com',          'Authorized medical equipment manufacturer'),
('Masimo',               'Kristen Salas',         'contact@masimo.com',               '+1-454-3645741', '951 Edwards Fields Nicoleburgh, IL 29070',                          'https://www.masimo.com',              'Authorized medical equipment manufacturer'),
('Hamilton Medical',     'Deanna Payne',          'contact@hamiltonmedical.com',      '+1-711-2533463', '40472 Logan Heights Michelleberg, DC 99327',                        'https://www.hamiltonmedical.com',     'Authorized medical equipment manufacturer');



INSERT INTO Suppliers (Name, ContactPerson, Email, Phone, Address, Website, Notes) VALUES
('Jordan Medical Supply 1',  'Michael James',       'sales1@jmsupply.jo',   '+96266677780', 'Amman Jordan', 'https://www.jmsupply1.jo',  'Authorized regional healthcare supplier'),
('Jordan Medical Supply 2',  'Billy Wilkinson',     'sales2@jmsupply.jo',   '+96262007511', 'Amman Jordan', 'https://www.jmsupply2.jo',  'Authorized regional healthcare supplier'),
('Jordan Medical Supply 3',  'Shaun Haynes',        'sales3@jmsupply.jo',   '+96262002860', 'Amman Jordan', 'https://www.jmsupply3.jo',  'Authorized regional healthcare supplier'),
('Jordan Medical Supply 4',  'Mr. Daniel Thomas',   'sales4@jmsupply.jo',   '+96266964596', 'Amman Jordan', 'https://www.jmsupply4.jo',  'Authorized regional healthcare supplier'),
('Jordan Medical Supply 5',  'Deanna Ortiz',        'sales5@jmsupply.jo',   '+96263582322', 'Amman Jordan', 'https://www.jmsupply5.jo',  'Authorized regional healthcare supplier'),
('Jordan Medical Supply 6',  'Paul Phillips DVM',   'sales6@jmsupply.jo',   '+96269998515', 'Amman Jordan', 'https://www.jmsupply6.jo',  'Authorized regional healthcare supplier'),
('Jordan Medical Supply 7',  'Shawn Guzman',        'sales7@jmsupply.jo',   '+96262801733', 'Amman Jordan', 'https://www.jmsupply7.jo',  'Authorized regional healthcare supplier'),
('Jordan Medical Supply 8',  'Jennifer Key',        'sales8@jmsupply.jo',   '+96263111034', 'Amman Jordan', 'https://www.jmsupply8.jo',  'Authorized regional healthcare supplier'),
('Jordan Medical Supply 9',  'Kristie Yang',        'sales9@jmsupply.jo',   '+96269184645', 'Amman Jordan', 'https://www.jmsupply9.jo',  'Authorized regional healthcare supplier'),
('Jordan Medical Supply 10', 'Matthew Davis',       'sales10@jmsupply.jo',  '+96264903632', 'Amman Jordan', 'https://www.jmsupply10.jo', 'Authorized regional healthcare supplier'),
('Jordan Medical Supply 11', 'Laura Richardson',    'sales11@jmsupply.jo',  '+96266314007', 'Amman Jordan', 'https://www.jmsupply11.jo', 'Authorized regional healthcare supplier'),
('Jordan Medical Supply 12', 'Kimberly Smith',      'sales12@jmsupply.jo',  '+96266827024', 'Amman Jordan', 'https://www.jmsupply12.jo', 'Authorized regional healthcare supplier'),
('Jordan Medical Supply 13', 'Erin Travis',         'sales13@jmsupply.jo',  '+96268360481', 'Amman Jordan', 'https://www.jmsupply13.jo', 'Authorized regional healthcare supplier'),
('Jordan Medical Supply 14', 'Nathan Jennings',     'sales14@jmsupply.jo',  '+96264237420', 'Amman Jordan', 'https://www.jmsupply14.jo', 'Authorized regional healthcare supplier'),
('Jordan Medical Supply 15', 'Miguel Long',         'sales15@jmsupply.jo',  '+96261358241', 'Amman Jordan', 'https://www.jmsupply15.jo', 'Authorized regional healthcare supplier');

INSERT INTO Devices (
    DeviceCode, Name, ModelNumber, SerialNumber, ManufacturerId, SupplierId, DepartmentId,
    Status, RiskLevel, Location, PurchaseDate, ExpectedLifespan, FailureCount, 
    NextMaintenanceDate, Notes
) VALUES
('DEV-1001', 'Anesthesia Machine', 'MDL-466', 'SN8595871',  5,  5,  6,  'Operational',    'Critical', 'Floor 1 - Room 348', '2022-12-15', '15 Years', 5, '2026-09-07', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1002', 'Incubator',          'MDL-456', 'SN4394032',  15, 5,  8,  'Operational',    'High',     'Floor 5 - Room 568', '2022-09-06', '9 Years',  3, '2026-08-27', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1003', 'Blood Gas Analyzer', 'MDL-564', 'SN1007543',  17, 7,  5,  'Operational',    'High',     'Floor 2 - Room 552', '2022-01-20', '10 Years', 2, '2026-11-03', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1004', 'Defibrillator',      'MDL-636', 'SN8164124',  16, 13, 3,  'Operational',    'Critical', 'Floor 5 - Room 178', '2022-10-10', '12 Years', 0, '2026-06-17', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1005', 'Ventilator',         'MDL-282', 'SN1327925',  7,  2,  2,  'Operational',    'Critical', 'Floor 4 - Room 198', '2022-02-02', '13 Years', 5, '2026-09-17', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1006', 'ECG Monitor',        'MDL-150', 'SN4841506',  8,  10, 4,  'Operational',    'Medium',   'Floor 1 - Room 106', '2022-04-21', '8 Years',  3, '2026-12-21', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1007', 'Incubator',          'MDL-861', 'SN5361587',  12, 13, 8,  'Operational',    'High',     'Floor 1 - Room 521', '2022-10-09', '8 Years',  2, '2026-06-07', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1008', 'CT Scanner',         'MDL-842', 'SN3422398',  18, 7,  1,  'Out of Service', 'High',     'Floor 2 - Room 198', '2022-09-22', '9 Years',  4, '2026-11-18', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1009', 'Dialysis Machine',   'MDL-466', 'SN8072569',  15, 12, 10, 'Operational',    'High',     'Floor 4 - Room 552', '2022-02-06', '13 Years', 4, '2026-08-22', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1010', 'Dialysis Machine',   'MDL-553', 'SN46943810', 11, 9,  10, 'Operational',    'High',     'Floor 3 - Room 532', '2022-06-14', '12 Years', 4, '2026-07-19', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1011', 'Anesthesia Machine', 'MDL-884', 'SN67696711', 9,  6,  6,  'Operational',    'Critical', 'Floor 4 - Room 329', '2022-07-01', '11 Years', 4, '2026-06-17', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1012', 'MRI Scanner',        'MDL-407', 'SN33266512', 7,  14, 1,  'Out of Service', 'High',     'Floor 3 - Room 111', '2022-12-27', '15 Years', 4, '2026-10-19', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1013', 'MRI Scanner',        'MDL-571', 'SN47293213', 5,  3,  1,  'Out of Service', 'High',     'Floor 3 - Room 287', '2022-10-08', '11 Years', 0, '2026-10-13', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1014', 'CT Scanner',         'MDL-840', 'SN44046414', 16, 3,  1,  'Out of Service', 'High',     'Floor 4 - Room 522', '2022-12-23', '8 Years',  4, '2026-11-10', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1015', 'Anesthesia Machine', 'MDL-180', 'SN91801715', 2,  15, 6,  'Operational',    'Critical', 'Floor 3 - Room 278', '2022-07-14', '10 Years', 5, '2026-11-28', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1016', 'Anesthesia Machine', 'MDL-155', 'SN41958816', 17, 1,  6,  'Operational',    'Critical', 'Floor 3 - Room 376', '2022-10-05', '10 Years', 1, '2026-06-22', 'Equipment inspected oasis within manufacturer specifications.'),
('DEV-1017', 'Dialysis Machine',   'MDL-689', 'SN62924717', 3,  4,  10, 'Operational',    'High',     'Floor 4 - Room 248', '2022-07-05', '8 Years',  3, '2026-10-24', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1018', 'Anesthesia Machine', 'MDL-368', 'SN18921118', 13, 9,  6,  'Operational',    'Critical', 'Floor 4 - Room 496', '2022-12-05', '15 Years', 1, '2026-09-13', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1019', 'Incubator',          'MDL-730', 'SN87140319', 17, 4,  8,  'Operational',    'High',     'Floor 2 - Room 136', '2022-05-25', '8 Years',  0, '2026-11-14', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1020', 'Blood Gas Analyzer', 'MDL-295', 'SN43631820', 12, 10, 5,  'Operational',    'High',     'Floor 1 - Room 494', '2022-05-06', '14 Years', 4, '2026-07-15', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1021', 'Defibrillator',      'MDL-801', 'SN49197921', 5,  13, 3,  'Operational',    'Critical', 'Floor 1 - Room 157', '2022-12-02', '9 Years',  2, '2026-06-11', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1022', 'Infusion Pump',      'MDL-582', 'SN12385322', 11, 4,  2,  'Operational',    'Medium',   'Floor 5 - Room 208', '2022-04-18', '11 Years', 4, '2026-09-20', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1023', 'Anesthesia Machine', 'MDL-696', 'SN79575923', 17, 1,  6,  'Operational',    'Critical', 'Floor 3 - Room 187', '2022-01-10', '11 Years', 0, '2026-09-21', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1024', 'Anesthesia Machine', 'MDL-776', 'SN38448924', 12, 14, 6,  'Operational',    'Critical', 'Floor 1 - Room 109', '2022-05-28', '10 Years', 3, '2026-10-23', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1025', 'Infusion Pump',      'MDL-987', 'SN72611125', 18, 2,  2,  'Operational',    'Medium',   'Floor 2 - Room 201', '2022-09-15', '7 Years',  0, '2026-07-17', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1026', 'Infusion Pump',      'MDL-502', 'SN88055026', 6,  1,  2,  'Operational',    'Medium',   'Floor 3 - Room 450', '2022-06-12', '10 Years', 4, '2026-08-26', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1027', 'Anesthesia Machine', 'MDL-934', 'SN13089727', 18, 5,  6,  'Operational',    'Critical', 'Floor 2 - Room 177', '2022-06-13', '8 Years',  5, '2026-09-17', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1028', 'Anesthesia Machine', 'MDL-346', 'SN70072728', 5,  7,  6,  'Operational',    'Critical', 'Floor 3 - Room 565', '2022-10-11', '9 Years',  1, '2026-11-05', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1029', 'Defibrillator',      'MDL-983', 'SN24481829', 10, 12, 3,  'Operational',    'Critical', 'Floor 5 - Room 565', '2022-03-14', '9 Years',  2, '2026-06-18', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1030', 'Ventilator',         'MDL-425', 'SN98030330', 5,  12, 2,  'Operational',    'Critical', 'Floor 1 - Room 491', '2022-06-14', '14 Years', 5, '2026-12-21', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1031', 'Defibrillator',      'MDL-209', 'SN23793531', 6,  1,  3,  'Operational',    'Critical', 'Floor 5 - Room 415', '2022-05-20', '10 Years', 4, '2026-07-08', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1032', 'CT Scanner',         'MDL-804', 'SN91770732', 16, 3,  1,  'Out of Service', 'High',     'Floor 5 - Room 533', '2022-10-16', '12 Years', 1, '2026-12-13', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1033', 'Ventilator',         'MDL-267', 'SN30264733', 8,  1,  2,  'Operational',    'Critical', 'Floor 4 - Room 491', '2022-01-09', '10 Years', 1, '2026-06-06', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1034', 'Infusion Pump',      'MDL-804', 'SN44067734', 15, 15, 2,  'Operational',    'Medium',   'Floor 2 - Room 318', '2022-07-25', '13 Years', 5, '2026-08-13', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1035', 'Dialysis Machine',   'MDL-649', 'SN65074435', 2,  9,  10, 'Operational',    'High',     'Floor 1 - Room 331', '2022-08-27', '15 Years', 4, '2026-09-24', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1036', 'Anesthesia Machine', 'MDL-628', 'SN90232836', 2,  4,  6,  'Operational',    'Critical', 'Floor 3 - Room 325', '2022-07-10', '9 Years',  3, '2026-08-20', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1037', 'Ventilator',         'MDL-574', 'SN97020637', 15, 2,  2,  'Operational',    'Critical', 'Floor 5 - Room 157', '2022-03-25', '15 Years', 1, '2026-09-14', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1038', 'MRI Scanner',        'MDL-283', 'SN57571838', 19, 15, 1,  'Out of Service', 'High',     'Floor 2 - Room 551', '2022-08-03', '9 Years',  5, '2026-11-04', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1039', 'Infusion Pump',      'MDL-749', 'SN48057339', 3,  9,  2,  'Operational',    'Medium',   'Floor 4 - Room 472', '2022-09-20', '15 Years', 3, '2026-06-25', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1040', 'Dialysis Machine',   'MDL-992', 'SN79156740', 5,  3,  10, 'Operational',    'High',     'Floor 2 - Room 191', '2022-12-02', '11 Years', 5, '2026-12-15', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1041', 'CT Scanner',         'MDL-234', 'SN31270541', 1,  10, 1,  'Out of Service', 'High',     'Floor 3 - Room 106', '2022-08-16', '7 Years',  4, '2026-09-08', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1042', 'Infusion Pump',      'MDL-564', 'SN53007442', 15, 2,  2,  'Operational',    'Medium',   'Floor 5 - Room 407', '2022-01-08', '15 Years', 1, '2026-06-06', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1043', 'Blood Gas Analyzer', 'MDL-137', 'SN25243643', 2,  11, 5,  'Operational',    'High',     'Floor 5 - Room 529', '2022-10-10', '7 Years',  0, '2026-09-06', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1044', 'Dialysis Machine',   'MDL-776', 'SN82510444', 20, 10, 10, 'Operational',    'High',     'Floor 5 - Room 190', '2022-08-24', '7 Years',  0, '2026-12-10', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1045', 'Anesthesia Machine', 'MDL-939', 'SN17095345', 14, 14, 6,  'Operational',    'Critical', 'Floor 5 - Room 133', '2022-05-16', '11 Years', 2, '2026-10-14', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1046', 'Defibrillator',      'MDL-726', 'SN16101046', 4,  10, 3,  'Operational',    'Critical', 'Floor 5 - Room 144', '2022-08-24', '11 Years', 3, '2026-10-28', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1047', 'Incubator',          'MDL-688', 'SN75351847', 12, 15, 8,  'Operational',    'High',     'Floor 2 - Room 414', '2022-10-16', '14 Years', 3, '2026-12-06', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1048', 'ECG Monitor',        'MDL-298', 'SN10165748', 4,  1,  4,  'Operational',    'Medium',   'Floor 4 - Room 420', '2022-02-24', '13 Years', 3, '2026-06-11', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1049', 'Defibrillator',      'MDL-111', 'SN77173649', 7,  12, 3,  'Operational',    'Critical', 'Floor 3 - Room 230', '2022-02-10', '15 Years', 2, '2026-12-01', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1050', 'Blood Gas Analyzer', 'MDL-505', 'SN85958450', 19, 10, 5,  'Operational',    'High',     'Floor 1 - Room 526', '2022-03-07', '12 Years', 5, '2026-09-16', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1051', 'Infusion Pump',      'MDL-259', 'SN75766751', 15, 15, 2,  'Operational',    'Medium',   'Floor 5 - Room 558', '2022-06-15', '7 Years',  4, '2026-08-19', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1052', 'Incubator',          'MDL-119', 'SN31837952', 19, 11, 8,  'Operational',    'High',     'Floor 3 - Room 509', '2022-06-03', '7 Years',  2, '2026-09-21', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1053', 'Dialysis Machine',   'MDL-720', 'SN50848953', 6,  10, 10, 'Operational',    'High',     'Floor 1 - Room 542', '2022-05-23', '7 Years',  2, '2026-09-03', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1054', 'Blood Gas Analyzer', 'MDL-562', 'SN96974954', 10, 4,  5,  'Operational',    'High',     'Floor 5 - Room 254', '2022-03-04', '7 Years',  1, '2026-06-15', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1055', 'Infusion Pump',      'MDL-612', 'SN26156255', 7,  6,  2,  'Operational',    'Medium',   'Floor 4 - Room 475', '2022-09-10', '8 Years',  4, '2026-11-05', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1056', 'Infusion Pump',      'MDL-496', 'SN84834456', 17, 2,  2,  'Operational',    'Medium',   'Floor 1 - Room 259', '2022-01-22', '8 Years',  2, '2026-09-02', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1057', 'ECG Monitor',        'MDL-123', 'SN70561557', 10, 11, 4,  'Operational',    'Medium',   'Floor 2 - Room 264', '2022-04-07', '15 Years', 2, '2026-12-15', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1058', 'Ventilator',         'MDL-261', 'SN38692958', 4,  1,  2,  'Operational',    'Critical', 'Floor 5 - Room 386', '2022-09-04', '12 Years', 4, '2026-08-09', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1059', 'Dialysis Machine',   'MDL-350', 'SN27959459', 3,  5,  10, 'Operational',    'High',     'Floor 5 - Room 556', '2022-06-18', '9 Years',  0, '2026-09-22', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1060', 'Anesthesia Machine', 'MDL-155', 'SN19030260', 14, 7,  6,  'Operational',    'Critical', 'Floor 2 - Room 460', '2022-10-19', '14 Years', 4, '2026-08-10', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1061', 'Incubator',          'MDL-180', 'SN89245361', 15, 10, 8,  'Operational',    'High',     'Floor 2 - Room 147', '2022-12-15', '11 Years', 5, '2026-11-23', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1062', 'Defibrillator',      'MDL-329', 'SN25070362', 20, 5,  3,  'Operational',    'Critical', 'Floor 3 - Room 300', '2022-03-23', '12 Years', 3, '2026-09-01', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1063', 'CT Scanner',         'MDL-459', 'SN60190363', 15, 14, 1,  'Out of Service', 'High',     'Floor 2 - Room 244', '2022-06-20', '15 Years', 0, '2026-09-27', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1064', 'Incubator',          'MDL-765', 'SN17436964', 10, 9,  8,  'Operational',    'High',     'Floor 4 - Room 305', '2022-10-14', '10 Years', 4, '2026-07-16', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1065', 'Blood Gas Analyzer', 'MDL-313', 'SN95345265', 19, 7,  5,  'Operational',    'High',     'Floor 4 - Room 390', '2022-06-18', '9 Years',  5, '2026-11-03', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1066', 'CT Scanner',         'MDL-706', 'SN72503066', 15, 6,  1,  'Out of Service', 'High',     'Floor 5 - Room 149', '2022-08-06', '8 Years',  4, '2026-06-22', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1067', 'Blood Gas Analyzer', 'MDL-606', 'SN77632067', 2,  2,  5,  'Operational',    'High',     'Floor 2 - Room 518', '2022-03-19', '7 Years',  1, '2026-11-05', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1068', 'ECG Monitor',        'MDL-499', 'SN74547468', 19, 8,  4,  'Operational',    'Medium',   'Floor 2 - Room 205', '2022-11-11', '11 Years', 3, '2026-11-12', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1069', 'MRI Scanner',        'MDL-726', 'SN63521869', 13, 14, 1,  'Out of Service', 'High',     'Floor 2 - Room 536', '2022-05-02', '7 Years',  0, '2026-11-18', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1070', 'ECG Monitor',        'MDL-986', 'SN13269970', 8,  7,  4,  'Operational',    'Medium',   'Floor 5 - Room 301', '2022-11-11', '13 Years', 3, '2026-09-18', 'Equipment inspected and operating within manufacturer specifications.'),
('DEV-1112', 'Anesthesia Machine', 'MDL-643', 'SN998812112',4,  2,  6,  'Operational',    'Critical', 'Floor 1 - Room 204', '2022-04-12', '10 Years', 1, '2026-08-14', 'Equipment inspected and operating within manufacturer specifications.');

INSERT INTO Accessories (Name, Description, QuantityInStock) VALUES
('ECG Lead Set',                'Replacement ECG monitoring leads',      50),
('Ultrasound Probe',           'High-frequency ultrasound imaging probe',9),
('Defibrillator Pads',         'Adult disposable defibrillator pads',   33),
('Ventilator Circuit',         'Disposable ventilator breathing circuit',15),
('SpO2 Sensor',                'Reusable oxygen saturation sensor',     114),
('Infusion Pump Battery',      'Rechargeable infusion pump battery',    73),
('MRI Coil',                   'MRI head imaging coil',                 104),
('CT Contrast Injector',       'Automated contrast media injector',      92),
('Anesthesia Breathing Circuit','Reusable anesthesia circuit',          35),
('Patient Monitor Cable',      'Multi-parameter monitor cable',           7),
('Fetal Monitor Belt',         'Elastic fetal monitoring belt',          17),
('Syringe Pump Clamp',         'Adjustable syringe fixation clamp',      69),
('Dialysis Tubing Set',        'Dialysis blood tubing',                  58),
('XRay Detector Plate',        'Digital X-Ray imaging detector',         11),
('Blood Pressure Cuff',        'Adult reusable NIBP cuff',               70);

INSERT INTO DeviceAccessories (DeviceId, AccessoryId, Quantity, Notes) VALUES
(1,  15, 3, 'Accessory installed and verified during equipment inspection.'),
(2,  14, 5, 'Accessory installed and verified during equipment inspection.'),
(3,  9,  6, 'Accessory installed and verified during equipment inspection.'),
(4,  3,  1, 'Accessory installed and verified during equipment inspection.'),
(5,  10, 2, 'Accessory installed and verified during equipment inspection.'),
(6,  3,  2, 'Accessory installed and verified during equipment inspection.'),
(7,  6,  1, 'Accessory installed and verified during equipment inspection.'),
(8,  1,  4, 'Accessory installed and verified during equipment inspection.'),
(9,  13, 2, 'Accessory installed and verified during equipment inspection.'),
(10, 4,  5, 'Accessory installed and verified during equipment inspection.'),
(11, 11, 1, 'Accessory installed and verified during equipment inspection.'),
(12, 8,  3, 'Accessory installed and verified during equipment inspection.'),
(13, 15, 4, 'Accessory installed and verified during equipment inspection.'),
(14, 5,  4, 'Accessory installed and verified during equipment inspection.'),
(15, 5,  3, 'Accessory installed and verified during equipment inspection.'),
(16, 10, 1, 'Accessory installed and verified during equipment inspection.'),
(17, 9,  6, 'Accessory installed and verified during equipment inspection.'),
(18, 3,  2, 'Accessory installed and verified during equipment inspection.'),
(19, 10, 5, 'Accessory installed and verified during equipment inspection.'),
(20, 2,  3, 'Accessory installed and verified during equipment inspection.'),
(21, 1,  5, 'Accessory installed and verified during equipment inspection.'),
(22, 2,  2, 'Accessory installed and verified during equipment inspection.'),
(23, 11, 3, 'Accessory installed and verified during equipment inspection.'),
(24, 11, 3, 'Accessory installed and verified during equipment inspection.'),
(25, 5,  5, 'Accessory installed and verified during equipment inspection.'),
(26, 5,  5, 'Accessory installed and verified during equipment inspection.'),
(27, 9,  2, 'Accessory installed and verified during equipment inspection.'),
(28, 15, 3, 'Accessory installed and verified during equipment inspection.'),
(29, 11, 5, 'Accessory installed and verified during equipment inspection.'),
(30, 13, 6, 'Accessory installed and verified during equipment inspection.'),
(31, 15, 3, 'Accessory installed and verified during equipment inspection.'),
(32, 6,  2, 'Accessory installed and verified during equipment inspection.'),
(33, 9,  6, 'Accessory installed and verified during equipment inspection.'),
(34, 8,  6, 'Accessory installed and verified during equipment inspection.'),
(35, 5,  2, 'Accessory installed and verified during equipment inspection.'),
(36, 6,  4, 'Accessory installed and verified during equipment inspection.'),
(37, 15, 5, 'Accessory installed and verified during equipment inspection.'),
(38, 7,  5, 'Accessory installed and verified during equipment inspection.'),
(39, 15, 3, 'Accessory installed and verified during equipment inspection.'),
(40, 11, 1, 'Accessory installed and verified during equipment inspection.'),
(41, 13, 6, 'Accessory installed and verified during equipment inspection.'),
(42, 5,  3, 'Accessory installed and verified during equipment inspection.'),
(43, 9,  1, 'Accessory installed and verified during equipment inspection.'),
(44, 13, 6, 'Accessory installed and verified during equipment inspection.'),
(45, 5,  1, 'Accessory installed and verified during equipment inspection.'),
(46, 12, 5, 'Accessory installed and verified during equipment inspection.'),
(47, 10, 5, 'Accessory installed and verified during equipment inspection.'),
(48, 9,  2, 'Accessory installed and verified during equipment inspection.'),
(49, 3,  3, 'Accessory installed and verified during equipment inspection.'),
(50, 9,  5, 'Accessory installed and verified during equipment inspection.'),
(51, 10, 5, 'Accessory installed and verified during equipment inspection.'),
(52, 5,  1, 'Accessory installed and verified during equipment inspection.'),
(53, 14, 1, 'Accessory installed and verified during equipment inspection.'),
(54, 8,  5, 'Accessory installed and verified during equipment inspection.'),
(55, 3,  6, 'Accessory installed and verified during equipment inspection.'),
(56, 2,  6, 'Accessory installed and verified during equipment inspection.'),
(57, 2,  2, 'Accessory installed and verified during equipment inspection.'),
(58, 8,  4, 'Accessory installed and verified during equipment inspection.'),
(59, 1,  2, 'Accessory installed and verified during equipment inspection.'),
(60, 5,  2, 'Accessory installed and verified during equipment inspection.'),
(61, 15, 2, 'Accessory installed and verified during equipment inspection.'),
(62, 1,  2, 'Accessory installed and verified during equipment inspection.'),
(63, 8,  3, 'Accessory installed and verified during equipment inspection.'),
(64, 15, 6, 'Accessory installed and verified during equipment inspection.'),
(65, 15, 4, 'Accessory installed and verified during equipment inspection.'),
(66, 3,  1, 'Accessory installed and verified during equipment inspection.'),
(67, 10, 5, 'Accessory installed and verified during equipment inspection.'),
(68, 7,  6, 'Accessory installed and verified during equipment inspection.'),
(69, 4,  2, 'Accessory installed and verified during equipment inspection.'),
(70, 14, 3, 'Accessory installed and verified during equipment inspection.'),
(71, 9,  5, 'Accessory installed and verified during equipment inspection.');

INSERT INTO MaintenanceTypes (Name) VALUES
('Preventive Maintenance'),
('Corrective Maintenance');

INSERT INTO MaintenanceRequests (
    RequestCode, DeviceId, ReportedByStaffId, AssignedEngineerId, MaintenanceTypeId,
    Status, RiskLevel, IssueTitle, IssueDescription, EngineerNotes, 
    CompletionNotes, HasAlternative, RequestDate, StartedAt, CompletedAt
) VALUES
('MR-5001', 71, 38, 21, 2, 'Pending',     'High',    'Touchscreen unresponsive',            'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-03-16', '2026-01-01', NULL),
('MR-5002', 39, 41, 5,  2, 'Completed',   'Medium',  'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-02-16', '2026-05-27', '2026-05-15'),
('MR-5003', 19, 27, 10, 2, 'In Progress', 'Medium',  'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-05-10', '2026-03-25', NULL),
('MR-5004', 21, 20, 4,  1, 'Pending',     'High',    'Sensor malfunction',                  'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-04-09', '2026-02-27', NULL),
('MR-5005', 45, 25, 12, 2, 'In Progress', 'High',    'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-05-06', '2026-02-07', NULL),
('MR-5006', 60, 14, 15, 2, 'Completed',   'Medium',  'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-05-17', '2026-03-04', '2026-01-13'),
('MR-5007', 55, 41, 22, 1, 'In Progress', 'Critical', 'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-03-23', '2026-03-16', NULL),
('MR-5008', 44, 40, 4,  2, 'Pending',     'Medium',  'Sensor malfunction',                  'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-05-27', '2026-04-02', NULL),
('MR-5009', 42, 8,  20, 2, 'Pending',     'Critical', 'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-03-07', '2026-04-12', NULL),
('MR-5010', 65, 10, 17, 2, 'Pending',     'Medium',  'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-03-20', '2026-01-18', NULL),
('MR-5011', 12, 32, 12, 2, 'Completed',   'High',    'Sensor malfunction',                  'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-03-12', '2026-05-02', '2026-03-13'),
('MR-5012', 28, 33, 24, 2, 'In Progress', 'High',    'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-02-26', '2026-03-15', NULL),
('MR-5013', 19, 18, 10, 2, 'Completed',   'Critical', 'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-01-19', '2026-04-24', '2026-04-05'),
('MR-5014', 32, 22, 14, 2, 'In Progress', 'Critical', 'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-05-25', '2026-01-01', NULL),
('MR-5015', 38, 40, 16, 2, 'Pending',     'High',    'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-04-18', '2026-02-06', NULL),
('MR-5016', 20, 21, 10, 2, 'Completed',   'Medium',  'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-04-06', '2026-04-08', '2026-01-26'),
('MR-5017', 68, 22, 16, 2, 'In Progress', 'Critical', 'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-05-16', '2026-02-01', NULL),
('MR-5018', 49, 12, 12, 2, 'Completed',   'Critical', 'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-03-06', '2026-04-28', '2026-03-21'),
('MR-5019', 46, 15, 2,  2, 'Completed',   'High',    'Sensor malfunction',                  'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-03-02', '2026-01-05', '2026-03-15'),
('MR-5020', 66, 17, 25, 2, 'In Progress', 'Medium',  'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-05-11', '2026-03-17', NULL),
('MR-5021', 13, 39, 19, 2, 'In Progress', 'Medium',  'Power supply instability',              'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-02-17', '2026-03-09', NULL),
('MR-5022', 53, 26, 30, 2, 'In Progress', 'Medium',  'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-02-17', '2026-03-25', NULL),
('MR-5023', 54, 34, 30, 2, 'Pending',     'Medium',  'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-01-16', '2026-02-26', NULL),
('MR-5024', 18, 33, 29, 2, 'In Progress', 'Critical', 'Power supply instability',              'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-02-23', '2026-03-23', NULL),
('MR-5025', 29, 13, 28, 2, 'In Progress', 'Critical', 'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-03-05', '2026-01-24', NULL),
('MR-5026', 39, 12, 3,  2, 'Pending',     'High',    'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-01-07', '2026-02-03', NULL),
('MR-5027', 61, 12, 30, 2, 'Pending',     'Medium',  'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-03-22', '2026-01-26', NULL),
('MR-5028', 48, 43, 7,  2, 'Pending',     'Medium',  'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-03-01', '2026-01-26', NULL),
('MR-5029', 70, 27, 12, 1, 'Pending',     'Critical', 'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-05-19', '2026-02-28', NULL),
('MR-5030', 68, 18, 23, 2, 'Pending',     'High',    'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-05-01', '2026-01-27', NULL),
('MR-5031', 47, 42, 6,  2, 'Completed',   'Critical', 'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-02-27', '2026-01-07', '2026-05-28'),
('MR-5032', 66, 19, 14, 2, 'In Progress', 'High',    'Touchscreen unresponsive',            'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-04-04', '2026-04-14', NULL),
('MR-5033', 12, 7,  3,  1, 'Completed',   'High',    'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-04-10', '2026-02-16', '2026-03-23'),
('MR-5034', 28, 33, 8,  1, 'In Progress', 'High',    'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-04-07', '2026-05-24', NULL),
('MR-5035', 23, 5,  22, 2, 'Pending',     'Critical', 'Power supply instability',              'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-04-15', '2026-04-22', NULL),
('MR-5036', 59, 42, 4,  2, 'Completed',   'Medium',  'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-02-11', '2026-01-04', '2026-02-13'),
('MR-5037', 16, 19, 27, 2, 'Pending',     'Critical', 'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-03-25', '2026-05-28', NULL),
('MR-5038', 46, 35, 22, 2, 'Completed',   'High',    'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-05-09', '2026-05-19', '2026-03-25'),
('MR-5039', 33, 36, 16, 2, 'Pending',     'Critical', 'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-01-04', '2026-04-04', NULL),
('MR-5040', 23, 29, 20, 2, 'Completed',   'Medium',  'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-05-20', '2026-01-27', '2026-01-16'),
('MR-5041', 52, 34, 7,  2, 'Pending',     'Critical', 'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-04-17', '2026-01-22', NULL),
('MR-5042', 38, 7,  14, 2, 'In Progress', 'High',    'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-03-28', '2026-05-11', NULL),
('MR-5043', 5,  16, 18, 2, 'In Progress', 'Critical', 'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-04-14', '2026-04-15', NULL),
('MR-5044', 63, 12, 19, 2, 'Pending',     'Critical', 'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-04-18', '2026-03-23', NULL),
('MR-5045', 34, 36, 19, 2, 'Pending',     'Critical', 'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-03-11', '2026-05-02', NULL),
('MR-5046', 17, 28, 12, 2, 'Pending',     'Medium',  'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-04-04', '2026-01-19', NULL),
('MR-5047', 45, 8,  22, 2, 'In Progress', 'Medium',  'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-03-19', '2026-03-12', NULL),
('MR-5048', 15, 26, 6,  1, 'Pending',     'Critical', 'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-01-15', '2026-01-28', NULL),
('MR-5049', 3,  35, 19, 1, 'Completed',   'High',    'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-05-13', '2026-04-19', '2026-05-14'),
('MR-5050', 13, 18, 11, 2, 'In Progress', 'Critical', 'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-02-20', '2026-05-04', NULL),
('MR-5051', 64, 45, 28, 2, 'In Progress', 'Critical', 'Touchscreen unresponsive',            'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-03-09', '2026-03-14', NULL),
('MR-5052', 53, 19, 20, 2, 'Pending',     'Critical', 'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-05-11', '2026-03-19', NULL),
('MR-5053', 55, 18, 12, 2, 'In Progress', 'Critical', 'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-05-26', '2026-01-19', NULL),
('MR-5054', 26, 31, 13, 2, 'In Progress', 'Critical', 'Power supply instability',              'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-05-06', '2026-04-22', NULL),
('MR-5055', 44, 36, 7,  2, 'Pending',     'High',    'Sensor malfunction',                  'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-05-24', '2026-05-28', NULL),
('MR-5056', 19, 20, 5,  2, 'Completed',   'High',    'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-03-28', '2026-01-17', '2026-02-12'),
('MR-5057', 35, 23, 21, 2, 'Pending',     'High',    'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-04-08', '2026-02-28', NULL),
('MR-5058', 32, 24, 15, 2, 'Pending',     'Critical', 'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-04-21', '2026-02-07', NULL),
('MR-5059', 18, 6,  9,  1, 'Pending',     'Critical', 'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-01-24', '2026-02-06', NULL),
('MR-5060', 41, 43, 10, 2, 'In Progress', 'High',    'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-01-13', '2026-01-02', NULL),
('MR-5061', 67, 42, 25, 2, 'Completed',   'High',    'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-01-08', '2026-02-08', '2026-01-19'),
('MR-5062', 54, 20, 30, 2, 'Completed',   'Medium',  'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-01-06', '2026-01-25', '2026-03-11'),
('MR-5063', 33, 42, 2,  2, 'Pending',     'Critical', 'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-01-11', '2026-03-15', NULL),
('MR-5064', 47, 35, 23, 2, 'Completed',   'High',    'Sensor malfunction',                  'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-05-02', '2026-04-16', '2026-02-09'),
('MR-5065', 40, 45, 23, 2, 'Completed',   'Critical', 'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-05-28', '2026-01-23', '2026-02-09'),
('MR-5066', 57, 30, 30, 2, 'Pending',     'High',    'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-02-10', '2026-04-13', NULL),
('MR-5067', 63, 31, 26, 2, 'Pending',     'High',    'Power supply instability',              'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-04-16', '2026-02-15', NULL),
('MR-5068', 13, 11, 27, 2, 'Completed',   'Critical', 'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-05-03', '2026-04-27', '2026-03-23'),
('MR-5069', 59, 6,  25, 2, 'In Progress', 'Critical', 'Power supply instability',              'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-04-26', '2026-04-20', NULL),
('MR-5070', 41, 41, 3,  1, 'Completed',   'Medium',  'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-05-23', '2026-02-01', '2026-04-07'),
('MR-5071', 52, 25, 15, 2, 'Completed',   'Critical', 'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-03-06', '2026-02-28', '2026-05-22'),
('MR-5072', 26, 10, 14, 1, 'Completed',   'Medium',  'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-04-27', '2026-05-27', '2026-03-26'),
('MR-5073', 57, 10, 15, 2, 'Completed',   'High',    'Touchscreen unresponsive',            'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-01-13', '2026-02-01', '2026-01-18'),
('MR-5074', 48, 15, 19, 2, 'Completed',   'Medium',  'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-04-27', '2026-04-20', '2026-03-05'),
('MR-5075', 25, 22, 10, 2, 'Completed',   'High',    'Touchscreen unresponsive',            'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-04-28', '2026-04-04', '2026-02-23'),
('MR-5076', 44, 17, 14, 1, 'Pending',     'High',    'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-01-10', '2026-05-07', NULL),
('MR-5077', 47, 20, 12, 2, 'In Progress', 'Critical', 'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-01-01', '2026-02-20', NULL),
('MR-5078', 39, 30, 22, 2, 'In Progress', 'Critical', 'Power supply instability',              'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-04-24', '2026-02-28', NULL),
('MR-5079', 7,  26, 30, 2, 'Completed',   'High',    'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-05-22', '2026-03-03', '2026-01-18'),
('MR-5080', 39, 30, 22, 2, 'Completed',   'Medium',  'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-05-06', '2026-01-01', '2026-03-17'),
('MR-5081', 60, 26, 29, 2, 'In Progress', 'Medium',  'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-03-23', '2026-02-17', NULL),
('MR-5082', 6,  13, 16, 2, 'Completed',   'Critical', 'Power supply instability',              'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-04-10', '2026-04-05', '2026-03-04'),
('MR-5083', 67, 25, 14, 1, 'In Progress', 'High',    'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-02-18', '2026-05-27', NULL),
('MR-5084', 30, 16, 2,  2, 'Completed',   'High',    'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-04-14', '2026-04-02', '2026-02-05'),
('MR-5085', 46, 40, 16, 2, 'Pending',     'High',    'Image quality degradation',           'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-02-04', '2026-04-02', NULL),
('MR-5086', 24, 15, 24, 1, 'In Progress', 'High',    'Preventive maintenance overdue',       'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-05-01', '2026-01-07', NULL),
('MR-5087', 59, 36, 22, 2, 'Completed',   'High',    'Touchscreen unresponsive',            'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-03-22', '2026-05-06', '2026-04-03'),
('MR-5088', 58, 25, 5,  2, 'Completed',   'Medium',  'Battery replacement required',         'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-04-15', '2026-01-13', '2026-03-19'),
('MR-5089', 34, 44, 15, 2, 'Completed',   'Critical', 'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 0, '2026-02-28', '2026-03-20', '2026-01-11'),
('MR-5090', 56, 9,  18, 2, 'Pending',     'High',    'Calibration required',                'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 0, '2026-04-09', '2026-03-11', NULL),
('MR-5091', 12, 6,  28, 2, 'Pending',     'Medium',  'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', NULL, 1, '2026-04-10', '2026-02-25', NULL),
('MR-5092', 45, 24, 29, 2, 'Completed',   'Medium',  'Software update needed',               'Technical issue identified during daily operational inspection.', 'Engineer assigned and diagnostic procedures initiated.', 'Maintenance completed successfully after full operational testing.', 1, '2026-04-12', '2026-04-20', '2026-04-25');



INSERT INTO Parts (Name, PartNumber, ManufacturerId, QuantityInStock, UnitPrice, MinimumStockLevel, Notes) VALUES
('Cooling Fan Module',      'FAN-CT-210',  2,  145, 637.00,  18, 'Approved spare part compatible with hospital equipment inventory.'),
('Power Supply Unit',       'PSU-MRI-88',  8,  139, 1142.00, 23, 'Approved spare part compatible with hospital equipment inventory.'),
('Touchscreen Display',     'LCD-VNT-44',  13, 30,  2363.00, 17, 'Approved spare part compatible with hospital equipment inventory.'),
('Battery Pack',            'BAT-INF-990', 4,  66,  2391.00, 19, 'Approved spare part compatible with hospital equipment inventory.'),
('ECG Sensor Module',       'ECG-MOD-120', 8,  12,  1555.00, 13, 'Approved spare part compatible with hospital equipment inventory.'),
('Oxygen Flow Sensor',      'OXY-SNS-774', 2,  102, 2039.00, 5,  'Approved spare part compatible with hospital equipment inventory.'),
('Pressure Valve Kit',      'PVK-550',     1,  29,  2079.00, 12, 'Approved spare part compatible with hospital equipment inventory.'),
('Infusion Pump Motor',     'MTR-INF-210', 17, 171, 1479.00, 9,  'Approved spare part compatible with hospital equipment inventory.'),
('Defibrillator Capacitor', 'DFC-700',     5,  193, 357.00,  17, 'Approved spare part compatible with hospital equipment inventory.'),
('Ultrasound Probe Cable',  'UPC-930',     7,  84,  1145.00, 22, 'Approved spare part compatible with hospital equipment inventory.'),
('MRI RF Coil',             'RFC-111',     6,  83,  1396.00, 10, 'Approved spare part compatible with hospital equipment inventory.'),
('Dialysis Pump Rotor',     'DPR-422',     8,  106, 2264.00, 15, 'Approved spare part compatible with hospital equipment inventory.'),
('Ventilator Air Filter',   'VAF-333',     2,  188, 105.00,  16, 'Approved spare part compatible with hospital equipment inventory.'),
('Blood Pressure Sensor',   'BPS-821',     7,  115, 2313.00, 24, 'Approved spare part compatible with hospital equipment inventory.'),
('Temperature Sensor',      'TMP-440',     4,  61,  2500.00, 21, 'Approved spare part compatible with hospital equipment inventory.');



INSERT INTO MaintenanceParts (MaintenanceRequestId, PartId, QuantityUsed, UnitPrice, Notes) VALUES
(6,   6,  1, 2500.00, 'Replacement component installed during corrective maintenance procedure.'),
(24,  2,  1, 2133.00, 'Replacement component installed during corrective maintenance procedure.'),
(48,  13, 1, 1178.00, 'Replacement component installed during corrective maintenance procedure.'),
(44,  1,  4, 1276.00, 'Replacement component installed during corrective maintenance procedure.'),
(56,  8,  3, 1425.00, 'Replacement component installed during corrective maintenance procedure.'),
(35,  14, 3, 1593.00, 'Replacement component installed during corrective maintenance procedure.'),
(13,  2,  2, 2346.00, 'Replacement component installed during corrective maintenance procedure.'),
(63,  10, 2, 1265.00, 'Replacement component installed during corrective maintenance procedure.'),
(71,  15, 4, 845.00,  'Replacement component installed during corrective maintenance procedure.'),
(85,  2,  3, 655.00,  'Replacement component installed during corrective maintenance procedure.'),
(12,  15, 4, 253.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 102 إلى 12
(37,  2,  2, 2342.00, 'Replacement component installed during corrective maintenance procedure.'),
(87,  13, 4, 725.00,  'Replacement component installed during corrective maintenance procedure.'),
(90,  8,  2, 118.00,  'Replacement component installed during corrective maintenance procedure.'),
(36,  5,  2, 398.00,  'Replacement component installed during corrective maintenance procedure.'),
(82,  8,  3, 2234.00, 'Replacement component installed during corrective maintenance procedure.'),
(18,  1,  4, 116.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 98 إلى 18
(32,  6,  4, 425.00,  'Replacement component installed during corrective maintenance procedure.'),
(84,  10, 4, 525.00,  'Replacement component installed during corrective maintenance procedure.'),
(66,  5,  1, 1452.00, 'Replacement component installed during corrective maintenance procedure.'),
(35,  1,  4, 481.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 135 إلى 35
(85,  9,  4, 353.00,  'Replacement component installed during corrective maintenance procedure.'),
(22,  7,  3, 593.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 99 إلى 22
(57,  3,  3, 668.00,  'Replacement component installed during corrective maintenance procedure.'),
(9,   13, 1, 2318.00, 'Replacement component installed during corrective maintenance procedure.'),
(7,   9,  2, 501.00,  'Replacement component installed during corrective maintenance procedure.'),
(11,  5,  1, 2323.00, 'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 111 إلى 11
(8,   12, 4, 1010.00, 'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 108 إلى 8
(10,  11, 2, 591.00,  'Replacement component installed during corrective maintenance procedure.'),
(89,  6,  1, 1239.00, 'Replacement component installed during corrective maintenance procedure.'),
(89,  15, 1, 1759.00, 'Replacement component installed during corrective maintenance procedure.'),
(29,  11, 2, 630.00,  'Replacement component installed during corrective maintenance procedure.'),
(37,  3,  4, 587.00,  'Replacement component installed during corrective maintenance procedure.'),
(5,   13, 1, 260.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 105 إلى 5
(37,  9,  3, 1511.00, 'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 137 إلى 37
(25,  13, 3, 602.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 125 إلى 25
(69,  13, 2, 1025.00, 'Replacement component installed during corrective maintenance procedure.'),
(55,  9,  3, 219.00,  'Replacement component installed during corrective maintenance procedure.'),
(10,  9,  2, 1168.00, 'Replacement component installed during corrective maintenance procedure.'),
(15,  9,  2, 815.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 115 إلى 15
(18,  3,  1, 2257.00, 'Replacement component installed during corrective maintenance procedure.'),
(11,  3,  3, 857.00,  'Replacement component installed during corrective maintenance procedure.'),
(50,  5,  4, 207.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 150 إلى 50
(88,  3,  2, 2360.00, 'Replacement component installed during corrective maintenance procedure.'),
(8,   4,  2, 1088.00, 'Replacement component installed during corrective maintenance procedure.'),
(41,  10, 1, 2461.00, 'Replacement component installed during corrective maintenance procedure.'),
(21,  8,  4, 1675.00, 'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 121 إلى 21
(27,  11, 3, 684.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 97 إلى 27
(71,  8,  2, 2274.00, 'Replacement component installed during corrective maintenance procedure.'),
(16,  7,  3, 239.00,  'Replacement component installed during corrective maintenance procedure.'),
(12,  3,  2, 1046.00, 'Replacement component installed during corrective maintenance procedure.'),
(25,  14, 2, 250.00,  'Replacement component installed during corrective maintenance procedure.'),
(37,  7,  1, 1263.00, 'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 137 إلى 37
(73,  13, 4, 2198.00, 'Replacement component installed during corrective maintenance procedure.'),
(42,  15, 1, 2435.00, 'Replacement component installed during corrective maintenance procedure.'),
(81,  1,  3, 1628.00, 'Replacement component installed during corrective maintenance procedure.'),
(65,  12, 4, 1517.00, 'Replacement component installed during corrective maintenance procedure.'),
(7,   6,  1, 2270.00, 'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 107 إلى 7
(55,  15, 1, 1410.00, 'Replacement component installed during corrective maintenance procedure.'),
(23,  15, 4, 1508.00, 'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 123 إلى 23
(7,   3,  1, 2230.00, 'Replacement component installed during corrective maintenance procedure.'),
(25,  2,  2, 373.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 125 إلى 25
(74,  1,  2, 741.00,  'Replacement component installed during corrective maintenance procedure.'),
(56,  12, 2, 2149.00, 'Replacement component installed during corrective maintenance procedure.'),
(39,  11, 1, 2028.00, 'Replacement component installed during corrective maintenance procedure.'),
(36,  8,  1, 2456.00, 'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 136 إلى 36
(16,  13, 3, 549.00,  'Replacement component installed during corrective maintenance procedure.'),
(15,  13, 1, 734.00,  'Replacement component installed during corrective maintenance procedure.'), -- تم التعديل من 115 إلى 15
(42,  11, 1, 1342.00, 'Replacement component installed during corrective maintenance procedure.'),
(22,  5,  1, 1290.00, 'Replacement component installed during corrective maintenance procedure.'), 
(65,  12, 4, 1150.00, 'Replacement component installed during corrective maintenance procedure.'),
(78,  4,  1, 271.00,  'Replacement component installed during corrective maintenance procedure.'),
(57,  6,  1, 2400.00, 'Replacement component installed during corrective maintenance procedure.'),
(24,  14, 4, 1528.00, 'Replacement component installed during corrective maintenance procedure.'),
(87,  15, 4, 224.00,  'Replacement component installed during corrective maintenance procedure.'),
(52,  5,  4, 84.00,   'Replacement component installed during corrective maintenance procedure.'),
(61,  10, 2, 1238.00, 'Replacement component installed during corrective maintenance procedure.'),
(13,  12, 1, 2027.00, 'Replacement component installed during corrective maintenance procedure.'),
(77,  14, 4, 1659.00, 'Replacement component installed during corrective maintenance procedure.'),
(51,  13, 2, 1380.00, 'Replacement component installed during corrective maintenance procedure.');

INSERT INTO DeviceWarranties (DeviceId, StartDate, EndDate, WarrantyProvider, Terms) VALUES
(65,  '2023-01-01', '2026-12-31', 'Drager',               'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'), -- تم التعديل من 97 إلى 65
(61,  '2024-01-01', '2027-12-31', 'GE Healthcare',        'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(64,  '2022-01-01', '2025-12-31', 'Drager',               'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(21,  '2025-01-01', '2028-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'), -- تم التعديل من 87 إلى 21
(30,  '2024-01-01', '2027-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(30,  '2025-01-01', '2028-12-31', 'Mindray',              'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(7,   '2022-01-01', '2025-12-31', 'Philips Medical',      'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(4,   '2024-01-01', '2027-12-31', 'Mindray',              'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(8,   '2025-01-01', '2028-12-31', 'Mindray',              'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(32,  '2024-01-01', '2027-12-31', 'Mindray',              'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(18,  '2024-01-01', '2027-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(1,   '2024-01-01', '2027-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(30,  '2024-01-01', '2027-12-31', 'Mindray',              'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(6,   '2023-01-01', '2026-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(13,  '2024-01-01', '2027-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(4,   '2024-01-01', '2027-12-31', 'Drager',               'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(48,  '2022-01-01', '2025-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(27,  '2025-01-01', '2028-12-31', 'GE Healthcare',        'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(39,  '2024-01-01', '2027-12-31', 'GE Healthcare',        'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(55,  '2024-01-01', '2027-12-31', 'Mindray',              'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'), -- تم التعديل من 96 إلى 55
(17,  '2022-01-01', '2025-12-31', 'GE Healthcare',        'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'), -- تم التعديل من 107 إلى 17
(2,   '2023-01-01', '2026-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(27,  '2025-01-01', '2028-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(10,  '2022-01-01', '2025-12-31', 'Mindray',              'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'), -- تم التعديل من 100 إلى 10
(11,  '2023-01-01', '2026-12-31', 'Philips Medical',      'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'), -- تم التعديل من 110 إلى 11
(20,  '2023-01-01', '2026-12-31', 'GE Healthcare',        'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'), -- تم التعديل من 100 إلى 20
(35,  '2025-01-01', '2028-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(39,  '2024-01-01', '2027-12-31', 'Siemens Healthineers', 'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(46,  '2024-01-01', '2027-12-31', 'Philips Medical',      'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.'),
(52,  '2023-01-01', '2026-12-31', 'Philips Medical',      'Comprehensive manufacturer warranty covering replacement parts, labor, and certified service maintenance.');






SELECT * FROM Admins;
SELECT * FROM Engineers;
SELECT * FROM Staff;


SELECT DeviceId, Name, Status, FailureCount 
FROM Devices 
WHERE DepartmentId = (SELECT DepartmentId FROM Staff WHERE Username = 'staff1')



SELECT 
    r.RequestId,
    r.RequestCode,
    r.Status,
    r.AssignedEngineerId,
    r.ReportedByStaffId,
    d.Name AS DeviceName,
    s.FullName AS ReportedBy
FROM MaintenanceRequests r
JOIN Devices d ON r.DeviceId = d.DeviceId
LEFT JOIN Engineers e ON r.AssignedEngineerId = e.EngineerId
LEFT JOIN Staff s ON r.ReportedByStaffId = s.StaffId
ORDER BY r.RequestDate DESC