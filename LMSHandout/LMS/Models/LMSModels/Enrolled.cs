using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrolled
    {
        public string Student { get; set; } = null!;
        public uint Class { get; set; }
        public string Grade { get; set; } = null!;
    }
}
