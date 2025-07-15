// Namespace that stores all reusable SQL query strings for loading schema information
namespace AdventureWorksGraph.SqlQueries
{
    /// <summary>
    /// Provides SQL queries for extracting table metadata (keys and relationships)
    /// from the AdventureWorks SQL Server database.
    /// </summary>
    public static class SchemaSql
    {
        /// <summary>
        /// SQL query to get all tables and their primary key columns.
        /// It joins system catalog views to find which columns belong to primary keys.
        /// </summary>
        public const string PrimaryKeyQuery = @"
            SELECT t.name, c.name
            FROM sys.tables t
            JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
            JOIN sys.index_columns ic ON ic.object_id = t.object_id AND ic.index_id = i.index_id
            JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id";

        /// <summary>
        /// SQL query to get all tables and their foreign key columns.
        /// It finds columns that act as foreign keys by joining system views.
        /// </summary>
        public const string ForeignKeyQuery = @"
            SELECT t.name, c.name
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
            JOIN sys.tables t ON fk.parent_object_id = t.object_id
            JOIN sys.columns c ON t.object_id = c.object_id AND fkc.parent_column_id = c.column_id";

        /// <summary>
        /// SQL query to get detailed foreign key relationships between tables.
        /// Returns: source table/column → target table/column.
        /// </summary>
        public const string RelationshipQuery = @"
            SELECT tp.name, cp.name, tr.name, cr.name
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
            JOIN sys.tables tp ON tp.object_id = fk.parent_object_id
            JOIN sys.columns cp ON cp.object_id = tp.object_id AND cp.column_id = fkc.parent_column_id
            JOIN sys.tables tr ON tr.object_id = fk.referenced_object_id
            JOIN sys.columns cr ON cr.object_id = tr.object_id AND cr.column_id = fkc.referenced_column_id";
    }
}
