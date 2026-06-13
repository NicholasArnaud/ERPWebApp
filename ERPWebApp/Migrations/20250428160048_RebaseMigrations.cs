using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPWebApp.Migrations;

/// <inheritdoc />
public partial class RebaseMigrations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AccessPlan",
            columns: table => new
            {
                AccessPlanId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccessPlanName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Has24HourAccess = table.Column<bool>(type: "bit", nullable: false),
                EarliestCheckInTime = table.Column<TimeSpan>(type: "time", nullable: false),
                LatestCheckInTime = table.Column<TimeSpan>(type: "time", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccessPlan", x => x.AccessPlanId);
            });

        migrationBuilder.CreateTable(
            name: "AccessPoint",
            columns: table => new
            {
                AccessPointId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccessPointLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                MacAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccessPoint", x => x.AccessPointId);
            });

        migrationBuilder.CreateTable(
            name: "AlertTriggerTemplateMappings",
            columns: table => new
            {
                AlertTemplateId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TriggerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                MessageContents = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AlertTriggerTemplateMappings", x => x.AlertTemplateId);
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BusinessEntity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PropertyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "BarcodeScan",
            columns: table => new
            {
                BarcodeScanId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BarcodeScanCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ShipStationOrderId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BarcodeScan", x => x.BarcodeScanId);
            });

        migrationBuilder.CreateTable(
            name: "Bundle",
            columns: table => new
            {
                BundleId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BundleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                FulfillmentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Bundle", x => x.BundleId);
            });

        migrationBuilder.CreateTable(
            name: "Department",
            columns: table => new
            {
                DepartmentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DepartmentName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                DepartmentColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                IsProduction = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Department", x => x.DepartmentId);
            });

        migrationBuilder.CreateTable(
            name: "DeputyTimeSheet",
            columns: table => new
            {
                DeputyTimeSheetId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DeputyId = table.Column<int>(type: "int", nullable: false),
                DeputyEmployeeId = table.Column<int>(type: "int", nullable: false),
                EmployeeHistory = table.Column<int>(type: "int", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                StartTime = table.Column<int>(type: "int", nullable: false),
                StartTimeLocalized = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndTime = table.Column<int>(type: "int", nullable: false),
                EndTimeLocalized = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsInProgress = table.Column<bool>(type: "bit", nullable: false),
                IsDiscarded = table.Column<bool>(type: "bit", nullable: false),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                MealBreak = table.Column<DateTime>(type: "datetime2", nullable: false),
                TotalTime = table.Column<float>(type: "real", nullable: false),
                TotalTimeInv = table.Column<float>(type: "real", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DeputyTimeSheet", x => x.DeputyTimeSheetId);
            });

        migrationBuilder.CreateTable(
            name: "DHLInvoices",
            columns: table => new
            {
                DHLInvoiceId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                fileName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ImportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ImportedBy = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                RecType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                SoldTo = table.Column<int>(type: "int", nullable: true),
                InvPosnr = table.Column<int>(type: "int", nullable: true),
                BOL = table.Column<int>(type: "int", nullable: true),
                BillRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                BillRef2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ProcessingFacility = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PickUpFrom = table.Column<int>(type: "int", nullable: true),
                PUDATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                PUTIME = table.Column<int>(type: "int", nullable: true),
                InternalTrackingNum = table.Column<long>(type: "bigint", nullable: true),
                CustomerConfirm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                DeliveryConfirm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Address1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Address2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Zip = table.Column<int>(type: "int", nullable: true),
                Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                MaterialOrVASNum = table.Column<int>(type: "int", nullable: true),
                MaterialOrVASDesc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ACTWeight = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                UOMACTWeight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                BILLWEIGHT = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                UOMBillWgt = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Quantity = table.Column<int>(type: "int", nullable: true),
                UOMQuantity = table.Column<int>(type: "int", nullable: true),
                PricingZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Charge = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                CustRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CustRef2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                WorkshareDropoff = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                WorkshareSort = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                WorkshareStamp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                WorkshareMachine = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                WorkshareManifest = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                WorkshareBPM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                WorkshareFutureUse1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                WorkshareFutureUse2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                WorkshareFutureUse3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SurchargeContentEndors = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                SurchargeUnassignableAdd = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                SurchargeSpecialHandling = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                SurchargeLateArrival = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                SurchargeUSPSQualif = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                SurchargeClientSRD = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                SurchargeIrregular = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                ReturnedMailUnassignable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReturnedMailUnprocessable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReturnedMailRecall = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReturnedMailDuplicate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReturnedMailContAssur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReturnedMailMoveUpdate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                GST_Tax = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                HST_Tax = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                PST_Tax = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                VAT_Tax = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                Duties = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Tax = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                ReturnedMailPaperInvoice = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReturnedMailScreening = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReturnedMailNonAutoFlats = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReturnedMailFutureUse = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SurchargeFuel = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                MinPickupCharge = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                OverlabeledValue = table.Column<decimal>(type: "decimal(30,2)", nullable: true),
                DimWeight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                UOMDimWeight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                DimLength = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                DimWidth = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                DimHeight = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                UOMDims = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PeakSurcharge = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                ReservedFutureUse1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReservedFutureUse2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReservedFutureUse3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReservedFutureUse4 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReservedFutureUse5 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DHLInvoices", x => x.DHLInvoiceId);
            });

        migrationBuilder.CreateTable(
            name: "EasyPostInvoices",
            columns: table => new
            {
                EasyPostInvoiceId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                fileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ImportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ImportedBy = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                TrackingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Service = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Rate = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Carrier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                InsuredValue = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                IsReturn = table.Column<bool>(type: "bit", nullable: false),
                RefundStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                LabelFee = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                PostageFee = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                InsuranceFee = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                Options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PostageLabelCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                RateId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ParcelId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                FromAddressId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                FromName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                FromCompany = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                FromStreet1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                FromStreet2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                FromCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                FromState = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                FromZip = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                FromCountry = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                FromResidential = table.Column<bool>(type: "bit", nullable: false),
                ToAddressId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ToName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ToCompany = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ToStreet1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ToStreet2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ToCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ToState = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                ToZip = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                ToCountry = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                ToResidential = table.Column<bool>(type: "bit", nullable: false),
                Length = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                Width = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                Height = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                Weight = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                PredefinedPackage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EasyPostInvoices", x => x.EasyPostInvoiceId);
            });

        migrationBuilder.CreateTable(
            name: "Fonts",
            columns: table => new
            {
                FontId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FontTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Fonts", x => x.FontId);
            });

        migrationBuilder.CreateTable(
            name: "InventoryBalance",
            columns: table => new
            {
                InventoryBalanceId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Sku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TotalAvailable = table.Column<int>(type: "int", nullable: false),
                PendingShipStationOrders = table.Column<int>(type: "int", nullable: false),
                OrderDifference = table.Column<int>(type: "int", nullable: false),
                IsExternalSiteInventory = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InventoryBalance", x => x.InventoryBalanceId);
            });

        migrationBuilder.CreateTable(
            name: "MyDash",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                SpeedOMeter = table.Column<bool>(type: "bit", nullable: false),
                DepartmentOrderHistory = table.Column<bool>(type: "bit", nullable: false),
                TopDepartment = table.Column<bool>(type: "bit", nullable: false),
                YearlyProfit = table.Column<bool>(type: "bit", nullable: false),
                HistoricalTrends = table.Column<bool>(type: "bit", nullable: false),
                TotalFulfillmentSales = table.Column<bool>(type: "bit", nullable: false),
                TopProductSales = table.Column<bool>(type: "bit", nullable: false),
                SiteVolumetrics = table.Column<bool>(type: "bit", nullable: false),
                ProductCyleCount = table.Column<bool>(type: "bit", nullable: false),
                TopRequestedProducts = table.Column<bool>(type: "bit", nullable: false),
                TopMovedProducts = table.Column<bool>(type: "bit", nullable: false),
                TopReasonRequest = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MyDash", x => x.UserId);
            });

        migrationBuilder.CreateTable(
            name: "NirfForm",
            columns: table => new
            {
                NirfFormId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SellersProductSku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Orientation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                DesignPlacement = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                DesignAlignment = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                SpecialInstructions = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                AspUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                StartedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                NirfStatus = table.Column<int>(type: "int", nullable: false),
                IsLoopCount = table.Column<bool>(type: "bit", nullable: false),
                IsSpeed = table.Column<bool>(type: "bit", nullable: false),
                IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                IsFrequency = table.Column<bool>(type: "bit", nullable: false),
                IsSizingX = table.Column<bool>(type: "bit", nullable: false),
                IsSizingY = table.Column<bool>(type: "bit", nullable: false),
                IsWhiteLayer = table.Column<bool>(type: "bit", nullable: false),
                IsColorLayer = table.Column<bool>(type: "bit", nullable: false),
                IsUVPType = table.Column<bool>(type: "bit", nullable: false),
                IsTemperature = table.Column<bool>(type: "bit", nullable: false),
                IsFont = table.Column<bool>(type: "bit", nullable: false),
                IsThreadColor = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfForm", x => x.NirfFormId);
            });

        migrationBuilder.CreateTable(
            name: "OrderShippingInfo",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                street1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                street2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                street3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                company = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                postalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                city = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                residential = table.Column<bool>(type: "bit", nullable: true),
                state = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderShippingInfo", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "OrderSource",
            columns: table => new
            {
                OrderSourceId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(50)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderSource", x => x.OrderSourceId);
            });

        migrationBuilder.CreateTable(
            name: "OrderTags",
            columns: table => new
            {
                OrderTagId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                tagId = table.Column<int>(type: "int", nullable: false),
                name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderTags", x => x.OrderTagId);
            });

        migrationBuilder.CreateTable(
            name: "ProductionVsLaborCostHistory",
            columns: table => new
            {
                ProductionVsLaborCostHistoryId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                ElectroplatingItemCost = table.Column<decimal>(type: "money", nullable: false),
                EmbroideryItemCost = table.Column<decimal>(type: "money", nullable: false),
                EngravingItemCost = table.Column<decimal>(type: "money", nullable: false),
                MetalTotalItemCost = table.Column<decimal>(type: "money", nullable: false),
                UVPTotalItemCost = table.Column<decimal>(type: "money", nullable: false),
                ElectroplatingProdCost = table.Column<decimal>(type: "money", nullable: false),
                EmbroideryProdCost = table.Column<decimal>(type: "money", nullable: false),
                EngravingProdCost = table.Column<decimal>(type: "money", nullable: false),
                MetalTotalProdCost = table.Column<decimal>(type: "money", nullable: false),
                UVPTotalProdCost = table.Column<decimal>(type: "money", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductionVsLaborCostHistory", x => x.ProductionVsLaborCostHistoryId);
            });

        migrationBuilder.CreateTable(
            name: "ProductionVsLaborCostPrice",
            columns: table => new
            {
                ProductionVsLaborCostPriceId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ElectroplatingItemCost = table.Column<decimal>(type: "money", nullable: false),
                EmbroideryItemCost = table.Column<decimal>(type: "money", nullable: false),
                EngravingItemCost = table.Column<decimal>(type: "money", nullable: false),
                MetalItemCost = table.Column<decimal>(type: "money", nullable: false),
                UVItemCost = table.Column<decimal>(type: "money", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductionVsLaborCostPrice", x => x.ProductionVsLaborCostPriceId);
            });

        migrationBuilder.CreateTable(
            name: "ProductTagsRegistry",
            columns: table => new
            {
                TagId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Description = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductTagsRegistry", x => x.TagId);
            });

        migrationBuilder.CreateTable(
            name: "QCDiagnosis",
            columns: table => new
            {
                QCDiagnosisId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                QCDiagnosisName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QCDiagnosis", x => x.QCDiagnosisId);
            });

        migrationBuilder.CreateTable(
            name: "RedoOrder",
            columns: table => new
            {
                RedoOrderId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RedoReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DateReported = table.Column<DateTime>(type: "datetime2", nullable: false),
                OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ItemSku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Quantity = table.Column<int>(type: "int", nullable: false),
                LoggedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RedoOrder", x => x.RedoOrderId);
            });

        migrationBuilder.CreateTable(
            name: "SalesReport",
            columns: table => new
            {
                SalesReportId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Sku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                QuantitySold = table.Column<int>(type: "int", nullable: false),
                CostPerItem = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesReport", x => x.SalesReportId);
            });

        migrationBuilder.CreateTable(
            name: "SellerMargin",
            columns: table => new
            {
                OrderNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                FulfillmentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                StoreCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                StoreShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                StoreSale = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ProfitMargin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                StoreName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ShipStationStoreId = table.Column<int>(type: "int", nullable: false),
                OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SellerMargin", x => x.OrderNumber);
            });

        migrationBuilder.CreateTable(
            name: "SellerMargins",
            columns: table => new
            {
                OrderNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ShipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                StoreName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ServiceCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                StoreItemsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CustomerItemsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ShipmentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                StoreCostWithEtsy = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                StoreCostDiffSubfulfillmentAndShipping = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SellerMargins", x => x.OrderNumber);
            });

        migrationBuilder.CreateTable(
            name: "ShippingManifests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ManifestId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                WarehouseId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Warehouse = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                CarrierId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Carrier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ShipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ShipmentCount = table.Column<int>(type: "int", nullable: false),
                ManifestFile = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShippingManifests", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ShippingProvider",
            columns: table => new
            {
                ShippingProviderId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ShippingProviderName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShippingProvider", x => x.ShippingProviderId);
            });

        migrationBuilder.CreateTable(
            name: "ShipStationAwaitingOrder",
            columns: table => new
            {
                ShipStationAwaitingOrderId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                OrderStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CustomerNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ItemSku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ItemQuantity = table.Column<int>(type: "int", nullable: false),
                StoreId = table.Column<int>(type: "int", nullable: false),
                OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                SSOrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ItemOptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TagIds = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShipStationAwaitingOrder", x => x.ShipStationAwaitingOrderId);
            });

        migrationBuilder.CreateTable(
            name: "ShipStationOrderedHistory",
            columns: table => new
            {
                ShipStationOrderedHistoryId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Sku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TotalFromAllLocations = table.Column<int>(type: "int", nullable: false),
                OnOrder = table.Column<int>(type: "int", nullable: false),
                LeadTime = table.Column<int>(type: "int", nullable: false),
                OrderedIn24Hours = table.Column<int>(type: "int", nullable: false),
                OrderedIn3Days = table.Column<int>(type: "int", nullable: false),
                OrderedIn7Days = table.Column<int>(type: "int", nullable: false),
                OrderedIn15Days = table.Column<int>(type: "int", nullable: false),
                OrderedIn30Days = table.Column<int>(type: "int", nullable: false),
                OrderedIn90Days = table.Column<int>(type: "int", nullable: false),
                SalesTrend = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShipStationOrderedHistory", x => x.ShipStationOrderedHistoryId);
            });

        migrationBuilder.CreateTable(
            name: "ShipStationStore",
            columns: table => new
            {
                ShipStationStoreId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                StoreId = table.Column<int>(type: "int", nullable: false),
                StoreName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PublicEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                HasIncreasedPricing = table.Column<bool>(type: "bit", nullable: false),
                StoreType = table.Column<int>(type: "int", nullable: false),
                ContactName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                FaxNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShipStationStore", x => x.ShipStationStoreId);
            });

        migrationBuilder.CreateTable(
            name: "Site",
            columns: table => new
            {
                SiteId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SiteName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                SiteDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                SiteVolume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                IsRestricted = table.Column<bool>(type: "bit", nullable: false),
                IsExternal = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Site", x => x.SiteId);
            });

        migrationBuilder.CreateTable(
            name: "SkuCategory",
            columns: table => new
            {
                SkuCategoryId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Attribute = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SkuCategory", x => x.SkuCategoryId);
            });

        migrationBuilder.CreateTable(
            name: "SkuColor",
            columns: table => new
            {
                SkuColorId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Color = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                Attribute = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SkuColor", x => x.SkuColorId);
            });

        migrationBuilder.CreateTable(
            name: "SkulabsImport",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Store = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Order = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                TrackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Line_ID = table.Column<long>(type: "bigint", nullable: false),
                OrderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Archived = table.Column<bool>(type: "bit", nullable: false),
                Image = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                ListingName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                VariantName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                SKU = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Cost = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                Wholesale = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                Retail = table.Column<float>(type: "real", nullable: true),
                PriceSold = table.Column<int>(type: "int", nullable: true),
                Quantity = table.Column<int>(type: "int", nullable: false),
                DropShipped = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Cleared = table.Column<bool>(type: "bit", nullable: false),
                PickedQuantity = table.Column<int>(type: "int", nullable: false),
                OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ShipmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ManualShipment = table.Column<bool>(type: "bit", nullable: true),
                Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                AssignedWarehouse = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                LineSKU = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                LineName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                CustomerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CustomerEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Company = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CustomerNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                AddressLine1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                AddressLine2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Zip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Postage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                _3PLPartnerSKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                OrderTags = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Personalization1Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Personalization1Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Personalization2Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Personalization2Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Personalization3Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Personalization3Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Personalization4Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Personalization4Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Personalization5Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Personalization5Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Personalization6Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Personalization6Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Personalization7Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Personalization7Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Personalization8Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Personalization8Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                Personalization9Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Personalization9Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                fileName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ImportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ImportedBy = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SkulabsImport", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SkuUnitOfMeasure",
            columns: table => new
            {
                SkuUnitOfMeasureId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UnitOfMeasure = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                Attribute = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SkuUnitOfMeasure", x => x.SkuUnitOfMeasureId);
            });

        migrationBuilder.CreateTable(
            name: "SpeedOMeterGoal",
            columns: table => new
            {
                SpeedOMeterGoalId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ElectroplatingGoal = table.Column<int>(type: "int", nullable: false),
                EmbroideryGoal = table.Column<int>(type: "int", nullable: false),
                EngravingGoal = table.Column<int>(type: "int", nullable: false),
                MetalGoal = table.Column<int>(type: "int", nullable: false),
                UVGoal = table.Column<int>(type: "int", nullable: false),
                SublimationGoal = table.Column<int>(type: "int", nullable: false),
                PlantGoal = table.Column<int>(type: "int", nullable: false),
                WoodGoal = table.Column<int>(type: "int", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SpeedOMeterGoal", x => x.SpeedOMeterGoalId);
            });

        migrationBuilder.CreateTable(
            name: "StampsUSPSInvoices",
            columns: table => new
            {
                StampsUSPSInvoiceId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                orderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                fileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ImportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ImportedBy = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                DatePrinted = table.Column<DateTime>(type: "datetime2", nullable: true),
                AmountPaid = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                AdjustedAmount = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                QuotedAmount = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                PaymentType = table.Column<int>(type: "int", nullable: true),
                Shipment = table.Column<int>(type: "int", nullable: true),
                TrackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                DateDelivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                Recipient = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Address1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Address2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Address3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                StateOrProvince = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PostalCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                OriginZip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Weight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Carrier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Service = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                TrackingConfirmation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ExtraService = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                InsuredFor = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CostCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PrintedMessage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                User = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                RefundType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                RefundRequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                RefundStatus = table.Column<int>(type: "int", nullable: true),
                RefundRequested = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                Reference1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                OrderID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Store = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                OrderTotal = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ItemSKUs = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Items = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ProductTotal = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ShippingPaid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                TaxPaid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                InsuranceProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                DutiesTaxesAmount = table.Column<decimal>(type: "decimal(16,4)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StampsUSPSInvoices", x => x.StampsUSPSInvoiceId);
            });

        migrationBuilder.CreateTable(
            name: "SubCategory",
            columns: table => new
            {
                SubCategoryId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubCategory", x => x.SubCategoryId);
            });

        migrationBuilder.CreateTable(
            name: "UPSInvoices",
            columns: table => new
            {
                UPSInvoiceId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                fileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ImportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ImportedBy = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                CustomerNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                LineOfBusiness = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                AirbillNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShipDate = table.Column<DateTime>(type: "datetime2", maxLength: 50, nullable: true),
                ProNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                BolNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Scac = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                BillType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShippersName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShippersAddress1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShippersAddress2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShippersAddress3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShippersCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShippersState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShippersZip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverAddress1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverAddress2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverAddress3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverZip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ConsigneeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ConsigneeCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ConsigneeState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ConsigneeZip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                OriginatingCustomer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CustomerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CustomerAddress1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CustomerAddress2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CustomerCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CustomerState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CustomerZip = table.Column<int>(type: "int", nullable: true),
                HandlingUnit = table.Column<int>(type: "int", nullable: true),
                Pieces = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                OriginalWeight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ChargedWeight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Class = table.Column<int>(type: "int", nullable: true),
                ChargeType1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ChargeAmount1 = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ChargeType2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ChargeAmount2 = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ChargeType3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ChargeAmount3 = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ChargeType4 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ChargeAmount4 = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ChargeType5 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ChargeAmount5 = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ChargeType6 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ChargeAmount6 = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ChargeType7 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ChargeAmount7 = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ChargeType8 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ChargeAmount8 = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                ChargeTotal = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                BillingReference1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                BillingReference2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                VendorReference1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                VendorReference2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SentBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ServiceLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Zone = table.Column<int>(type: "int", nullable: true),
                YouOweAs = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                Description1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Description2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Description3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Description4 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PickupLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SenderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverLine1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ReceiverLine2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PackageReference1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PackageReference2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PackageReference3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PackageReference4 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PackageReference5 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PackageReference6 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PackageReference7 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PackageReference8 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                UpsNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UPSInvoices", x => x.UPSInvoiceId);
            });

        migrationBuilder.CreateTable(
            name: "UserRolesViewModel",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Role = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRolesViewModel", x => x.UserId);
            });

        migrationBuilder.CreateTable(
            name: "Vendor",
            columns: table => new
            {
                VendorId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VendorNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                VendorName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BusinessEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Address1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                IsExternal = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Vendor", x => x.VendorId);
            });

        migrationBuilder.CreateTable(
            name: "WebHookBatch",
            columns: table => new
            {
                WebHookBatchId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                WebhookURL = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                RequestHeaders = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ResponseStatus = table.Column<int>(type: "int", nullable: false),
                ResponseHeaders = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ErrorMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                ErrorStackTrace = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                RetryCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WebHookBatch", x => x.WebHookBatchId);
            });

        migrationBuilder.CreateTable(
            name: "AccessPlanDoor",
            columns: table => new
            {
                AccessPlanDoorId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccessPlanId = table.Column<int>(type: "int", nullable: false),
                AccessPointId = table.Column<int>(type: "int", nullable: false),
                CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccessPlanDoor", x => x.AccessPlanDoorId);
                table.ForeignKey(
                    name: "FK_AccessPlanDoor_AccessPlan_AccessPlanId",
                    column: x => x.AccessPlanId,
                    principalTable: "AccessPlan",
                    principalColumn: "AccessPlanId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AccessPlanDoor_AccessPoint_AccessPointId",
                    column: x => x.AccessPointId,
                    principalTable: "AccessPoint",
                    principalColumn: "AccessPointId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EmailAlerts",
            columns: table => new
            {
                EmailAlertId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Subject = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Body = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                AlertType = table.Column<int>(type: "int", nullable: false),
                Frequency = table.Column<int>(type: "int", nullable: true),
                AlertTemplateId = table.Column<int>(type: "int", nullable: true),
                ScheduledTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailAlerts", x => x.EmailAlertId);
                table.ForeignKey(
                    name: "FK_EmailAlerts_AlertTriggerTemplateMappings_AlertTemplateId",
                    column: x => x.AlertTemplateId,
                    principalTable: "AlertTriggerTemplateMappings",
                    principalColumn: "AlertTemplateId");
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BatchItemStatus",
            columns: table => new
            {
                BatchItemStatusId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                DepartmentId = table.Column<int>(type: "int", nullable: false),
                ExecutionSequence = table.Column<int>(type: "int", nullable: false),
                IsDeletable = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BatchItemStatus", x => x.BatchItemStatusId);
                table.ForeignKey(
                    name: "FK_BatchItemStatus_Department_DepartmentId",
                    column: x => x.DepartmentId,
                    principalTable: "Department",
                    principalColumn: "DepartmentId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DepartmentRoleMapping",
            columns: table => new
            {
                DepartmentRoleId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DepartmentId = table.Column<int>(type: "int", nullable: false),
                UserRoleId = table.Column<string>(type: "nvarchar(450)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DepartmentRoleMapping", x => x.DepartmentRoleId);
                table.ForeignKey(
                    name: "FK_DepartmentRoleMapping_AspNetRoles_UserRoleId",
                    column: x => x.UserRoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_DepartmentRoleMapping_Department_DepartmentId",
                    column: x => x.DepartmentId,
                    principalTable: "Department",
                    principalColumn: "DepartmentId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "QCStationLocation",
            columns: table => new
            {
                QCStationLocationId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                QCStationLocationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DepartmentId = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QCStationLocation", x => x.QCStationLocationId);
                table.ForeignKey(
                    name: "FK_QCStationLocation_Department_DepartmentId",
                    column: x => x.DepartmentId,
                    principalTable: "Department",
                    principalColumn: "DepartmentId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserPreferences",
            columns: table => new
            {
                PreferencesId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                PreferDashboard = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Theme = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                PreferDepartment = table.Column<int>(type: "int", nullable: true),
                DashboardConfig = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserPreferences", x => x.PreferencesId);
                table.ForeignKey(
                    name: "FK_UserPreferences_Department_PreferDepartment",
                    column: x => x.PreferDepartment,
                    principalTable: "Department",
                    principalColumn: "DepartmentId");
            });

        migrationBuilder.CreateTable(
            name: "NirfForecasting",
            columns: table => new
            {
                NirfForecastingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                LeadTime = table.Column<int>(type: "int", nullable: false),
                MinMaxLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Count = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SignedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SignedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                AspUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                NirfFormId = table.Column<int>(type: "int", nullable: false),
                Comments = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfForecasting", x => x.NirfForecastingId);
                table.ForeignKey(
                    name: "FK_NirfForecasting_NirfForm_NirfFormId",
                    column: x => x.NirfFormId,
                    principalTable: "NirfForm",
                    principalColumn: "NirfFormId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "NirfPackaging",
            columns: table => new
            {
                NirfPackagingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BoxSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Bag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                FoamWrap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                BubbleSleeve = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                UnitsPerBox = table.Column<int>(type: "int", nullable: true),
                UnitsPerBag = table.Column<int>(type: "int", nullable: true),
                SignedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                AspUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SignedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                Height = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Width = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Length = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ContainerDiminsion = table.Column<int>(type: "int", nullable: false),
                UnitsPerContainer = table.Column<int>(type: "int", nullable: true),
                NirfFormId = table.Column<int>(type: "int", nullable: false),
                Comments = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfPackaging", x => x.NirfPackagingId);
                table.ForeignKey(
                    name: "FK_NirfPackaging_NirfForm_NirfFormId",
                    column: x => x.NirfFormId,
                    principalTable: "NirfForm",
                    principalColumn: "NirfFormId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "NirfParameters",
            columns: table => new
            {
                NirfParametersId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                LoopCount = table.Column<int>(type: "int", nullable: true),
                Speed = table.Column<int>(type: "int", nullable: true),
                Current = table.Column<int>(type: "int", nullable: true),
                Frequency = table.Column<int>(type: "int", nullable: true),
                SizingX = table.Column<int>(type: "int", nullable: true),
                SizingY = table.Column<int>(type: "int", nullable: false),
                WhiteLayers = table.Column<int>(type: "int", nullable: true),
                ColorLayers = table.Column<int>(type: "int", nullable: true),
                SignedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SignedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                AspUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                NirfFormId = table.Column<int>(type: "int", nullable: false),
                Comments = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                UVPTypes = table.Column<int>(type: "int", nullable: false),
                ThreadTypes = table.Column<int>(type: "int", nullable: false),
                TimeToComplete = table.Column<DateTime>(type: "datetime2", nullable: false),
                Temperature = table.Column<int>(type: "int", nullable: true),
                IsFahrenheit = table.Column<bool>(type: "bit", nullable: true),
                ThreadColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ThreadHex = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ThreadCode = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                FontId = table.Column<int>(type: "int", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfParameters", x => x.NirfParametersId);
                table.ForeignKey(
                    name: "FK_NirfParameters_Fonts_FontId",
                    column: x => x.FontId,
                    principalTable: "Fonts",
                    principalColumn: "FontId");
                table.ForeignKey(
                    name: "FK_NirfParameters_NirfForm_NirfFormId",
                    column: x => x.NirfFormId,
                    principalTable: "NirfForm",
                    principalColumn: "NirfFormId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "NirfShipping",
            columns: table => new
            {
                NirfShippingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SignedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SignedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                AspUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                NirfFormId = table.Column<int>(type: "int", nullable: false),
                Comments = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfShipping", x => x.NirfShippingId);
                table.ForeignKey(
                    name: "FK_NirfShipping_NirfForm_NirfFormId",
                    column: x => x.NirfFormId,
                    principalTable: "NirfForm",
                    principalColumn: "NirfFormId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                ERPOrderId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                orderId = table.Column<long>(type: "bigint", nullable: false),
                orderNumber = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                orderKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                orderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                createDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                modifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                paymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                shipByDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                orderStatus = table.Column<int>(type: "int", nullable: false),
                customerId = table.Column<long>(type: "bigint", nullable: true),
                customerUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                customerEmail = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                billToId = table.Column<int>(type: "int", nullable: true),
                shipToId = table.Column<int>(type: "int", nullable: true),
                shipFromId = table.Column<int>(type: "int", nullable: true),
                orderTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                amountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                taxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                shippingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                customerNotes = table.Column<string>(type: "nvarchar(3500)", maxLength: 3500, nullable: true),
                internalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                gift = table.Column<bool>(type: "bit", nullable: false),
                giftMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                paymentMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                requestedShippingService = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                carrierCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                carrierNickname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                serviceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                packageCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                confirmation = table.Column<int>(type: "int", nullable: false),
                shipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                holdUntilDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                userId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                externallyFulfilled = table.Column<bool>(type: "bit", nullable: false),
                externallyFulfilledBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                estimatedShipmentCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                isExpedited = table.Column<bool>(type: "bit", nullable: false),
                carrierId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ERPTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                ERPModifyByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                duplicationReason = table.Column<int>(type: "int", nullable: true),
                IsDuplicated = table.Column<bool>(type: "bit", nullable: true),
                ParentERPOrderId = table.Column<int>(type: "int", nullable: true),
                dimensions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                insuranceOptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                internationalOptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                weight = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.ERPOrderId);
                table.ForeignKey(
                    name: "FK_Orders_OrderShippingInfo_billToId",
                    column: x => x.billToId,
                    principalTable: "OrderShippingInfo",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Orders_OrderShippingInfo_shipFromId",
                    column: x => x.shipFromId,
                    principalTable: "OrderShippingInfo",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Orders_OrderShippingInfo_shipToId",
                    column: x => x.shipToId,
                    principalTable: "OrderShippingInfo",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Orders_Orders_ParentERPOrderId",
                    column: x => x.ParentERPOrderId,
                    principalTable: "Orders",
                    principalColumn: "ERPOrderId");
            });

        migrationBuilder.CreateTable(
            name: "Warehouse",
            columns: table => new
            {
                WarehouseId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                WarehouseName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                DefaultWarehouse = table.Column<bool>(type: "bit", nullable: false),
                Company = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                StreetAddress1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                StreetAddress2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                SameAsReturnAddress = table.Column<bool>(type: "bit", nullable: false),
                BillingAddressId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Warehouse", x => x.WarehouseId);
                table.ForeignKey(
                    name: "FK_Warehouse_OrderShippingInfo_BillingAddressId",
                    column: x => x.BillingAddressId,
                    principalTable: "OrderShippingInfo",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "ShippingMethod",
            columns: table => new
            {
                ShippingMethodId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ShippingProviderId = table.Column<int>(type: "int", nullable: false),
                ShippingMethodName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShippingMethod", x => x.ShippingMethodId);
                table.ForeignKey(
                    name: "FK_ShippingMethod_ShippingProvider_ShippingProviderId",
                    column: x => x.ShippingProviderId,
                    principalTable: "ShippingProvider",
                    principalColumn: "ShippingProviderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CycleCountFrequency",
            columns: table => new
            {
                CycleCountFrequencyId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BaseDays = table.Column<int>(type: "int", nullable: false),
                Over1000 = table.Column<int>(type: "int", nullable: false),
                Cost10 = table.Column<int>(type: "int", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SiteId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CycleCountFrequency", x => x.CycleCountFrequencyId);
                table.ForeignKey(
                    name: "FK_CycleCountFrequency_Site_SiteId",
                    column: x => x.SiteId,
                    principalTable: "Site",
                    principalColumn: "SiteId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Location",
            columns: table => new
            {
                LocationId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SiteId = table.Column<int>(type: "int", nullable: false),
                LocationName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                LocationDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Type = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                IsExternal = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Location", x => x.LocationId);
                table.ForeignKey(
                    name: "FK_Location_Site_SiteId",
                    column: x => x.SiteId,
                    principalTable: "Site",
                    principalColumn: "SiteId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserSiteMapping",
            columns: table => new
            {
                UserSiteMappingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                SiteId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSiteMapping", x => x.UserSiteMappingId);
                table.ForeignKey(
                    name: "FK_UserSiteMapping_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserSiteMapping_Site_SiteId",
                    column: x => x.SiteId,
                    principalTable: "Site",
                    principalColumn: "SiteId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Product",
            columns: table => new
            {
                ProductId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SubCategoryId = table.Column<int>(type: "int", nullable: true),
                Sku = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                FulfillmentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                AltItemNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AlternateProductId = table.Column<int>(type: "int", nullable: true),
                OnOrder = table.Column<int>(type: "int", nullable: false),
                IsEmbroidery = table.Column<bool>(type: "bit", nullable: false),
                IsEngraving = table.Column<bool>(type: "bit", nullable: false),
                IsMetal = table.Column<bool>(type: "bit", nullable: false),
                IsUv = table.Column<bool>(type: "bit", nullable: false),
                LeadTime = table.Column<int>(type: "int", nullable: false),
                WeightAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                WeightUnit = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ModifySource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Height = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Width = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Length = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                DimensionalUnit = table.Column<int>(type: "int", nullable: false),
                IsExternalProduct = table.Column<bool>(type: "bit", nullable: false),
                MinInventory = table.Column<int>(type: "int", nullable: false),
                MaxInventory = table.Column<int>(type: "int", nullable: false),
                OverseasCost = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                ShippingWeightAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ShippingWeightUnit = table.Column<int>(type: "int", nullable: false),
                IsShippingContainer = table.Column<bool>(type: "bit", nullable: false),
                ExpectedShipmentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ShippingHeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ShippingWidth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ShippingLength = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Product", x => x.ProductId);
                table.ForeignKey(
                    name: "FK_Product_Product_AlternateProductId",
                    column: x => x.AlternateProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId");
                table.ForeignKey(
                    name: "FK_Product_SubCategory_SubCategoryId",
                    column: x => x.SubCategoryId,
                    principalTable: "SubCategory",
                    principalColumn: "SubCategoryId");
            });

        migrationBuilder.CreateTable(
            name: "Employee",
            columns: table => new
            {
                EmployeeId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                FullName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                UserRolesViewModelId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                Position = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DepartmentId = table.Column<int>(type: "int", nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PersonalEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CompanyEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                JobStatus = table.Column<int>(type: "int", nullable: false),
                IncomePerHour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                EmployeeReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ApsuId = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employee", x => x.EmployeeId);
                table.ForeignKey(
                    name: "FK_Employee_Department_DepartmentId",
                    column: x => x.DepartmentId,
                    principalTable: "Department",
                    principalColumn: "DepartmentId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Employee_UserRolesViewModel_UserRolesViewModelId",
                    column: x => x.UserRolesViewModelId,
                    principalTable: "UserRolesViewModel",
                    principalColumn: "UserId");
            });

        migrationBuilder.CreateTable(
            name: "NirfVendorMapping",
            columns: table => new
            {
                NirfVendorMappingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VendorId = table.Column<int>(type: "int", nullable: false),
                SignedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SignedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                AspUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                NirfFormId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfVendorMapping", x => x.NirfVendorMappingId);
                table.ForeignKey(
                    name: "FK_NirfVendorMapping_NirfForm_NirfFormId",
                    column: x => x.NirfFormId,
                    principalTable: "NirfForm",
                    principalColumn: "NirfFormId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_NirfVendorMapping_Vendor_VendorId",
                    column: x => x.VendorId,
                    principalTable: "Vendor",
                    principalColumn: "VendorId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserEmailAlertMapping",
            columns: table => new
            {
                UserEmailAlertMappingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                EmailAlertId = table.Column<int>(type: "int", nullable: false),
                UserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserEmailAlertMapping", x => x.UserEmailAlertMappingId);
                table.ForeignKey(
                    name: "FK_UserEmailAlertMapping_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserEmailAlertMapping_EmailAlerts_EmailAlertId",
                    column: x => x.EmailAlertId,
                    principalTable: "EmailAlerts",
                    principalColumn: "EmailAlertId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "NirfShippingProvider",
            columns: table => new
            {
                NirfShippingProviderId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ShippingProviderId = table.Column<int>(type: "int", nullable: false),
                ShippingWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ShippingSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                NirfShippingId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfShippingProvider", x => x.NirfShippingProviderId);
                table.ForeignKey(
                    name: "FK_NirfShippingProvider_NirfShipping_NirfShippingId",
                    column: x => x.NirfShippingId,
                    principalTable: "NirfShipping",
                    principalColumn: "NirfShippingId");
                table.ForeignKey(
                    name: "FK_NirfShippingProvider_ShippingProvider_ShippingProviderId",
                    column: x => x.ShippingProviderId,
                    principalTable: "ShippingProvider",
                    principalColumn: "ShippingProviderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "InvoicedOrders",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DateInvoiced = table.Column<DateTime>(type: "datetime2", nullable: true),
                OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                OrderCarrier = table.Column<int>(type: "int", nullable: false),
                ERPOrderId = table.Column<int>(type: "int", nullable: true),
                TrackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                EasyPostInvoiceId = table.Column<int>(type: "int", nullable: true),
                DHLInvoiceId = table.Column<int>(type: "int", nullable: true),
                UPSInvoiceId = table.Column<int>(type: "int", nullable: true),
                StampsUSPSInvoiceId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InvoicedOrders", x => x.Id);
                table.ForeignKey(
                    name: "FK_InvoicedOrders_DHLInvoices_DHLInvoiceId",
                    column: x => x.DHLInvoiceId,
                    principalTable: "DHLInvoices",
                    principalColumn: "DHLInvoiceId");
                table.ForeignKey(
                    name: "FK_InvoicedOrders_EasyPostInvoices_EasyPostInvoiceId",
                    column: x => x.EasyPostInvoiceId,
                    principalTable: "EasyPostInvoices",
                    principalColumn: "EasyPostInvoiceId");
                table.ForeignKey(
                    name: "FK_InvoicedOrders_Orders_ERPOrderId",
                    column: x => x.ERPOrderId,
                    principalTable: "Orders",
                    principalColumn: "ERPOrderId");
                table.ForeignKey(
                    name: "FK_InvoicedOrders_StampsUSPSInvoices_StampsUSPSInvoiceId",
                    column: x => x.StampsUSPSInvoiceId,
                    principalTable: "StampsUSPSInvoices",
                    principalColumn: "StampsUSPSInvoiceId");
                table.ForeignKey(
                    name: "FK_InvoicedOrders_UPSInvoices_UPSInvoiceId",
                    column: x => x.UPSInvoiceId,
                    principalTable: "UPSInvoices",
                    principalColumn: "UPSInvoiceId");
            });

        migrationBuilder.CreateTable(
            name: "OrderAdvancedOptions",
            columns: table => new
            {
                OrderAdvancedOptionsId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ERPOrderId = table.Column<int>(type: "int", nullable: false),
                warehouseId = table.Column<long>(type: "bigint", nullable: true),
                nonMachinable = table.Column<bool>(type: "bit", nullable: false),
                saturdayDelivery = table.Column<bool>(type: "bit", nullable: false),
                containsAlcohol = table.Column<bool>(type: "bit", nullable: false),
                storeId = table.Column<long>(type: "bigint", nullable: false),
                storeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                customField1 = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                customField2 = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                customField3 = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                source = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                mergedOrSplit = table.Column<bool>(type: "bit", nullable: false),
                parentId = table.Column<long>(type: "bigint", nullable: true),
                billToParty = table.Column<int>(type: "int", nullable: true),
                billToAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                billToPostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                billToCountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                billToMyOtherAccount = table.Column<long>(type: "bigint", nullable: true),
                labelMessageReference1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                labelMessageReference2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                labelMessageReference3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderAdvancedOptions", x => x.OrderAdvancedOptionsId);
                table.ForeignKey(
                    name: "FK_OrderAdvancedOptions_Orders_ERPOrderId",
                    column: x => x.ERPOrderId,
                    principalTable: "Orders",
                    principalColumn: "ERPOrderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderFulfillments",
            columns: table => new
            {
                OrderFulfillmentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ERPOrderId = table.Column<int>(type: "int", nullable: false),
                orderId = table.Column<int>(type: "int", nullable: false),
                orderNumber = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                userId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                customerEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                trackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                createDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                shipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                voidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                deliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                carrierCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                fulfillmentProviderCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                fulfillmentServiceCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                fulfillmentFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                voidRequested = table.Column<bool>(type: "bit", nullable: true),
                voided = table.Column<bool>(type: "bit", nullable: false),
                marketplaceNotified = table.Column<bool>(type: "bit", nullable: true),
                notifyErrorMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                shipToId = table.Column<int>(type: "int", nullable: true),
                sellerFillProviderId = table.Column<int>(type: "int", nullable: true),
                sellerFillProviderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                ERPTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                ERPModifyByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ERPModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderFulfillments", x => x.OrderFulfillmentId);
                table.ForeignKey(
                    name: "FK_OrderFulfillments_OrderShippingInfo_shipToId",
                    column: x => x.shipToId,
                    principalTable: "OrderShippingInfo",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_OrderFulfillments_Orders_ERPOrderId",
                    column: x => x.ERPOrderId,
                    principalTable: "Orders",
                    principalColumn: "ERPOrderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderSourceMapping",
            columns: table => new
            {
                OrderSourceId = table.Column<int>(type: "int", nullable: false),
                ERPOrderId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderSourceMapping", x => new { x.OrderSourceId, x.ERPOrderId });
                table.ForeignKey(
                    name: "FK_OrderSourceMapping_OrderSource_OrderSourceId",
                    column: x => x.OrderSourceId,
                    principalTable: "OrderSource",
                    principalColumn: "OrderSourceId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderSourceMapping_Orders_ERPOrderId",
                    column: x => x.ERPOrderId,
                    principalTable: "Orders",
                    principalColumn: "ERPOrderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderTagMapping",
            columns: table => new
            {
                OrderTagId = table.Column<int>(type: "int", nullable: false),
                ERPOrderId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderTagMapping", x => new { x.OrderTagId, x.ERPOrderId });
                table.ForeignKey(
                    name: "FK_OrderTagMapping_OrderTags_OrderTagId",
                    column: x => x.OrderTagId,
                    principalTable: "OrderTags",
                    principalColumn: "OrderTagId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderTagMapping_Orders_ERPOrderId",
                    column: x => x.ERPOrderId,
                    principalTable: "Orders",
                    principalColumn: "ERPOrderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PurchaseOrder",
            columns: table => new
            {
                PurchaseOrderId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ShippingMethodId = table.Column<int>(type: "int", nullable: false),
                ShippingProviderId = table.Column<int>(type: "int", nullable: false),
                VendorId = table.Column<int>(type: "int", nullable: false),
                PurchaseOrderNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EstimatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                POStatus = table.Column<int>(type: "int", nullable: false),
                ReferenceNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ShippingCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                GrandTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                Discount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                ShippingTax = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                OtherCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PurchaseOrder", x => x.PurchaseOrderId);
                table.ForeignKey(
                    name: "FK_PurchaseOrder_ShippingMethod_ShippingMethodId",
                    column: x => x.ShippingMethodId,
                    principalTable: "ShippingMethod",
                    principalColumn: "ShippingMethodId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PurchaseOrder_ShippingProvider_ShippingProviderId",
                    column: x => x.ShippingProviderId,
                    principalTable: "ShippingProvider",
                    principalColumn: "ShippingProviderId",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_PurchaseOrder_Vendor_VendorId",
                    column: x => x.VendorId,
                    principalTable: "Vendor",
                    principalColumn: "VendorId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "NirfInventory",
            columns: table => new
            {
                NirfInventoryId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                MembraneSiteId = table.Column<int>(type: "int", nullable: false),
                MainSiteId = table.Column<int>(type: "int", nullable: false),
                MembraneLocationId = table.Column<int>(type: "int", nullable: false),
                MainLocationId = table.Column<int>(type: "int", nullable: false),
                AltMainLocationId = table.Column<int>(type: "int", nullable: false),
                AltMembraneLocationId = table.Column<int>(type: "int", nullable: false),
                NirfFormId = table.Column<int>(type: "int", nullable: false),
                Comments = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                SignedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                SignedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                AspUserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfInventory", x => x.NirfInventoryId);
                table.ForeignKey(
                    name: "FK_NirfInventory_Location_AltMainLocationId",
                    column: x => x.AltMainLocationId,
                    principalTable: "Location",
                    principalColumn: "LocationId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_NirfInventory_Location_AltMembraneLocationId",
                    column: x => x.AltMembraneLocationId,
                    principalTable: "Location",
                    principalColumn: "LocationId",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_NirfInventory_Location_MainLocationId",
                    column: x => x.MainLocationId,
                    principalTable: "Location",
                    principalColumn: "LocationId",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_NirfInventory_Location_MembraneLocationId",
                    column: x => x.MembraneLocationId,
                    principalTable: "Location",
                    principalColumn: "LocationId",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_NirfInventory_NirfForm_NirfFormId",
                    column: x => x.NirfFormId,
                    principalTable: "NirfForm",
                    principalColumn: "NirfFormId",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateTable(
            name: "BundleItem",
            columns: table => new
            {
                BundleItemId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BundleId = table.Column<int>(type: "int", nullable: false),
                ProductId = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BundleItem", x => x.BundleItemId);
                table.ForeignKey(
                    name: "FK_BundleItem_Bundle_BundleId",
                    column: x => x.BundleId,
                    principalTable: "Bundle",
                    principalColumn: "BundleId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BundleItem_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DepartmentProduct",
            columns: table => new
            {
                DepartmentsDepartmentId = table.Column<int>(type: "int", nullable: false),
                ProductsProductId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DepartmentProduct", x => new { x.DepartmentsDepartmentId, x.ProductsProductId });
                table.ForeignKey(
                    name: "FK_DepartmentProduct_Department_DepartmentsDepartmentId",
                    column: x => x.DepartmentsDepartmentId,
                    principalTable: "Department",
                    principalColumn: "DepartmentId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DepartmentProduct_Product_ProductsProductId",
                    column: x => x.ProductsProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Files",
            columns: table => new
            {
                FileId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Content = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                FileType = table.Column<int>(type: "int", nullable: false),
                ProductId = table.Column<int>(type: "int", nullable: true),
                IsThumbnail = table.Column<bool>(type: "bit", nullable: false),
                IsDetailed = table.Column<bool>(type: "bit", nullable: false),
                FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Files", x => x.FileId);
                table.ForeignKey(
                    name: "FK_Files_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId");
            });

        migrationBuilder.CreateTable(
            name: "NirfProductMapping",
            columns: table => new
            {
                NirfProductMappingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                NirfFormId = table.Column<int>(type: "int", nullable: false),
                ProductId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfProductMapping", x => x.NirfProductMappingId);
                table.ForeignKey(
                    name: "FK_NirfProductMapping_NirfForm_NirfFormId",
                    column: x => x.NirfFormId,
                    principalTable: "NirfForm",
                    principalColumn: "NirfFormId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_NirfProductMapping_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId");
            });

        migrationBuilder.CreateTable(
            name: "ProductCustomFulFillment",
            columns: table => new
            {
                ProductCustomFulfillmentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductId = table.Column<int>(type: "int", nullable: true),
                BundleId = table.Column<int>(type: "int", nullable: true),
                ShipStationStoreId = table.Column<int>(type: "int", nullable: false),
                CustomFulfillmentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductCustomFulFillment", x => x.ProductCustomFulfillmentId);
                table.ForeignKey(
                    name: "FK_ProductCustomFulFillment_Bundle_BundleId",
                    column: x => x.BundleId,
                    principalTable: "Bundle",
                    principalColumn: "BundleId");
                table.ForeignKey(
                    name: "FK_ProductCustomFulFillment_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId");
                table.ForeignKey(
                    name: "FK_ProductCustomFulFillment_ShipStationStore_ShipStationStoreId",
                    column: x => x.ShipStationStoreId,
                    principalTable: "ShipStationStore",
                    principalColumn: "ShipStationStoreId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "productTag",
            columns: table => new
            {
                ProductId = table.Column<int>(type: "int", nullable: false),
                TagId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_productTag", x => new { x.ProductId, x.TagId });
                table.ForeignKey(
                    name: "FK_productTag_ProductTagsRegistry_TagId",
                    column: x => x.TagId,
                    principalTable: "ProductTagsRegistry",
                    principalColumn: "TagId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_productTag_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductVendorMapping",
            columns: table => new
            {
                ProductVendorMappingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductId = table.Column<int>(type: "int", nullable: false),
                VendorId = table.Column<int>(type: "int", nullable: false),
                isPrimaryVendor = table.Column<bool>(type: "bit", nullable: false),
                Cost = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                LeadTime = table.Column<int>(type: "int", nullable: false),
                VendorSku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                MOQ = table.Column<int>(type: "int", nullable: false),
                OrderMultiples = table.Column<int>(type: "int", nullable: false),
                UnitofMeasure = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                Term = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                IsRawMaterial = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductVendorMapping", x => x.ProductVendorMappingId);
                table.ForeignKey(
                    name: "FK_ProductVendorMapping_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductVendorMapping_Vendor_VendorId",
                    column: x => x.VendorId,
                    principalTable: "Vendor",
                    principalColumn: "VendorId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Stock",
            columns: table => new
            {
                StockId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductId = table.Column<int>(type: "int", nullable: false),
                LocationId = table.Column<int>(type: "int", nullable: false),
                TotalAvailable = table.Column<int>(type: "int", nullable: false),
                RecentlyReadded = table.Column<bool>(type: "bit", nullable: false),
                IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastCounted = table.Column<DateTime>(type: "datetime2", nullable: false),
                BeingCounted = table.Column<bool>(type: "bit", nullable: false),
                IsExternal = table.Column<bool>(type: "bit", nullable: false),
                ShipStationStoreId = table.Column<int>(type: "int", nullable: true),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Stock", x => x.StockId);
                table.ForeignKey(
                    name: "FK_Stock_Location_LocationId",
                    column: x => x.LocationId,
                    principalTable: "Location",
                    principalColumn: "LocationId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Stock_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Stock_ShipStationStore_ShipStationStoreId",
                    column: x => x.ShipStationStoreId,
                    principalTable: "ShipStationStore",
                    principalColumn: "ShipStationStoreId");
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "StockHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateTable(
            name: "AccessCard",
            columns: table => new
            {
                AccessCardId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                EmployeeId = table.Column<int>(type: "int", nullable: false),
                Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccessCard", x => x.AccessCardId);
                table.ForeignKey(
                    name: "FK_AccessCard_Employee_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employee",
                    principalColumn: "EmployeeId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "HelpRequestForm",
            columns: table => new
            {
                HelpRequestFormId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Subject = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                RequestedByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RequestingEmployeeId = table.Column<int>(type: "int", nullable: false),
                Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Urgency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                HelperEmployeeId = table.Column<int>(type: "int", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Priority = table.Column<int>(type: "int", nullable: true),
                IsDenied = table.Column<bool>(type: "bit", nullable: false),
                IsComplete = table.Column<bool>(type: "bit", nullable: false),
                CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HelpRequestForm", x => x.HelpRequestFormId);
                table.ForeignKey(
                    name: "FK_HelpRequestForm_Employee_HelperEmployeeId",
                    column: x => x.HelperEmployeeId,
                    principalTable: "Employee",
                    principalColumn: "EmployeeId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_HelpRequestForm_Employee_RequestingEmployeeId",
                    column: x => x.RequestingEmployeeId,
                    principalTable: "Employee",
                    principalColumn: "EmployeeId",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateTable(
            name: "MessageEmployee",
            columns: table => new
            {
                MessageEmployeeId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Message = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                EmployeeId = table.Column<int>(type: "int", nullable: false),
                SentFromEmployee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SentTime = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MessageEmployee", x => x.MessageEmployeeId);
                table.ForeignKey(
                    name: "FK_MessageEmployee_Employee_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employee",
                    principalColumn: "EmployeeId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "QualityControlCapture",
            columns: table => new
            {
                QualityControlCaptureId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                QCStationLocationId = table.Column<int>(type: "int", nullable: true),
                DepartmentId = table.Column<int>(type: "int", nullable: true),
                SkuNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                QCDiagnosisId = table.Column<int>(type: "int", nullable: true),
                EmployeeId = table.Column<int>(type: "int", nullable: true),
                CaptureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                QCPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Quantity = table.Column<int>(type: "int", nullable: false),
                BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QualityControlCapture", x => x.QualityControlCaptureId);
                table.ForeignKey(
                    name: "FK_QualityControlCapture_Department_DepartmentId",
                    column: x => x.DepartmentId,
                    principalTable: "Department",
                    principalColumn: "DepartmentId");
                table.ForeignKey(
                    name: "FK_QualityControlCapture_Employee_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employee",
                    principalColumn: "EmployeeId");
                table.ForeignKey(
                    name: "FK_QualityControlCapture_QCDiagnosis_QCDiagnosisId",
                    column: x => x.QCDiagnosisId,
                    principalTable: "QCDiagnosis",
                    principalColumn: "QCDiagnosisId");
                table.ForeignKey(
                    name: "FK_QualityControlCapture_QCStationLocation_QCStationLocationId",
                    column: x => x.QCStationLocationId,
                    principalTable: "QCStationLocation",
                    principalColumn: "QCStationLocationId");
            });

        migrationBuilder.CreateTable(
            name: "OrderShipments",
            columns: table => new
            {
                OrderShipmentId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                shipmentId = table.Column<long>(type: "bigint", nullable: false),
                ERPOrderId = table.Column<int>(type: "int", nullable: false),
                orderId = table.Column<long>(type: "bigint", nullable: false),
                userId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                orderKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                createDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                shipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                shipmentCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                insuranceCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                trackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                isReturnLabel = table.Column<bool>(type: "bit", nullable: false),
                batchNumber = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                carrierCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                serviceCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                packageCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                confirmation = table.Column<int>(type: "int", nullable: true),
                warehouseId = table.Column<int>(type: "int", nullable: true),
                voided = table.Column<bool>(type: "bit", nullable: false),
                voidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                marketplaceNotified = table.Column<bool>(type: "bit", nullable: false),
                notifyErrorMessage = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                shipFromId = table.Column<int>(type: "int", nullable: true),
                shipToId = table.Column<int>(type: "int", nullable: true),
                advancedOptionsOrderAdvancedOptionsId = table.Column<int>(type: "int", nullable: true),
                labelData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                formData = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                testLabel = table.Column<bool>(type: "bit", nullable: false),
                ShippingAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsExpedited = table.Column<bool>(type: "bit", nullable: false),
                ShipEngineShipmentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ERPTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                ERPModifyByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ERPModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                dimensions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                weight = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderShipments", x => x.OrderShipmentId);
                table.ForeignKey(
                    name: "FK_OrderShipments_OrderAdvancedOptions_advancedOptionsOrderAdvancedOptionsId",
                    column: x => x.advancedOptionsOrderAdvancedOptionsId,
                    principalTable: "OrderAdvancedOptions",
                    principalColumn: "OrderAdvancedOptionsId");
                table.ForeignKey(
                    name: "FK_OrderShipments_OrderShippingInfo_shipFromId",
                    column: x => x.shipFromId,
                    principalTable: "OrderShippingInfo",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_OrderShipments_OrderShippingInfo_shipToId",
                    column: x => x.shipToId,
                    principalTable: "OrderShippingInfo",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_OrderShipments_Orders_ERPOrderId",
                    column: x => x.ERPOrderId,
                    principalTable: "Orders",
                    principalColumn: "ERPOrderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MiscProdcut",
            columns: table => new
            {
                MiscProductId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                Sku = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                ProductCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CustomCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                DiscountPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ExpectedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MiscProdcut", x => x.MiscProductId);
                table.ForeignKey(
                    name: "FK_MiscProdcut_PurchaseOrder_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalTable: "PurchaseOrder",
                    principalColumn: "PurchaseOrderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderBatch",
            columns: table => new
            {
                OrderBatchId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreateBy = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<int>(type: "int", nullable: true),
                IsDeductible = table.Column<bool>(type: "bit", nullable: false),
                PurchaseOrderId = table.Column<int>(type: "int", nullable: true),
                RequiresPO = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderBatch", x => x.OrderBatchId);
                table.ForeignKey(
                    name: "FK_OrderBatch_PurchaseOrder_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalTable: "PurchaseOrder",
                    principalColumn: "PurchaseOrderId");
            });

        migrationBuilder.CreateTable(
            name: "NirfImageMapping",
            columns: table => new
            {
                NirfImageMappingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FileId = table.Column<int>(type: "int", nullable: false),
                NirfFormId = table.Column<int>(type: "int", nullable: false),
                IsThumbnail = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NirfImageMapping", x => x.NirfImageMappingId);
                table.ForeignKey(
                    name: "FK_NirfImageMapping_Files_FileId",
                    column: x => x.FileId,
                    principalTable: "Files",
                    principalColumn: "FileId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_NirfImageMapping_NirfForm_NirfFormId",
                    column: x => x.NirfFormId,
                    principalTable: "NirfForm",
                    principalColumn: "NirfFormId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductFilesMappings",
            columns: table => new
            {
                ProductFilesMappingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductId = table.Column<int>(type: "int", nullable: false),
                FileId = table.Column<int>(type: "int", nullable: false),
                IsDetailedImage = table.Column<bool>(type: "bit", nullable: false),
                IsThumbnail = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductFilesMappings", x => x.ProductFilesMappingId);
                table.ForeignKey(
                    name: "FK_ProductFilesMappings_Files_FileId",
                    column: x => x.FileId,
                    principalTable: "Files",
                    principalColumn: "FileId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductFilesMappings_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductImage",
            columns: table => new
            {
                ProductImageId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductId = table.Column<int>(type: "int", maxLength: 255, nullable: false),
                FileId = table.Column<int>(type: "int", nullable: false),
                FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsDefault = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductImage", x => x.ProductImageId);
                table.ForeignKey(
                    name: "FK_ProductImage_Files_FileId",
                    column: x => x.FileId,
                    principalTable: "Files",
                    principalColumn: "FileId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductImage_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PurchaseOrderFilesMapping",
            columns: table => new
            {
                PurchaseOrderFilesMappingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                FileId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PurchaseOrderFilesMapping", x => x.PurchaseOrderFilesMappingId);
                table.ForeignKey(
                    name: "FK_PurchaseOrderFilesMapping_Files_FileId",
                    column: x => x.FileId,
                    principalTable: "Files",
                    principalColumn: "FileId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PurchaseOrderFilesMapping_PurchaseOrder_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalTable: "PurchaseOrder",
                    principalColumn: "PurchaseOrderId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ShipStationStoreFiles",
            columns: table => new
            {
                StoreFileId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ShipStationStoreId = table.Column<int>(type: "int", nullable: false),
                FileId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShipStationStoreFiles", x => x.StoreFileId);
                table.ForeignKey(
                    name: "FK_ShipStationStoreFiles_Files_FileId",
                    column: x => x.FileId,
                    principalTable: "Files",
                    principalColumn: "FileId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ShipStationStoreFiles_ShipStationStore_ShipStationStoreId",
                    column: x => x.ShipStationStoreId,
                    principalTable: "ShipStationStore",
                    principalColumn: "ShipStationStoreId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserImage",
            columns: table => new
            {
                UserImageId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                FileId = table.Column<int>(type: "int", nullable: false),
                FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserImage", x => x.UserImageId);
                table.ForeignKey(
                    name: "FK_UserImage_Files_FileId",
                    column: x => x.FileId,
                    principalTable: "Files",
                    principalColumn: "FileId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductContainer",
            columns: table => new
            {
                ContainerId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductVendorMappingId = table.Column<int>(type: "int", nullable: false),
                ContainerQuantity = table.Column<int>(type: "int", nullable: false),
                Length = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Width = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Height = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                ContainerDiminsions = table.Column<int>(type: "int", nullable: false),
                ContainerCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductContainer", x => x.ContainerId);
                table.ForeignKey(
                    name: "FK_ProductContainer_ProductVendorMapping_ProductVendorMappingId",
                    column: x => x.ProductVendorMappingId,
                    principalTable: "ProductVendorMapping",
                    principalColumn: "ProductVendorMappingId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductPurchaseOrder",
            columns: table => new
            {
                ProductPurchaseOrderId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                ProductVendorMappingId = table.Column<int>(type: "int", nullable: false),
                CustomCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                AverageCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                TotalRecieved = table.Column<int>(type: "int", nullable: false),
                TotalOrdered = table.Column<int>(type: "int", nullable: false),
                DiscountPercentage = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                TotalProductCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifyByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ExpectedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductPurchaseOrder", x => x.ProductPurchaseOrderId);
                table.ForeignKey(
                    name: "FK_ProductPurchaseOrder_ProductVendorMapping_ProductVendorMappingId",
                    column: x => x.ProductVendorMappingId,
                    principalTable: "ProductVendorMapping",
                    principalColumn: "ProductVendorMappingId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductPurchaseOrder_PurchaseOrder_PurchaseOrderId",
                    column: x => x.PurchaseOrderId,
                    principalTable: "PurchaseOrder",
                    principalColumn: "PurchaseOrderId",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateTable(
            name: "CycleCount",
            columns: table => new
            {
                CycleCountId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                StockId = table.Column<int>(type: "int", nullable: false),
                EnteredSku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                EnteredQuantity = table.Column<int>(type: "int", nullable: true),
                ExpectedQuantity = table.Column<int>(type: "int", nullable: false),
                EnteredById = table.Column<int>(type: "int", nullable: true),
                EnteredOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                VerifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                VerifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                Finished = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CycleCount", x => x.CycleCountId);
                table.ForeignKey(
                    name: "FK_CycleCount_Employee_EnteredById",
                    column: x => x.EnteredById,
                    principalTable: "Employee",
                    principalColumn: "EmployeeId");
                table.ForeignKey(
                    name: "FK_CycleCount_Stock_StockId",
                    column: x => x.StockId,
                    principalTable: "Stock",
                    principalColumn: "StockId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "InventoryRequestForm",
            columns: table => new
            {
                InventoryRequestFormId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductId = table.Column<int>(type: "int", nullable: false),
                QuantityNeeded = table.Column<int>(type: "int", nullable: false),
                RequestedByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RequestedByEmployeeId = table.Column<int>(type: "int", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                PickReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ReasonExplanation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                ToLocationId = table.Column<int>(type: "int", nullable: false),
                IsPicked = table.Column<bool>(type: "bit", nullable: false),
                PickedByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PickedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                PickedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsFromExtrasLocation = table.Column<bool>(type: "bit", nullable: false),
                StockId = table.Column<int>(type: "int", nullable: true),
                FromLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsReceived = table.Column<bool>(type: "bit", nullable: false),
                ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InventoryRequestForm", x => x.InventoryRequestFormId);
                table.ForeignKey(
                    name: "FK_InventoryRequestForm_Employee_PickedByEmployeeId",
                    column: x => x.PickedByEmployeeId,
                    principalTable: "Employee",
                    principalColumn: "EmployeeId");
                table.ForeignKey(
                    name: "FK_InventoryRequestForm_Employee_RequestedByEmployeeId",
                    column: x => x.RequestedByEmployeeId,
                    principalTable: "Employee",
                    principalColumn: "EmployeeId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_InventoryRequestForm_Location_ToLocationId",
                    column: x => x.ToLocationId,
                    principalTable: "Location",
                    principalColumn: "LocationId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_InventoryRequestForm_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_InventoryRequestForm_Stock_StockId",
                    column: x => x.StockId,
                    principalTable: "Stock",
                    principalColumn: "StockId");
            });

        migrationBuilder.CreateTable(
            name: "MoveStockHistory",
            columns: table => new
            {
                MoveStockHistoryId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Sku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ToStockId = table.Column<int>(type: "int", nullable: true),
                FromStockId = table.Column<int>(type: "int", nullable: true),
                EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Quantity = table.Column<int>(type: "int", nullable: false),
                DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MoveStockHistory", x => x.MoveStockHistoryId);
                table.ForeignKey(
                    name: "FK_MoveStockHistory_Stock_FromStockId",
                    column: x => x.FromStockId,
                    principalTable: "Stock",
                    principalColumn: "StockId");
                table.ForeignKey(
                    name: "FK_MoveStockHistory_Stock_ToStockId",
                    column: x => x.ToStockId,
                    principalTable: "Stock",
                    principalColumn: "StockId");
            });

        migrationBuilder.CreateTable(
            name: "AccessPlanUser",
            columns: table => new
            {
                AccessPlanUserId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccessCardId = table.Column<int>(type: "int", nullable: false),
                AccessPlanId = table.Column<int>(type: "int", nullable: false),
                CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccessPlanUser", x => x.AccessPlanUserId);
                table.ForeignKey(
                    name: "FK_AccessPlanUser_AccessCard_AccessCardId",
                    column: x => x.AccessCardId,
                    principalTable: "AccessCard",
                    principalColumn: "AccessCardId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AccessPlanUser_AccessPlan_AccessPlanId",
                    column: x => x.AccessPlanId,
                    principalTable: "AccessPlan",
                    principalColumn: "AccessPlanId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AccessPointLog",
            columns: table => new
            {
                AccessPointLogId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccessCardId = table.Column<int>(type: "int", nullable: false),
                AccessPointId = table.Column<int>(type: "int", nullable: false),
                RecievedUID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RecievedKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RecievedPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RecievedMacAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RecievedIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsSuccess = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccessPointLog", x => x.AccessPointLogId);
                table.ForeignKey(
                    name: "FK_AccessPointLog_AccessCard_AccessCardId",
                    column: x => x.AccessCardId,
                    principalTable: "AccessCard",
                    principalColumn: "AccessCardId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AccessPointLog_AccessPoint_AccessPointId",
                    column: x => x.AccessPointId,
                    principalTable: "AccessPoint",
                    principalColumn: "AccessPointId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderItem",
            columns: table => new
            {
                ERPOrderItemId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ERPOrderId = table.Column<int>(type: "int", nullable: false),
                OrderShipmentId = table.Column<int>(type: "int", nullable: true),
                orderItemId = table.Column<long>(type: "bigint", nullable: false),
                lineItemKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                sku = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                imageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                quantity = table.Column<int>(type: "int", nullable: false),
                unitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                taxAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                shippingAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                warehouseLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                productId = table.Column<long>(type: "bigint", nullable: true),
                ERPProductId = table.Column<int>(type: "int", nullable: true),
                ERPBundleId = table.Column<int>(type: "int", nullable: true),
                fulfillmentSku = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                adjustment = table.Column<bool>(type: "bit", nullable: false),
                upc = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                createDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                modifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ERPTimestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                ERPModifyByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                weight = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderItem", x => x.ERPOrderItemId);
                table.ForeignKey(
                    name: "FK_OrderItem_Bundle_ERPBundleId",
                    column: x => x.ERPBundleId,
                    principalTable: "Bundle",
                    principalColumn: "BundleId");
                table.ForeignKey(
                    name: "FK_OrderItem_OrderShipments_OrderShipmentId",
                    column: x => x.OrderShipmentId,
                    principalTable: "OrderShipments",
                    principalColumn: "OrderShipmentId");
                table.ForeignKey(
                    name: "FK_OrderItem_Orders_ERPOrderId",
                    column: x => x.ERPOrderId,
                    principalTable: "Orders",
                    principalColumn: "ERPOrderId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderItem_Product_ERPProductId",
                    column: x => x.ERPProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId");
            });

        migrationBuilder.CreateTable(
            name: "ShippingScanout",
            columns: table => new
            {
                ShippingScanoutId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrderShipmentId = table.Column<int>(type: "int", nullable: true),
                OrderFulfillmentId = table.Column<int>(type: "int", nullable: true),
                ScannedTrackingNumber = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                IsValidTrackingNumber = table.Column<bool>(type: "bit", nullable: false),
                TrailerNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                WebhookBatchId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShippingScanout", x => x.ShippingScanoutId);
                table.ForeignKey(
                    name: "FK_ShippingScanout_OrderFulfillments_OrderFulfillmentId",
                    column: x => x.OrderFulfillmentId,
                    principalTable: "OrderFulfillments",
                    principalColumn: "OrderFulfillmentId");
                table.ForeignKey(
                    name: "FK_ShippingScanout_OrderShipments_OrderShipmentId",
                    column: x => x.OrderShipmentId,
                    principalTable: "OrderShipments",
                    principalColumn: "OrderShipmentId");
                table.ForeignKey(
                    name: "FK_ShippingScanout_WebHookBatch_WebhookBatchId",
                    column: x => x.WebhookBatchId,
                    principalTable: "WebHookBatch",
                    principalColumn: "WebHookBatchId");
            });

        migrationBuilder.CreateTable(
            name: "ProductPurchaseOrderStockMapping",
            columns: table => new
            {
                ProductPurchaseOrderStockMappingId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductPurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                StockId = table.Column<int>(type: "int", nullable: false),
                QtyRecieved = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductPurchaseOrderStockMapping", x => x.ProductPurchaseOrderStockMappingId);
                table.ForeignKey(
                    name: "FK_ProductPurchaseOrderStockMapping_ProductPurchaseOrder_ProductPurchaseOrderId",
                    column: x => x.ProductPurchaseOrderId,
                    principalTable: "ProductPurchaseOrder",
                    principalColumn: "ProductPurchaseOrderId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductPurchaseOrderStockMapping_Stock_StockId",
                    column: x => x.StockId,
                    principalTable: "Stock",
                    principalColumn: "StockId",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateTable(
            name: "OrderBatchItem",
            columns: table => new
            {
                OrderBatchItemId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrderBatchId = table.Column<int>(type: "int", nullable: false),
                ERPOrderId = table.Column<int>(type: "int", nullable: false),
                OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ERPOrderItemId = table.Column<int>(type: "int", nullable: false),
                ProductId = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                BatchItemStatusId = table.Column<int>(type: "int", nullable: false),
                IsPicked = table.Column<bool>(type: "bit", nullable: false),
                IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                    .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderBatchItem", x => x.OrderBatchItemId);
                table.ForeignKey(
                    name: "FK_OrderBatchItem_BatchItemStatus_BatchItemStatusId",
                    column: x => x.BatchItemStatusId,
                    principalTable: "BatchItemStatus",
                    principalColumn: "BatchItemStatusId");
                table.ForeignKey(
                    name: "FK_OrderBatchItem_OrderBatch_OrderBatchId",
                    column: x => x.OrderBatchId,
                    principalTable: "OrderBatch",
                    principalColumn: "OrderBatchId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderBatchItem_OrderItem_ERPOrderItemId",
                    column: x => x.ERPOrderItemId,
                    principalTable: "OrderItem",
                    principalColumn: "ERPOrderItemId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderBatchItem_Orders_ERPOrderId",
                    column: x => x.ERPOrderId,
                    principalTable: "Orders",
                    principalColumn: "ERPOrderId",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_OrderBatchItem_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "ProductId",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "OrderBatchItemHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.CreateIndex(
            name: "IX_AccessCard_EmployeeId",
            table: "AccessCard",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_AccessPlanDoor_AccessPlanId",
            table: "AccessPlanDoor",
            column: "AccessPlanId");

        migrationBuilder.CreateIndex(
            name: "IX_AccessPlanDoor_AccessPointId",
            table: "AccessPlanDoor",
            column: "AccessPointId");

        migrationBuilder.CreateIndex(
            name: "IX_AccessPlanUser_AccessCardId",
            table: "AccessPlanUser",
            column: "AccessCardId");

        migrationBuilder.CreateIndex(
            name: "IX_AccessPlanUser_AccessPlanId",
            table: "AccessPlanUser",
            column: "AccessPlanId");

        migrationBuilder.CreateIndex(
            name: "IX_AccessPointLog_AccessCardId",
            table: "AccessPointLog",
            column: "AccessCardId");

        migrationBuilder.CreateIndex(
            name: "IX_AccessPointLog_AccessPointId",
            table: "AccessPointLog",
            column: "AccessPointId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true,
            filter: "[NormalizedName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true,
            filter: "[NormalizedUserName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_BatchItemStatus_DepartmentId",
            table: "BatchItemStatus",
            column: "DepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_Bundle_BundleName",
            table: "Bundle",
            column: "BundleName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BundleItem_BundleId_ProductId",
            table: "BundleItem",
            columns: new[] { "BundleId", "ProductId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BundleItem_ProductId",
            table: "BundleItem",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_CycleCount_EnteredById",
            table: "CycleCount",
            column: "EnteredById");

        migrationBuilder.CreateIndex(
            name: "IX_CycleCount_StockId",
            table: "CycleCount",
            column: "StockId");

        migrationBuilder.CreateIndex(
            name: "IX_CycleCountFrequency_SiteId",
            table: "CycleCountFrequency",
            column: "SiteId");

        migrationBuilder.CreateIndex(
            name: "IX_Department_DepartmentName",
            table: "Department",
            column: "DepartmentName",
            unique: true,
            filter: "[DepartmentName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_DepartmentProduct_ProductsProductId",
            table: "DepartmentProduct",
            column: "ProductsProductId");

        migrationBuilder.CreateIndex(
            name: "IX_DepartmentRoleMapping_DepartmentId",
            table: "DepartmentRoleMapping",
            column: "DepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_DepartmentRoleMapping_UserRoleId",
            table: "DepartmentRoleMapping",
            column: "UserRoleId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailAlerts_AlertTemplateId",
            table: "EmailAlerts",
            column: "AlertTemplateId");

        migrationBuilder.CreateIndex(
            name: "IX_Employee_DepartmentId",
            table: "Employee",
            column: "DepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_Employee_FullName",
            table: "Employee",
            column: "FullName",
            unique: true,
            filter: "[FullName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Employee_UserRolesViewModelId",
            table: "Employee",
            column: "UserRolesViewModelId");

        migrationBuilder.CreateIndex(
            name: "IX_Files_ProductId",
            table: "Files",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_HelpRequestForm_HelperEmployeeId",
            table: "HelpRequestForm",
            column: "HelperEmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_HelpRequestForm_RequestingEmployeeId",
            table: "HelpRequestForm",
            column: "RequestingEmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryRequestForm_PickedByEmployeeId",
            table: "InventoryRequestForm",
            column: "PickedByEmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryRequestForm_ProductId",
            table: "InventoryRequestForm",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryRequestForm_RequestedByEmployeeId",
            table: "InventoryRequestForm",
            column: "RequestedByEmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryRequestForm_StockId",
            table: "InventoryRequestForm",
            column: "StockId");

        migrationBuilder.CreateIndex(
            name: "IX_InventoryRequestForm_ToLocationId",
            table: "InventoryRequestForm",
            column: "ToLocationId");

        migrationBuilder.CreateIndex(
            name: "IX_InvoicedOrders_ERPOrderId",
            table: "InvoicedOrders",
            column: "ERPOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_InvoicedOrders_DHLInvoiceId",
            table: "InvoicedOrders",
            column: "DHLInvoiceId",
            unique: true,
            filter: "[DHLInvoiceId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_InvoicedOrders_EasyPostInvoiceId",
            table: "InvoicedOrders",
            column: "EasyPostInvoiceId",
            unique: true,
            filter: "[EasyPostInvoiceId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_InvoicedOrders_StampsUSPSInvoiceId",
            table: "InvoicedOrders",
            column: "StampsUSPSInvoiceId",
            unique: true,
            filter: "[StampsUSPSInvoiceId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_InvoicedOrders_UPSInvoiceId",
            table: "InvoicedOrders",
            column: "UPSInvoiceId",
            unique: true,
            filter: "[UPSInvoiceId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Location_LocationName",
            table: "Location",
            column: "LocationName",
            unique: true,
            filter: "[LocationName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Location_SiteId",
            table: "Location",
            column: "SiteId");

        migrationBuilder.CreateIndex(
            name: "IX_MessageEmployee_EmployeeId",
            table: "MessageEmployee",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_MiscProdcut_PurchaseOrderId",
            table: "MiscProdcut",
            column: "PurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_MoveStockHistory_FromStockId",
            table: "MoveStockHistory",
            column: "FromStockId");

        migrationBuilder.CreateIndex(
            name: "IX_MoveStockHistory_ToStockId",
            table: "MoveStockHistory",
            column: "ToStockId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfForecasting_NirfFormId",
            table: "NirfForecasting",
            column: "NirfFormId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfImageMapping_FileId",
            table: "NirfImageMapping",
            column: "FileId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfImageMapping_NirfFormId",
            table: "NirfImageMapping",
            column: "NirfFormId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfInventory_AltMainLocationId",
            table: "NirfInventory",
            column: "AltMainLocationId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfInventory_AltMembraneLocationId",
            table: "NirfInventory",
            column: "AltMembraneLocationId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfInventory_MainLocationId",
            table: "NirfInventory",
            column: "MainLocationId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfInventory_MembraneLocationId",
            table: "NirfInventory",
            column: "MembraneLocationId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfInventory_NirfFormId",
            table: "NirfInventory",
            column: "NirfFormId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfPackaging_NirfFormId",
            table: "NirfPackaging",
            column: "NirfFormId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfParameters_FontId",
            table: "NirfParameters",
            column: "FontId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfParameters_NirfFormId",
            table: "NirfParameters",
            column: "NirfFormId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfProductMapping_NirfFormId",
            table: "NirfProductMapping",
            column: "NirfFormId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfProductMapping_ProductId",
            table: "NirfProductMapping",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfShipping_NirfFormId",
            table: "NirfShipping",
            column: "NirfFormId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfShippingProvider_NirfShippingId",
            table: "NirfShippingProvider",
            column: "NirfShippingId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfShippingProvider_ShippingProviderId",
            table: "NirfShippingProvider",
            column: "ShippingProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfVendorMapping_NirfFormId",
            table: "NirfVendorMapping",
            column: "NirfFormId");

        migrationBuilder.CreateIndex(
            name: "IX_NirfVendorMapping_VendorId",
            table: "NirfVendorMapping",
            column: "VendorId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderAdvancedOptions_ERPOrderId",
            table: "OrderAdvancedOptions",
            column: "ERPOrderId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OrderBatch_PurchaseOrderId",
            table: "OrderBatch",
            column: "PurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderBatchItem_BatchItemStatusId",
            table: "OrderBatchItem",
            column: "BatchItemStatusId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderBatchItem_ERPOrderId",
            table: "OrderBatchItem",
            column: "ERPOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderBatchItem_ERPOrderItemId",
            table: "OrderBatchItem",
            column: "ERPOrderItemId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderBatchItem_OrderBatchId",
            table: "OrderBatchItem",
            column: "OrderBatchId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderBatchItem_ProductId",
            table: "OrderBatchItem",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderFulfillments_ERPOrderId",
            table: "OrderFulfillments",
            column: "ERPOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderFulfillments_shipToId",
            table: "OrderFulfillments",
            column: "shipToId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItem_ERPBundleId",
            table: "OrderItem",
            column: "ERPBundleId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItem_ERPOrderId",
            table: "OrderItem",
            column: "ERPOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItem_ERPProductId",
            table: "OrderItem",
            column: "ERPProductId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItem_orderItemId",
            table: "OrderItem",
            column: "orderItemId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderItem_OrderShipmentId",
            table: "OrderItem",
            column: "OrderShipmentId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_billToId",
            table: "Orders",
            column: "billToId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_orderKey",
            table: "Orders",
            column: "orderKey");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_ParentERPOrderId",
            table: "Orders",
            column: "ParentERPOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_shipFromId",
            table: "Orders",
            column: "shipFromId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_shipToId",
            table: "Orders",
            column: "shipToId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderShipments_advancedOptionsOrderAdvancedOptionsId",
            table: "OrderShipments",
            column: "advancedOptionsOrderAdvancedOptionsId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderShipments_ERPOrderId",
            table: "OrderShipments",
            column: "ERPOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderShipments_shipFromId",
            table: "OrderShipments",
            column: "shipFromId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderShipments_shipToId",
            table: "OrderShipments",
            column: "shipToId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderSourceMapping_ERPOrderId",
            table: "OrderSourceMapping",
            column: "ERPOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderTagMapping_ERPOrderId",
            table: "OrderTagMapping",
            column: "ERPOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_Product_AlternateProductId",
            table: "Product",
            column: "AlternateProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Product_Sku",
            table: "Product",
            column: "Sku",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Product_SubCategoryId",
            table: "Product",
            column: "SubCategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductContainer_ProductVendorMappingId",
            table: "ProductContainer",
            column: "ProductVendorMappingId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductCustomFulFillment_BundleId",
            table: "ProductCustomFulFillment",
            column: "BundleId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductCustomFulFillment_ProductId",
            table: "ProductCustomFulFillment",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductCustomFulFillment_ShipStationStoreId",
            table: "ProductCustomFulFillment",
            column: "ShipStationStoreId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductFilesMappings_FileId",
            table: "ProductFilesMappings",
            column: "FileId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductFilesMappings_ProductId",
            table: "ProductFilesMappings",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductImage_FileId",
            table: "ProductImage",
            column: "FileId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductImage_ProductId",
            table: "ProductImage",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductPurchaseOrder_ProductVendorMappingId",
            table: "ProductPurchaseOrder",
            column: "ProductVendorMappingId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductPurchaseOrder_PurchaseOrderId",
            table: "ProductPurchaseOrder",
            column: "PurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductPurchaseOrderStockMapping_ProductPurchaseOrderId",
            table: "ProductPurchaseOrderStockMapping",
            column: "ProductPurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductPurchaseOrderStockMapping_StockId",
            table: "ProductPurchaseOrderStockMapping",
            column: "StockId");

        migrationBuilder.CreateIndex(
            name: "IX_productTag_TagId",
            table: "productTag",
            column: "TagId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductTagsRegistry_Description",
            table: "ProductTagsRegistry",
            column: "Description",
            unique: true,
            filter: "[Description] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_ProductVendorMapping_ProductId",
            table: "ProductVendorMapping",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductVendorMapping_VendorId",
            table: "ProductVendorMapping",
            column: "VendorId");

        migrationBuilder.CreateIndex(
            name: "IX_PurchaseOrder_ShippingMethodId",
            table: "PurchaseOrder",
            column: "ShippingMethodId");

        migrationBuilder.CreateIndex(
            name: "IX_PurchaseOrder_ShippingProviderId",
            table: "PurchaseOrder",
            column: "ShippingProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_PurchaseOrder_VendorId",
            table: "PurchaseOrder",
            column: "VendorId");

        migrationBuilder.CreateIndex(
            name: "IX_PurchaseOrderFilesMapping_FileId",
            table: "PurchaseOrderFilesMapping",
            column: "FileId");

        migrationBuilder.CreateIndex(
            name: "IX_PurchaseOrderFilesMapping_PurchaseOrderId",
            table: "PurchaseOrderFilesMapping",
            column: "PurchaseOrderId");

        migrationBuilder.CreateIndex(
            name: "IX_QCStationLocation_DepartmentId",
            table: "QCStationLocation",
            column: "DepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_QualityControlCapture_DepartmentId",
            table: "QualityControlCapture",
            column: "DepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_QualityControlCapture_EmployeeId",
            table: "QualityControlCapture",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_QualityControlCapture_QCDiagnosisId",
            table: "QualityControlCapture",
            column: "QCDiagnosisId");

        migrationBuilder.CreateIndex(
            name: "IX_QualityControlCapture_QCStationLocationId",
            table: "QualityControlCapture",
            column: "QCStationLocationId");

        migrationBuilder.CreateIndex(
            name: "IX_ShippingManifests_ManifestId",
            table: "ShippingManifests",
            column: "ManifestId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ShippingMethod_ShippingProviderId",
            table: "ShippingMethod",
            column: "ShippingProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_ShippingProvider_ShippingProviderName",
            table: "ShippingProvider",
            column: "ShippingProviderName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ShippingScanout_OrderFulfillmentId",
            table: "ShippingScanout",
            column: "OrderFulfillmentId");

        migrationBuilder.CreateIndex(
            name: "IX_ShippingScanout_OrderShipmentId",
            table: "ShippingScanout",
            column: "OrderShipmentId");

        migrationBuilder.CreateIndex(
            name: "IX_ShippingScanout_ScannedTrackingNumber",
            table: "ShippingScanout",
            column: "ScannedTrackingNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ShippingScanout_WebhookBatchId",
            table: "ShippingScanout",
            column: "WebhookBatchId");

        migrationBuilder.CreateIndex(
            name: "UQ_ShipStationStore_StoreId",
            table: "ShipStationStore",
            column: "StoreId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UQ_ShipStationStore_StoreName",
            table: "ShipStationStore",
            column: "StoreName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ShipStationStoreFiles_FileId",
            table: "ShipStationStoreFiles",
            column: "FileId");

        migrationBuilder.CreateIndex(
            name: "IX_ShipStationStoreFiles_ShipStationStoreId",
            table: "ShipStationStoreFiles",
            column: "ShipStationStoreId");

        migrationBuilder.CreateIndex(
            name: "IX_Site_SiteName",
            table: "Site",
            column: "SiteName",
            unique: true,
            filter: "[SiteName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Stock_LocationId",
            table: "Stock",
            column: "LocationId");

        migrationBuilder.CreateIndex(
            name: "IX_Stock_ProductId",
            table: "Stock",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Stock_ShipStationStoreId",
            table: "Stock",
            column: "ShipStationStoreId");

        migrationBuilder.CreateIndex(
            name: "UQ_SubCategory_Description",
            table: "SubCategory",
            column: "Description",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserEmailAlertMapping_EmailAlertId",
            table: "UserEmailAlertMapping",
            column: "EmailAlertId");

        migrationBuilder.CreateIndex(
            name: "IX_UserEmailAlertMapping_UserId",
            table: "UserEmailAlertMapping",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserImage_FileId",
            table: "UserImage",
            column: "FileId");

        migrationBuilder.CreateIndex(
            name: "IX_UserPreferences_PreferDepartment",
            table: "UserPreferences",
            column: "PreferDepartment");

        migrationBuilder.CreateIndex(
            name: "IX_UserSiteMapping_SiteId",
            table: "UserSiteMapping",
            column: "SiteId");

        migrationBuilder.CreateIndex(
            name: "IX_UserSiteMapping_UserId_SiteId",
            table: "UserSiteMapping",
            columns: new[] { "UserId", "SiteId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Vendor_VendorName",
            table: "Vendor",
            column: "VendorName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Warehouse_BillingAddressId",
            table: "Warehouse",
            column: "BillingAddressId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AccessPlanDoor");

        migrationBuilder.DropTable(
            name: "AccessPlanUser");

        migrationBuilder.DropTable(
            name: "AccessPointLog");

        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "AuditLogs");

        migrationBuilder.DropTable(
            name: "BarcodeScan");

        migrationBuilder.DropTable(
            name: "BundleItem");

        migrationBuilder.DropTable(
            name: "CycleCount");

        migrationBuilder.DropTable(
            name: "CycleCountFrequency");

        migrationBuilder.DropTable(
            name: "DepartmentProduct");

        migrationBuilder.DropTable(
            name: "DepartmentRoleMapping");

        migrationBuilder.DropTable(
            name: "DeputyTimeSheet");

        migrationBuilder.DropTable(
            name: "HelpRequestForm");

        migrationBuilder.DropTable(
            name: "InventoryBalance");

        migrationBuilder.DropTable(
            name: "InventoryRequestForm");

        migrationBuilder.DropTable(
            name: "InvoicedOrders");

        migrationBuilder.DropTable(
            name: "MessageEmployee");

        migrationBuilder.DropTable(
            name: "MiscProdcut");

        migrationBuilder.DropTable(
            name: "MoveStockHistory");

        migrationBuilder.DropTable(
            name: "MyDash");

        migrationBuilder.DropTable(
            name: "NirfForecasting");

        migrationBuilder.DropTable(
            name: "NirfImageMapping");

        migrationBuilder.DropTable(
            name: "NirfInventory");

        migrationBuilder.DropTable(
            name: "NirfPackaging");

        migrationBuilder.DropTable(
            name: "NirfParameters");

        migrationBuilder.DropTable(
            name: "NirfProductMapping");

        migrationBuilder.DropTable(
            name: "NirfShippingProvider");

        migrationBuilder.DropTable(
            name: "NirfVendorMapping");

        migrationBuilder.DropTable(
            name: "OrderBatchItem")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "OrderBatchItemHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "OrderSourceMapping");

        migrationBuilder.DropTable(
            name: "OrderTagMapping");

        migrationBuilder.DropTable(
            name: "ProductContainer");

        migrationBuilder.DropTable(
            name: "ProductCustomFulFillment");

        migrationBuilder.DropTable(
            name: "ProductFilesMappings");

        migrationBuilder.DropTable(
            name: "ProductImage");

        migrationBuilder.DropTable(
            name: "ProductionVsLaborCostHistory");

        migrationBuilder.DropTable(
            name: "ProductionVsLaborCostPrice");

        migrationBuilder.DropTable(
            name: "ProductPurchaseOrderStockMapping");

        migrationBuilder.DropTable(
            name: "productTag");

        migrationBuilder.DropTable(
            name: "PurchaseOrderFilesMapping");

        migrationBuilder.DropTable(
            name: "QualityControlCapture");

        migrationBuilder.DropTable(
            name: "RedoOrder");

        migrationBuilder.DropTable(
            name: "SalesReport");

        migrationBuilder.DropTable(
            name: "SellerMargin");

        migrationBuilder.DropTable(
            name: "SellerMargins");

        migrationBuilder.DropTable(
            name: "ShippingManifests");

        migrationBuilder.DropTable(
            name: "ShippingScanout");

        migrationBuilder.DropTable(
            name: "ShipStationAwaitingOrder");

        migrationBuilder.DropTable(
            name: "ShipStationOrderedHistory");

        migrationBuilder.DropTable(
            name: "ShipStationStoreFiles");

        migrationBuilder.DropTable(
            name: "SkuCategory");

        migrationBuilder.DropTable(
            name: "SkuColor");

        migrationBuilder.DropTable(
            name: "SkulabsImport");

        migrationBuilder.DropTable(
            name: "SkuUnitOfMeasure");

        migrationBuilder.DropTable(
            name: "SpeedOMeterGoal");

        migrationBuilder.DropTable(
            name: "UserEmailAlertMapping");

        migrationBuilder.DropTable(
            name: "UserImage");

        migrationBuilder.DropTable(
            name: "UserPreferences");

        migrationBuilder.DropTable(
            name: "UserSiteMapping");

        migrationBuilder.DropTable(
            name: "Warehouse");

        migrationBuilder.DropTable(
            name: "AccessPlan");

        migrationBuilder.DropTable(
            name: "AccessCard");

        migrationBuilder.DropTable(
            name: "AccessPoint");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "DHLInvoices");

        migrationBuilder.DropTable(
            name: "EasyPostInvoices");

        migrationBuilder.DropTable(
            name: "StampsUSPSInvoices");

        migrationBuilder.DropTable(
            name: "UPSInvoices");

        migrationBuilder.DropTable(
            name: "Fonts");

        migrationBuilder.DropTable(
            name: "NirfShipping");

        migrationBuilder.DropTable(
            name: "BatchItemStatus");

        migrationBuilder.DropTable(
            name: "OrderBatch");

        migrationBuilder.DropTable(
            name: "OrderItem");

        migrationBuilder.DropTable(
            name: "OrderSource");

        migrationBuilder.DropTable(
            name: "OrderTags");

        migrationBuilder.DropTable(
            name: "ProductPurchaseOrder");

        migrationBuilder.DropTable(
            name: "Stock")
            .Annotation("SqlServer:IsTemporal", true)
            .Annotation("SqlServer:TemporalHistoryTableName", "StockHistory")
            .Annotation("SqlServer:TemporalHistoryTableSchema", null)
            .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
            .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

        migrationBuilder.DropTable(
            name: "ProductTagsRegistry");

        migrationBuilder.DropTable(
            name: "QCDiagnosis");

        migrationBuilder.DropTable(
            name: "QCStationLocation");

        migrationBuilder.DropTable(
            name: "OrderFulfillments");

        migrationBuilder.DropTable(
            name: "WebHookBatch");

        migrationBuilder.DropTable(
            name: "EmailAlerts");

        migrationBuilder.DropTable(
            name: "Files");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "Employee");

        migrationBuilder.DropTable(
            name: "NirfForm");

        migrationBuilder.DropTable(
            name: "Bundle");

        migrationBuilder.DropTable(
            name: "OrderShipments");

        migrationBuilder.DropTable(
            name: "ProductVendorMapping");

        migrationBuilder.DropTable(
            name: "PurchaseOrder");

        migrationBuilder.DropTable(
            name: "Location");

        migrationBuilder.DropTable(
            name: "ShipStationStore");

        migrationBuilder.DropTable(
            name: "AlertTriggerTemplateMappings");

        migrationBuilder.DropTable(
            name: "Department");

        migrationBuilder.DropTable(
            name: "UserRolesViewModel");

        migrationBuilder.DropTable(
            name: "OrderAdvancedOptions");

        migrationBuilder.DropTable(
            name: "Product");

        migrationBuilder.DropTable(
            name: "ShippingMethod");

        migrationBuilder.DropTable(
            name: "Vendor");

        migrationBuilder.DropTable(
            name: "Site");

        migrationBuilder.DropTable(
            name: "Orders");

        migrationBuilder.DropTable(
            name: "SubCategory");

        migrationBuilder.DropTable(
            name: "ShippingProvider");

        migrationBuilder.DropTable(
            name: "OrderShippingInfo");
    }
}
