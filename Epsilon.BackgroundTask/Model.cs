using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Epsilon.Model;

namespace Epsilon.BackgroundTask
{
    public class Model : IModel
    {
        private readonly IConfig _config;

        public Model(IConfig config)
        {
            _config = config;
        }

        public void AddTask(string taskKey, string taskType, string jsonConfig, DateTime? nextRun)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            connection.Execute("insert into Ep.tBackgroundTask (TaskKey, TaskType, JsonConfig, NextRun", 
                new{ taskKey , taskType , jsonConfig , nextRun });
        }

        public IEnumerable<BgTask> ShouldRunTasks()
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.Query<BgTask>(@"
                SELECT * from Ep.tBackgroundTask WHERE ShouldRun = 1 ORDER BY NextRun");
        }

        public BgTask GetByKey(string key)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.QuerySingleOrDefault<BgTask>(@"
                SELECT * from Ep.tBackgroundTask WHERE TaskKey = @key",new{ key });
        }

        public IEnumerable<BgTask> NotYetDoneTasks()
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.Query<BgTask>(@"
                SELECT * from Ep.tBackgroundTask WHERE State != 'done'");
        }

        public void SetState(string key, string state)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            connection.Execute(@"UPDATE Ep.tBackgroundTask set State = @state, NbRun = NbRun +1 WHERE TaskKey = @key", new{key, state});
        }

        public void SetNextRun(string key, DateTime date)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            connection.Execute(@"UPDATE Ep.tBackgroundTask set NextRun = @date, State= 'waiting' WHERE TaskKey = @key", new { key, date });
        }


        public void Update(BgTask task)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            connection.Execute(@"UPDATE Ep.tBackgroundTask set 
                TaskType = @TaskType,
                JsonConfig = @JsonConfig, 
                NextRun = @NextRun, 
                State = @State
            WHERE TaskKey = @key", task);
        }


        public class BgTask
        {
            public string TaskKey { get; set; }
            public string TaskType { get; set; }
            public string JsonConfig { get; set; }

            public DateTime NextRun { get; set; }
            public DateTime LastRun { get; set; }

            public string State { get; set; }
            public bool ShouldRun { get; set; }
            public int NbRun { get; set; }
        }
    }
}
