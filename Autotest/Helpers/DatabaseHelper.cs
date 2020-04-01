using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autotest.Providers;

namespace Autotest.Helpers
{
    internal class DatabaseHelper
    {
        public static void ExecuteQuery(string query)
        {
            SqlConnection sqlConnection = new SqlConnection(Database.ConnectionsString);
            SqlCommand cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = query,
                Connection = sqlConnection
            };

            sqlConnection.Open();
            cmd.ExecuteNonQuery();
            sqlConnection.Close();
        }

        /// <summary>
        /// columnIndex starts from 0.
        /// columnIndex is the index of the column
        /// [0] is the index of the row
        /// Usage: DatabaseWorker.ExecuteReader(dbName.ECA_database, query, 0)[0]
        /// To get data from the last row just use '.LastOrDefault()' instead of [0] index
        /// </summary>
        /// <returns></returns>
        public static List<string> ExecuteReader(string query, int columnIndex)
        {
            var results = ExecuteReaderWithoutZeroResultCheck(query, columnIndex);

            if (results.Any())
                return results;
            else
                throw new Exception("Database query returns zero results");
        }

        public static List<string> ExecuteReaderWithoutZeroResultCheck(string query, int columnIndex)
        {
            SqlConnection sqlConnection1 = new SqlConnection(Database.ConnectionsString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection1;

            sqlConnection1.Open();

            reader = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);

            sqlConnection1.Close();

            return dt.AsEnumerable().ToList().Select(row => row[columnIndex].ToString()).ToList();
        }
    }
}
