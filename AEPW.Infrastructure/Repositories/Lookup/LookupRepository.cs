using AEPW.Application.DTOs.Lookup;
using AEPW.Application.Interfaces.Repositories.Lookup;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.Lookup;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AEPW.Infrastructure.Repositories.Lookup;

public sealed class LookupRepository : ILookupRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public LookupRepository(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IReadOnlyList<AppUserLookupModel>> GetAllAppUsersAsync(
     string? searchText,
     CancellationToken cancellationToken = default)
    {
        var results = new List<AppUserLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllAppUsers,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue(
            "@SearchText",
            string.IsNullOrWhiteSpace(searchText)
                ? DBNull.Value
                : searchText);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new AppUserLookupModel
            {
                UserId = reader["UserId"].ToString()!,
                DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }
    public async Task<IReadOnlyList<CityLookupModel>> GetAllCitiesAsync(
    CancellationToken cancellationToken = default)
    {
        var results = new List<CityLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllCities, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new CityLookupModel
            {
                CityId = reader.GetInt64(reader.GetOrdinal("CityId")),
                CountryName = reader["CountryName"]?.ToString() ?? string.Empty,
                CityName = reader["CityName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }
    public async Task<List<RecordTypeLookupModel>> GetAllRecordTypesAsync()
    {
        var recordTypes = new List<RecordTypeLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync();

        await using var command =
            new SqlCommand(StoredProcedures.Lookup.GetAllRecordTypes, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            recordTypes.Add(new RecordTypeLookupModel
            {
                RecordTypeId = reader.GetInt64(reader.GetOrdinal("RecordTypeId")),
                Name = reader["Name"].ToString()!
            });
        }

        return recordTypes;
    }
    public async Task<IReadOnlyList<ProgramLookupModel>> GetAllProgramsAsync(
    string? searchTerm,
    CancellationToken cancellationToken = default)
    {
        var results = new List<ProgramLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllPrograms,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue(
            "@SearchTerm",
            string.IsNullOrWhiteSpace(searchTerm)
                ? DBNull.Value
                : searchTerm);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ProgramLookupModel
            {
                ProgramId = reader["ProgramId"].ToString()!,
                Code = reader["Code"]?.ToString() ?? string.Empty,
                Name = reader["Name"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }
    public async Task<IReadOnlyList<MetricSourceLookupModel>> GetAllMetricSourcesAsync(
    CancellationToken cancellationToken = default)
    {
        var results = new List<MetricSourceLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        Console.WriteLine($"Connection state: {connection.State}");

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllMetricSources,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MetricSourceLookupModel
            {
                MetricSourceId = reader.GetInt64(reader.GetOrdinal("MetricSourceId")),
                Code = reader["Code"]?.ToString() ?? string.Empty,
                Name = reader["Name"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }


    public async Task<IReadOnlyList<StatusLookupModel>> GetStatusesByModuleAsync(
        string module,
        string? pageKey,
        long? impactMasterId,
        CancellationToken cancellationToken = default)
    {
        var results = new List<StatusLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllStatus, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@Module", module);
        command.Parameters.AddWithValue("@PageKey", pageKey);
        command.Parameters.AddWithValue(
    "@ImpactMasterId",
    (object?)impactMasterId ?? DBNull.Value);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new StatusLookupModel
            {
                StatusId = reader["StatusId"].ToString()!,
                StatusCode = reader["StatusCode"]?.ToString() ?? string.Empty,
                StatusName = reader["StatusName"]?.ToString() ?? string.Empty,
                DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                IsDefault = reader.GetBoolean(reader.GetOrdinal("IsDefault"))
            });
        }

        return results;
    }

    public async Task<IReadOnlyList<MetricCategoryLookupModel>> GetAllMetricCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<MetricCategoryLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllMetricCategories, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MetricCategoryLookupModel
            {
                CategoryId = reader["CategoryId"].ToString()!,
                CategoryCode = reader["CategoryCode"]?.ToString() ?? string.Empty,
                Name = reader["Name"]?.ToString() ?? string.Empty,
                Color = reader["Color"] as string
            });
        }

        return results;
    }

    public async Task<IReadOnlyList<MetricUnitLookupModel>> GetAllMetricUnitsAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<MetricUnitLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllMetricUnits, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MetricUnitLookupModel
            {
                MetricUnitId = reader["MetricUnitId"].ToString()!,
                UnitCode = reader["UnitCode"]?.ToString() ?? string.Empty,
                UnitName = reader["UnitName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }
    public async Task<IReadOnlyList<ParentMetricLookupModel>> GetAllParentMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<ParentMetricLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllParentMetrics, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ParentMetricLookupModel
            {
                MetricId = reader["MetricId"].ToString()!,
                Code = reader["Code"]?.ToString() ?? string.Empty,
                Name = reader["Name"]?.ToString() ?? string.Empty,
                Description = reader["Description"] as string,
                CategoryId = reader["CategoryId"].ToString()!,
            });
        }

        return results;
    }



    public async Task<IReadOnlyList<TimeZoneLookupModel>> GetAllTimeZonesAsync(string? searchTerm,
    CancellationToken cancellationToken = default)
    {
        var results = new List<TimeZoneLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllTimeZones, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add(
            new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(searchTerm)
                    ? DBNull.Value
                    : searchTerm.Trim()
            });
        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new TimeZoneLookupModel
            {
                TimeZoneId = Convert.ToInt64(reader["TimeZoneId"]),
                TimeZoneCode = reader["TimeZoneCode"]?.ToString() ?? string.Empty,
                TimeZoneName = reader["TimeZoneName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }


    public async Task<IReadOnlyList<FundingRecipientLookupModel>> GetAllFundingRecipientsAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<FundingRecipientLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllFundingRecipients, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new FundingRecipientLookupModel
            {
                FundingRecipientId = reader["FundingRecipientId"].ToString()!,
                Name = reader["Name"]?.ToString() ?? string.Empty,
                OrgName = reader["OrgName"]?.ToString() ?? string.Empty,
            });
        }

        return results;
    }
    public async Task<IReadOnlyList<SystemRoleLookupModel>> GetAllSystemRolesAsync(
    CancellationToken cancellationToken = default)
    {
        var results = new List<SystemRoleLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetAllSystemRoles,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new SystemRoleLookupModel
            {
                RoleId = reader["RoleId"].ToString()!,
                Code = reader["Code"]?.ToString() ?? string.Empty,
                RoleName = reader["RoleName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }



    public async Task<IReadOnlyList<ProjectManagerLookupModel>> GetProjectManagersAsync(
     string? searchTerm,
     CancellationToken cancellationToken = default)
    {
        var results = new List<ProjectManagerLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetProjectManagers,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(
      new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 100)
      {
          Value = string.IsNullOrWhiteSpace(searchTerm)
              ? DBNull.Value
              : searchTerm
      });
        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ProjectManagerLookupModel
            {
                UserId = reader["UserId"].ToString()!,
                DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }
    public async Task<IReadOnlyList<BusinessRoleLookupModel>> GetAllBusinessRolesAsync(
   CancellationToken cancellationToken = default)
    {
        var results = new List<BusinessRoleLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetBusinessRoles,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new BusinessRoleLookupModel
            {
                BusinessRoleId = reader["BusinessRoleId"].ToString()!,
                RoleCode = reader["RoleCode"]?.ToString() ?? string.Empty,
                BusinessRoleName = reader["BusinessRoleName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }

    public async Task<IReadOnlyList<BusinessRoleUserLookupModel>> GetBusinessRoleUsersAsync(
        string? roleCode,
          long? impactMasterId,
        CancellationToken cancellationToken = default)
    {
        var results = new List<BusinessRoleUserLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetBusinessRoleUsers,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add("@RoleCode", SqlDbType.VarChar, 24).Value =
     string.IsNullOrWhiteSpace(roleCode)
         ? DBNull.Value
         : roleCode;

        command.Parameters.Add("@ImpactMasterId", SqlDbType.BigInt).Value =
    impactMasterId.HasValue
        ? impactMasterId.Value
        : DBNull.Value;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new BusinessRoleUserLookupModel
            {
                UserId = reader["UserId"].ToString()!,
                DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }
    public async Task<IReadOnlyList<PhaseLookupModel>> GetPhaseLookupAsync(
    CancellationToken cancellationToken = default)
    {
        var results = new List<PhaseLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetPhases,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new PhaseLookupModel
            {
                PhaseId = reader["PhaseId"].ToString()!,
                PhaseName = reader["PhaseName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }
    public async Task<IReadOnlyList<ImpactCategoryLookupModel>> GetImpactCategoryLookupAsync(
    CancellationToken cancellationToken = default)
    {
        var results = new List<ImpactCategoryLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetImpactCategoryLookup,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ImpactCategoryLookupModel
            {
                CategoryId = reader["CategoryId"].ToString()!,
                CategoryName = reader["CategoryName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }
    public async Task<IReadOnlyList<ImpactMasterLookupModel>> GetImpactMasterLookupAsync(
    string? searchTerm,
    CancellationToken cancellationToken = default)
    {
        var results = new List<ImpactMasterLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetImpactMasterLookup,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue(
            "@SearchTerm",
            string.IsNullOrWhiteSpace(searchTerm)
                ? DBNull.Value
                : searchTerm);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new ImpactMasterLookupModel
            {
                ImpactMasterId = reader["ImpactMasterId"].ToString()!,
                Pin = reader["Pin"]?.ToString() ?? string.Empty,
                Title = reader["Title"]?.ToString() ?? string.Empty,
                FundingRecipientName = reader["FundingRecipientName"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }

    public async Task<IReadOnlyList<ReviewStageLookupDto>>
           GetReviewStages(
               CancellationToken cancellationToken = default)
    {
        List<ReviewStageLookupDto> reviewStages = new List<ReviewStageLookupDto>();
        await using var connection =
            await _sqlConnectionFactory
                .CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetReviewStages,
            connection);

        command.CommandType =
            System.Data.CommandType.StoredProcedure;

        DataSet ds = new DataSet();
        await Task.Run(() =>
        {
            using var adapter = new SqlDataAdapter(command);
            adapter.Fill(ds);

            if (ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dataTable = ds.Tables[0];

                foreach (DataRow row in dataTable.Rows)
                {
                    reviewStages.Add(
                        new ReviewStageLookupDto()
                        {
                            ReviewStageId = row.Field<Int64>("ReviewStageId"),
                            Code = row.Field<string>("Code"),
                            Name = row.Field<string>("Name"),
                            Description = row.Field<string>("Description"),
                            //DefaultSequence = row.Field<short>("DefaultSequence"),
                            IsActive = (row.Field<bool>("IsActive") == true) ? 1 : 0,
                            CreatedBy = row.Field<long?>("CreatedBy"),
                            CreatedOnUTC = row.Field<DateTime>("CreatedOnUTC"),
                            UpdatedBy = row.Field<long?>("UpdatedBy"),
                            UpdatedOnUTC = row.Field<DateTime?>("UpdatedOnUTC"),
                            IsApprovalRequired = row.Field<int>("IsApprovalRequired"),
                        }
                    );
                }

            }
        });

        return reviewStages;
    }

    public async Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStagesByUserId(long userId, CancellationToken cancellationToken = default)
    {
        List<ReviewStageLookupDto> reviewStages = new List<ReviewStageLookupDto>();
        await using var connection =
            await _sqlConnectionFactory
                .CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetReviewStagesByUserId,
            connection);

        command.CommandType =
            System.Data.CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@UserId", userId);

        DataSet ds = new DataSet();
        await Task.Run(() =>
        {
            using var adapter = new SqlDataAdapter(command);
            adapter.Fill(ds);

            if (ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dataTable = ds.Tables[0];

                foreach (DataRow row in dataTable.Rows)
                {
                    reviewStages.Add(
                        new ReviewStageLookupDto()
                        {
                            ReviewStageId = row.Field<long?>("ReviewStageId"),
                            Code = row.Field<int>("Code").ToString(),
                            Name = row.Field<string>("Name"),
                            Description = row.Field<string>("Description"),
                            //DefaultSequence = row.Field<short?>("DefaultSequence"),
                            //IsActive =  row.Field<int>("IsActive")   ,
                            //CreatedBy = row.Field<long?>("CreatedBy"),
                            //CreatedOnUTC = row.Field<DateTime?>("CreatedOnUTC"),
                            //UpdatedBy = row.Field<long?>("UpdatedBy"),
                            //UpdatedOnUTC = row.Field<DateTime?>("UpdatedOnUTC"),
                            //IsApprovalRequired = row.Field<int>("IsApprovalRequired"),
                        }
                    );
                }

            }
        });

        return reviewStages;
    }

}
