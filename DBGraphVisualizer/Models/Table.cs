
// Namespace for all model classes used in the DBGraphVisualizer application
using System.Collections.Generic;
using System.Linq;

namespace DBGraphVisualizer.Models
{
    /// <summary>
    /// Represents a single table in the database schema.
    /// Includes the table's name, primary key columns, and foreign key columns.
    /// </summary>
    public class Table
    {  
        public string Name { get; set; }
        public List<string> PrimaryKeys { get; set; } = new();
        public List<string> ForeignKeys { get; set; } = new();

        public List<string> AllKeys
        {
            get
            {
                return PrimaryKeys.Concat(ForeignKeys).Distinct().ToList();
            }

        }
        
    }
}



