// Bring in models, SQL queries, and ADO.NET tools for database access
using DBGraphVisualizer.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DBGraphVisualizer.Services
{
    /// <summary>
    /// This static class handles loading database schema info (tables, keys, relationships)
    /// from the AdventureWorks SQL Server database.
    /// </summary>
    public static class SchemaLoader
    {
        private const string ConnectionString = "Server=IZHAK-LENOVO\\ELINORSQLSERVER;Database=AdventureWorks2022;Trusted_Connection=True;";


        /// <summary>
        /// Loads all tables from the database with their primary and foreign keys.
        /// </summary>
        /// <returns>
        /// A dictionary where the key is the table name and the value is a Table object.
        /// </returns>
        public static Dictionary<string, Table> LoadTables()
        {
            var tables = new Dictionary<string, Table>();

            ExecuteTableKeyQuery(SchemaSql.PrimaryKeyQuery, isPrimaryKey: true, tables);
            ExecuteTableKeyQuery(SchemaSql.ForeignKeyQuery, isPrimaryKey: false, tables);

            return tables;
        }

        /// <summary>
        /// Loads all foreign key relationships between tables.
        /// </summary>
        /// <returns>
        /// A list of Relationship objects representing foreign key links.
        /// </returns>
        public static List<Relationship> LoadRelationships(Dictionary<string, Table> tables)
        {
            var relationships = new List<Relationship>();

            using var conn = new SqlConnection(ConnectionString);
            conn.Open();

            using var cmd = new SqlCommand(SchemaSql.RelationshipQuery, conn);
            using var reader = cmd.ExecuteReader();

            // Read each row: FromTable.FromColumn ➝ ToTable.ToColumn
            while (reader.Read())
            {
                var fromTable = reader.GetString(0);
                var fromColumn = reader.GetString(1);
                relationships.Add(new Relationship
                {
                    FromTable = fromTable,
                    ToTable = reader.GetString(2),
                    IsUnique  =tables[fromTable].PrimaryKeys.Contains(fromColumn),
                });
            }

            return relationships.GroupBy(p => new { p.FromTable, p.ToTable }).Select(g => g.First()).ToList();
        }

        /// <summary>
        /// A shared method for loading primary or foreign keys using a given SQL query.
        /// </summary>
        /// <param name="query">The SQL query to run (PK or FK)</param>
        /// <param name="isPrimaryKey">True if loading PKs, false for FKs</param>
        /// <param name="tables">The dictionary of Table objects to populate</param>
        private static void ExecuteTableKeyQuery(string query, bool isPrimaryKey, Dictionary<string, Table> tables)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();

            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string tableName = reader.GetString(0);
                string columnName = reader.GetString(1);

                // Create new Table object if it doesn't exist yet
                if (!tables.ContainsKey(tableName))
                    tables[tableName] = new Table { Name = tableName };

                // Add to the appropriate list: PrimaryKeys or ForeignKeys
                var keyList = isPrimaryKey ? tables[tableName].PrimaryKeys : tables[tableName].ForeignKeys;
                if (!keyList.Contains(columnName))
                    keyList.Add(columnName);
            }
        }
    }
}
