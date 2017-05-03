using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Timers;
using log4net;

namespace DatabaseWatcher
{
    public class Program : IDisposable
    {
        private DataTable _oldValue;
        private readonly SqlConnection _connection;
        private string _query = ""; // UPDATE THIS
        private string _keyColumn = ""; // UPDATE THIS
        private ILog log = log4net.LogManager.GetLogger("DatabaseWatcher");

        public Program()
        {
            log4net.Config.XmlConfigurator.Configure();
            Timer timer = new Timer(1000);
            timer.Elapsed += (sender, args) => this.WatchDatabase();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ""; // UPDATE THIS
            builder.InitialCatalog = ""; // UPDATE THIS
            builder.UserID = ""; // UPDATE THIS
            builder.Password = ""; // UPDATE THIS
            this._connection = new SqlConnection(builder.ToString());
            this._connection.Open();
            timer.Start();
            this.log.Info("Started...");
        }

        private void WatchDatabase()
        {
            DataTable newDataTable = null;

            using (SqlCommand command = this._connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = this._query;
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataSet newDataSet = new DataSet();
                    adapter.Fill(newDataSet);
                    if (newDataSet.Tables.Count > 0)
                    {
                        newDataTable = newDataSet.Tables[0];
                    }
                    if (this._oldValue == null)
                    {
                        this._oldValue = newDataTable;
                    }
                }
            }

            this.LogTableDifferences(newDataTable);
            this._oldValue = newDataTable;
        }

        private void LogTableDifferences(DataTable newDataTable)
        {
            var newCells = (from row in (newDataTable.Rows.Cast<DataRow>()).ToList()
                           from col in (newDataTable.Columns.Cast<DataColumn>()).ToList()
                           select new { Key = row[this._keyColumn].ToString(), Column = col.ColumnName, Value = row[col.ColumnName].ToString() }).ToList();
            var oldCells = (from row in (this._oldValue.Rows.Cast<DataRow>()).ToList()
                            from col in (this._oldValue.Columns.Cast<DataColumn>()).ToList()
                            select new { Key = row[this._keyColumn].ToString(), Column = col.ColumnName, Value = row[col.ColumnName].ToString() }).ToList();
            var additions = from newCell in newCells
                            join oldCell in oldCells
                                on new { newCell.Key, newCell.Column } equals new { oldCell.Key, oldCell.Column } into addGroup
                            from item in addGroup.DefaultIfEmpty()
                            where (item == null ? "" : item.Value) != newCell.Value
                            select
                                new
                                {
                                    Key = newCell.Key.ToString(),
                                    Column = newCell.Column,
                                    NewValue = newCell.Value.ToString(),
                                    OldValue = (item == null ? "" : item.Value).ToString()
                                };

            foreach(var item in additions)
            {
                this.log.Error("Column Updated.\r\nKey: " + item.Key + "\r\nColumn: " + item.Column);
                this.log.Error("OldValue: " + item.OldValue + "\r\nNewValue: " + item.NewValue);
            }

        }

        public static void Main(string[] args)
        {
            using (var program = new Program())
            {
                Console.ReadKey();
            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this._connection.State != ConnectionState.Open)
                        this._connection.Close();
                    this._connection.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}
