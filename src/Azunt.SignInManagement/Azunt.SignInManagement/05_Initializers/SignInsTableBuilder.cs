using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Azunt.SignInManagement
{
    /// <summary>
    /// 테넌트/마스터 DB에 SignIns 테이블을 생성/보강하고 초기 데이터 시드를 처리합니다.
    /// </summary>
    public class SignInsTableBuilder
    {
        private readonly string _masterConnectionString;
        private readonly ILogger<SignInsTableBuilder> _logger;

        public SignInsTableBuilder(string masterConnectionString, ILogger<SignInsTableBuilder> logger)
        {
            _masterConnectionString = masterConnectionString;
            _logger = logger;
        }

        public void BuildTenantDatabases()
        {
            var tenantConnectionStrings = GetTenantConnectionStrings();

            foreach (var connStr in tenantConnectionStrings)
            {
                var dbName = new SqlConnectionStringBuilder(connStr).InitialCatalog;

                try
                {
                    EnsureSignInsTable(connStr);
                    _logger.LogInformation("SignIns table processed (tenant DB: {Database})", dbName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing tenant DB: {Database}", dbName);
                }
            }
        }

        public void BuildMasterDatabase()
        {
            var dbName = new SqlConnectionStringBuilder(_masterConnectionString).InitialCatalog;

            try
            {
                EnsureSignInsTable(_masterConnectionString);
                _logger.LogInformation("SignIns table processed (master DB: {Database})", dbName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing master DB: {Database}", dbName);
            }
        }

        private List<string> GetTenantConnectionStrings()
        {
            var result = new List<string>();

            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();

                using var cmd = new SqlCommand("SELECT ConnectionString FROM dbo.Tenants", connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var cs = reader["ConnectionString"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(cs))
                    {
                        result.Add(cs);
                    }
                }
            }

            return result;
        }

        private void EnsureSignInsTable(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // 1) 테이블 존재 여부
                using var cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SignIns';", connection);

                var tableCount = (int)cmdCheck.ExecuteScalar();

                // 2) 없으면 생성
                if (tableCount == 0)
                {
                    using var cmdCreate = new SqlCommand(@"
                        CREATE TABLE [dbo].[SignIns] (
                            [Id]               BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [DateTimeSignedIn] DATETIMEOFFSET(0)    NOT NULL,
                            [UserId]           NVARCHAR(450)        NULL,
                            [Email]            NVARCHAR(MAX)        NOT NULL,
                            [FirstName]        NVARCHAR(MAX)        NULL,
                            [LastName]         NVARCHAR(MAX)        NULL,
                            [Result]           NVARCHAR(MAX)        NOT NULL,
                            [IpAddress]        NVARCHAR(MAX)        NULL,
                            [Note]             NVARCHAR(MAX)        NULL,
                            [TenantId]         BIGINT               NULL,
                            [TenantName]       NVARCHAR(255)        NULL
                        );", connection);

                    cmdCreate.ExecuteNonQuery();
                    _logger.LogInformation("SignIns table created.");
                }
                else
                {
                    // 3) 누락 컬럼 보강
                    var expectedColumns = new Dictionary<string, string>
                    {
                        ["DateTimeSignedIn"] = "DATETIMEOFFSET(0) NOT NULL",
                        ["UserId"] = "NVARCHAR(450) NULL",
                        ["Email"] = "NVARCHAR(MAX) NOT NULL",
                        ["FirstName"] = "NVARCHAR(MAX) NULL",
                        ["LastName"] = "NVARCHAR(MAX) NULL",
                        ["Result"] = "NVARCHAR(MAX) NOT NULL",
                        ["IpAddress"] = "NVARCHAR(MAX) NULL",
                        ["Note"] = "NVARCHAR(MAX) NULL",
                        ["TenantId"] = "BIGINT NULL",
                        ["TenantName"] = "NVARCHAR(255) NULL"
                    };

                    foreach (var (columnName, columnSpec) in expectedColumns)
                    {
                        using var cmdCol = new SqlCommand(@"
                            SELECT COUNT(*) 
                            FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_SCHEMA = 'dbo' 
                              AND TABLE_NAME = 'SignIns' 
                              AND COLUMN_NAME = @ColumnName;", connection);

                        cmdCol.Parameters.AddWithValue("@ColumnName", columnName);
                        var exists = (int)cmdCol.ExecuteScalar() > 0;

                        if (!exists)
                        {
                            using var cmdAlter = new SqlCommand(
                                $"ALTER TABLE [dbo].[SignIns] ADD [{columnName}] {columnSpec};", connection);

                            cmdAlter.ExecuteNonQuery();
                            _logger.LogInformation("Added missing column {Column} ({Spec})", columnName, columnSpec);
                        }
                    }
                }

                // 4) 초기 데이터 시드
                using var cmdCount = new SqlCommand("SELECT COUNT(*) FROM [dbo].[SignIns];", connection);
                var rowCount = (int)cmdCount.ExecuteScalar();

                if (rowCount == 0)
                {
                    using var cmdSeed = new SqlCommand(@"
                        INSERT INTO [dbo].[SignIns]
                        (
                            DateTimeSignedIn, UserId, Email, FirstName, LastName, 
                            Result, IpAddress, Note, TenantId, TenantName
                        )
                        VALUES
                        (SYSDATETIMEOFFSET(), 'system-user', 'system@example.com', 'System', 'Admin',
                         'Success', '127.0.0.1', 'Initial record 1', NULL, NULL),
                        (SYSDATETIMEOFFSET(), 'system-user', 'system@example.com', 'System', 'Admin',
                         'Failure', '127.0.0.1', 'Initial record 2', NULL, NULL);", connection);

                    var inserted = cmdSeed.ExecuteNonQuery();
                    _logger.LogInformation("Seeded {Count} SignIns rows.", inserted);
                }
            }
        }

        public static void Run(IServiceProvider services, bool forMaster)
        {
            try
            {
                var logger = services.GetRequiredService<ILogger<SignInsTableBuilder>>();
                var config = services.GetRequiredService<IConfiguration>();
                var masterCs = config.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(masterCs))
                {
                    throw new InvalidOperationException("DefaultConnection is not configured in appsettings.json.");
                }

                var builder = new SignInsTableBuilder(masterCs, logger);

                if (forMaster)
                {
                    builder.BuildMasterDatabase();
                }
                else
                {
                    builder.BuildTenantDatabases();
                }
            }
            catch (Exception ex)
            {
                // 마지막 로거 시도
                var fallback = services.GetService<ILogger<SignInsTableBuilder>>();
                fallback?.LogError(ex, "Error while processing SignIns table.");
            }
        }
    }
}
