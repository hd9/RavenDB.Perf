using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perf.Core.Entities
{
    // A test record to insert in the database
    public class Listing
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }

        public List<string> Images { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public User CreatedBy { get; set; }

        public User ModifiedBy { get; set; }
    }
}
