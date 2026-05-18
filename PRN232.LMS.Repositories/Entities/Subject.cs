using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.LMS.Repositories.Entities
{
    public class Subject
    {
        public int SubjectId { get; set; }

        public string SubjectCode { get; set; } = null!;

        public string SubjectName { get; set; } = null!;

        public int Credit { get; set; }
        public ICollection<Course> Courses { get; set; }
        = new List<Course>();
    }
}
