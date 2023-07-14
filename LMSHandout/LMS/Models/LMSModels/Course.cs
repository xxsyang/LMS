using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public uint CatalogId { get; set; }
        public uint Number { get; set; }
        public string Name { get; set; } = null!;
        public string Department { get; set; } = null!;
    }
}
