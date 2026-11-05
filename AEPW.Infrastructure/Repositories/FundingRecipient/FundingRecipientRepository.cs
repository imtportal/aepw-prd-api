using AEPW.Application.DTOs.FundingRecipient;
using AEPW.Application.DTOs.FundingRecipient.Contact;
using AEPW.Application.Interfaces.Repositories.FundingRecipient;
using AEPW.Domain.Constants;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using AEPW.Application.Contracts.Responses;
using System.Data;
using AEPW.Application.Exceptions;
namespace AEPW.Infrastructure.Repositories.FundingRecipient;

using Microsoft.Data.SqlClient;
public sealed class FundingRecipientRepository
    : IFundingRecipientRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public FundingRecipientRepository(
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<PagedResponse<FundingRecipientDto>> GetAllAsync(
        GetFundingRecipientsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.FundingRecipient.GetAll,
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue(
            "@PageNumber",
            request.PageNumber);

        command.Parameters.AddWithValue(
            "@PageSize",
            request.PageSize);

        command.Parameters.AddWithValue(
            "@SearchText",
            (object?)request.SearchText ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "@FundingRecipientId",
            (object?)request.FundingRecipientId ?? DBNull.Value);


        command.Parameters.AddWithValue(
         "@IsActive",
         (object?)request.IsActive ?? DBNull.Value);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var recipients = new List<FundingRecipientDto>();

        int totalRecords = 0;

        while (await reader.ReadAsync(cancellationToken))
        {
            totalRecords =
                reader.GetInt32(reader.GetOrdinal("TotalRecords"));

            recipients.Add(new FundingRecipientDto
            {
                FundingRecipientId = reader.GetInt64(
                    reader.GetOrdinal("FundingRecipientId")),

                Name = reader["Name"]?.ToString() ?? string.Empty,

                OrgName = reader["OrgName"]?.ToString() ?? string.Empty,

                Address = reader["Address"]?.ToString() ?? string.Empty,

                ZipCode = reader["ZipCode"]?.ToString() ?? string.Empty,

                CityName = reader["CityName"]?.ToString() ?? string.Empty,

                CountryName = reader["CountryName"]?.ToString() ?? string.Empty,

                ContactId = reader.IsDBNull(
                    reader.GetOrdinal("ContactId"))
                    ? null
                    : reader.GetInt64(
                        reader.GetOrdinal("ContactId")),

                ContactName = reader["ContactName"]?.ToString(),

                Email = reader["Email"]?.ToString(),

                Phone = reader["Phone"]?.ToString(),

                IsActive = reader.GetBoolean(
                reader.GetOrdinal("IsActive"))
            });
        }
        return new PagedResponse<FundingRecipientDto>
        {
            Items = recipients,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
    public async Task<long> CreateAsync(
    CreateFundingRecipientRequestDto request,
    CancellationToken cancellationToken = default)
    {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(
                    cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.FundingRecipient.Insert,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter("@Name", SqlDbType.NVarChar, 50)
                {
                    Value = request.Name
                });

            command.Parameters.Add(
                new SqlParameter("@OrgName", SqlDbType.NVarChar, 120)
                {
                    Value = request.OrgName
                });

            command.Parameters.Add(
                new SqlParameter("@Address", SqlDbType.NVarChar, 500)
                {
                    Value = request.Address
                });

            command.Parameters.Add(
                new SqlParameter("@ZipCode", SqlDbType.NVarChar, 20)
                {
                    Value = request.ZipCode
                });

            // Build TVP for CityIds
            var citiesTable = new DataTable();
            citiesTable.Columns.Add("CityId", typeof(long));
            foreach (var cityId in request.CityIds)
            {
                citiesTable.Rows.Add(cityId);
            }

            command.Parameters.Add(
        new SqlParameter("@CityIds", SqlDbType.Structured)
        {
            TypeName = "dbo.FundingRecipientCityModel",
            Value = citiesTable
        });

            command.Parameters.Add(
                new SqlParameter("@ContactName", SqlDbType.NVarChar, 100)
                {
                    Value = (object?)request.ContactName ?? DBNull.Value
                });

            command.Parameters.Add(
                new SqlParameter("@Email", SqlDbType.NVarChar, 100)
                {
                    Value = request.Email
                });

            command.Parameters.Add(
                new SqlParameter("@Phone", SqlDbType.NVarChar, 20)
                {
                    Value = (object?)request.Phone ?? DBNull.Value
                });

            command.Parameters.Add(
                new SqlParameter("@IsActive", SqlDbType.Bit)
                {
                    Value = request.IsActive
                });

            command.Parameters.Add(
                new SqlParameter("@CreatedBy", SqlDbType.BigInt)
                {
                    Value = request.CreatedBy
                });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            await reader.ReadAsync(cancellationToken);

            return reader.GetInt64(
                reader.GetOrdinal("FundingRecipientId"));
            //catch (SqlException ex) when (ex.Number is 50001 or 50004)
            //{
            //    throw new ConflictException(ex.Message);
            //}
            //catch (SqlException ex) when (ex.Number == 50005)
            //{
            //    throw new ValidationException(ex.Message);
            //}
        }
    
    public async Task<bool> UpdateAsync(
      UpdateFundingRecipientRequestDto request,
      CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(
                    cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.FundingRecipient.Update,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter("@FundingRecipientId", SqlDbType.BigInt)
                {
                    Value = request.FundingRecipientId
                });

            command.Parameters.Add(
                new SqlParameter("@Name", SqlDbType.NVarChar, 50)
                {
                    Value = request.Name
                });

            command.Parameters.Add(
                new SqlParameter("@OrgName", SqlDbType.NVarChar, 120)
                {
                    Value = request.OrgName
                });

            command.Parameters.Add(
                new SqlParameter("@Address", SqlDbType.NVarChar, 500)
                {
                    Value = request.Address
                });

            command.Parameters.Add(
                new SqlParameter("@ZipCode", SqlDbType.NVarChar, 20)
                {
                    Value = request.ZipCode
                });

            // Build TVP for CityIds
            var citiesTable = new DataTable();
            citiesTable.Columns.Add("CityId", typeof(long));
            foreach (var cityId in request.CityIds)
            {
                citiesTable.Rows.Add(cityId);
            }

            command.Parameters.Add(
        new SqlParameter("@CityIds", SqlDbType.Structured)
        {
            TypeName = "dbo.FundingRecipientCityModel",
            Value = citiesTable
        });

            command.Parameters.Add(
                new SqlParameter("@ContactName", SqlDbType.NVarChar, 100)
                {
                    Value = (object?)request.ContactName ?? DBNull.Value
                });

            command.Parameters.Add(
                new SqlParameter("@Email", SqlDbType.NVarChar, 100)
                {
                    Value = request.Email
                });

            command.Parameters.Add(
                new SqlParameter("@Phone", SqlDbType.NVarChar, 20)
                {
                    Value = (object?)request.Phone ?? DBNull.Value
                });

            command.Parameters.Add(
                new SqlParameter("@IsActive", SqlDbType.Bit)
                {
                    Value = request.IsActive
                });

            command.Parameters.Add(
                new SqlParameter("@UpdatedBy", SqlDbType.BigInt)
                {
                    Value = request.UpdatedBy
                });

            await command.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }
        catch (SqlException ex) when (ex.Number == 50001)
        {
            throw new ConflictException(ex.Message);
        }
        catch (SqlException ex) when (
            ex.Number == 50002 ||
            ex.Number == 50003 ||
            ex.Number == 50004 || ex.Number == 50005)
        {
            throw new ValidationException(ex.Message);
        }
    }
    public async Task<FundingRecipientDetailDto?> GetByIdAsync(
        long fundingRecipientId,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.FundingRecipient.GetById,
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(
            new SqlParameter("@FundingRecipientId", SqlDbType.BigInt)
            {
                Value = fundingRecipientId
            });

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        // Result Set 1: FundingRecipient details
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var fundingRecipient = new FundingRecipientDetailDto
        {
            FundingRecipientId = reader.GetInt64(
                reader.GetOrdinal("FundingRecipientId")),

            Name = reader["Name"]?.ToString()
                ?? string.Empty,

            OrgName = reader["OrgName"]?.ToString()
                ?? string.Empty,

            Address = reader["Address"]?.ToString()
                ?? string.Empty,

            ZipCode = reader["ZipCode"]?.ToString()
                ?? string.Empty,

            IsActive = reader.GetBoolean(
                reader.GetOrdinal("IsActive")),

            CreatedBy = reader.GetInt64(
                reader.GetOrdinal("CreatedBy")),

            CreatedOnUTC = reader.GetDateTime(
                reader.GetOrdinal("CreatedOnUTC")),

            UpdatedBy = reader.IsDBNull(
                reader.GetOrdinal("UpdatedBy"))
                    ? null
                    : reader.GetInt64(
                        reader.GetOrdinal("UpdatedBy")),

            UpdatedOnUTC = reader.IsDBNull(
                reader.GetOrdinal("UpdatedOnUTC"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("UpdatedOnUTC"))
        };

        // Result Set 2: Cities list
        await reader.NextResultAsync(cancellationToken);

        var cities = new List<FundingRecipientCityDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            cities.Add(new FundingRecipientCityDto
            {
                FundingRecipientCityId = reader.GetInt64(
                    reader.GetOrdinal("FundingRecipientCityId")),

                CityId = reader.GetInt64(
                    reader.GetOrdinal("CityId")),

                CityName = reader["CityName"]?.ToString()
                    ?? string.Empty,

                CountryName = reader["CountryName"]?.ToString()
                    ?? string.Empty
            });
        }
        fundingRecipient.Cities = cities;

        // Result Set 3: Contact details
        await reader.NextResultAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            fundingRecipient.ContactId = reader.IsDBNull(
                reader.GetOrdinal("ContactId"))
                    ? null
                    : reader.GetInt64(
                        reader.GetOrdinal("ContactId"));

            fundingRecipient.ContactName = reader.IsDBNull(
                reader.GetOrdinal("ContactName"))
                    ? null
                    : reader["ContactName"]?.ToString();

            fundingRecipient.Email = reader["Email"]?.ToString()
                ?? string.Empty;

            fundingRecipient.Phone = reader.IsDBNull(
                reader.GetOrdinal("Phone"))
                    ? null
                    : reader["Phone"]?.ToString();
        }

        return fundingRecipient;
    }
    //Contact

}