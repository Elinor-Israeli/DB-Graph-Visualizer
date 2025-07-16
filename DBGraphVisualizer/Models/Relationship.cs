// Namespace for all data model classes in the application
namespace DBGraphVisualizer.Models
{
    /// <summary>
    /// Represents a foreign key relationship between two database tables.
    /// </summary>
    public class Relationship
    {
        public string FromTable { get; set; }
        public string ToTable { get; set; }
        public bool IsUnique { get; set; }
    }
}
