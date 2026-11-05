using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.ImpactMaster
{
    public sealed class ImpactMasterFilterOptionsDto
    {

        public List<ProgramFilterOptions> ProgramOptions { get; set; } = new();


        public List<RecordTypeFilterOptions> RecordTypeOptions { get; set; } = new();


        public List<StatusFilterOptions> StatusOptions { get; set; } = new();

    }


    public sealed class ProgramFilterOptions
    {

        public string? ProgramId { get; set; }

        public string? ProgramCode { get; set; }

        public string? ProgramName { get; set; }

    }


    public sealed class RecordTypeFilterOptions
    {

        public string? RecordTypeId { get; set; }

        public string? RecordTypeName { get; set; }

    }


    public sealed class StatusFilterOptions
    {

        public string? StatusId { get; set; }

        public string? StatusCode { get; set; }

        public string? StatusName { get; set; }

    }

    public sealed class ImpactMasterFilterRequestDto
    {

        public string? ProgramId { get; set; }

        public string? RecordTypeId { get; set; }

        public string? StatusId { get; set; }

    }

}
