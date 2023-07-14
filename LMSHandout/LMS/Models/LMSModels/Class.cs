using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public uint ClassId { get; set; }
        public string Season { get; set; } = null!;
        public uint Year { get; set; }
        public string Location { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public uint Listing { get; set; }
        public string? TaughtBy { get; set; }
    }
}
