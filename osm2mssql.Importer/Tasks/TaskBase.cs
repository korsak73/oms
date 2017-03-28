using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using osm2mssql.Importer.Enums;
using System.Windows.Threading;
using osm2mssql.Library.OsmReader;
using osm2mssql.Importer.Properties;

namespace osm2mssql.Importer.Tasks
{
    public abstract class TaskBase : INotifyPropertyChanged
    {
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                Notify("IsEnabled");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Notify("Name");
            }
        }

        public TimeSpan Duration
        {
            get { return DateTime.Now - _startTime; }
        }

        public TaskResult Result
        {
            get { return _result; }
            set
            {
                _result = value;
                Notify("Result");
            }
        }

        public string StepDescription
        {
            get { return _stepDescription; }
            set
            {
                _stepDescription = value;
                Notify("StepDescription");
            }
        }

        public string AdditionalInfos
        {
            get { return _additionalInfos; }
            set
            {
                _additionalInfos = value;
                Notify("AdditionalInfos");
            }
        }

        public Exception LastError { get; private set; }
        public TaskType Type { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private TaskResult _result;
        private string _name;
        private string _stepDescription;
        private string _additionalInfos;
        private readonly List<Task> _runningTaskList = new List<Task>();
        private DateTime _startTime = DateTime.Now;
        private bool _isEnabled = true;

        protected int MaxRowCountInMemory = Settings.Default.MaxRowCountInMemory;
        protected int MaxPendingOperations = Settings.Default.MaxPendingOperations;

        protected SqlConnectionStringBuilder Connection { get; private set; }

        protected TaskBase(TaskType taskType, string name)
        {
            Type = taskType;
            Name = name;
        }

        protected void Notify(string propertyName)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        /// <summary>
        /// Open a connection and executes the SqlCommand
        /// </summary>
        /// <param name="sqlCommand">SqlCommand</param>
        protected void ExecuteSqlCmd(string sqlCommand)
        {
            var sqlCommands = sqlCommand.Split(
                new[]
                    {
                        "GO"
                    }, StringSplitOptions.RemoveEmptyEntries);

            using (var con = new SqlConnection(Connection.ToString()))
            {
                con.Open();

                foreach (var sql in sqlCommands)
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandTimeout = int.MaxValue;
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        protected void DropIndexIfExist(string table, string name)
        {
            var sql = string.Format("IF EXISTS (SELECT name FROM sysindexes WHERE name = '{0}') DROP INDEX {1}.{0}", name, table);
            ExecuteSqlCmd(sql);
        }

        protected async Task<T> QuerySqlCmd<T>(string sqlCommand)
        {
            using (var con = new SqlConnection(Connection.ToString()))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandTimeout = int.MaxValue;
                    cmd.CommandText = sqlCommand;
                    return (T) await cmd.ExecuteScalarAsync();
                }
            }
        }

        /// <summary>
        /// Method for inserting a DataTable into a table - starts an asynchronous bulk insertion
        /// </summary>
        /// <param name="tableName">Destination table</param>
        /// <param name="dt">DataTable - flat copy will be made of it</param>
        protected void WriteToDb(string tableName, DataTable dt)
        {

            var tmp = dt.Clone();
            foreach (DataRow row in dt.Rows)
                tmp.Rows.Add(row.ItemArray);
            dt.Rows.Clear();

            var task = new Task(() =>
            {
                try
                {
                    var connBuilder = new SqlConnectionStringBuilder(Connection.ToString());
                    var options = SqlBulkCopyOptions.TableLock; //Is Faster with TableLock - also with parallel Imports
                    using (var s = new SqlBulkCopy(connBuilder.ToString(), options))
                    {
                        s.BulkCopyTimeout = int.MaxValue;
                        s.BatchSize = 0;
                        s.DestinationTableName = tableName;
                        s.WriteToServer(tmp);
                        s.Close();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Table: " + tableName + "\nBulk insert failed: " + ex);
                }
            });
            CheckForTooMuchConcurrentWork();

            lock (_runningTaskList)
            {
                _runningTaskList.Add(task);
            }
            task.Start();
        }
        
        private void CheckForTooMuchConcurrentWork()
        {
            lock (_runningTaskList)
            {
                foreach (var finished in _runningTaskList.Where(d => d.Status == TaskStatus.RanToCompletion).ToList())
                    _runningTaskList.Remove(finished);

                if (_runningTaskList.Count >= MaxPendingOperations)
                {
                    Task.WaitAll(_runningTaskList.Take(MaxPendingOperations / 2).ToArray());
                }
            }
        }

        /// <summary>
        /// Blocking method for waiting until all operations have finished
        /// </summary>
        protected void WaitUntilFinished()
        {
            Task.WaitAll(_runningTaskList.ToArray());
        }

        internal virtual void DurationRefresh()
        {
            if (Duration.TotalSeconds > 0)
                Notify("Duration");
        }

        /// <summary>
        /// Method for starting the work of this Task
        /// </summary>
        /// <param name="connection"></param>
        public void RunTask(SqlConnectionStringBuilder connection, string osmFile, AttributeRegistry attributeRegistry)
        {
            _startTime = DateTime.Now;

            try
            {
                Result = TaskResult.InProgress;
                Connection = connection;
                DoTaskWork(osmFile, attributeRegistry);
                var logLine = string.Format("Finished Task {0} - Duration: {1}", Name, Duration);
                Trace.WriteLine(logLine);
                Result = TaskResult.Successful;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                Result = TaskResult.Error;
                LastError = ex;
                throw;
            }
        }

        protected abstract void DoTaskWork(string osmFile, AttributeRegistry attributeRegistry);

    }
}
