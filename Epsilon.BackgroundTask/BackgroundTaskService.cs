using System;
using Epsilon.Model;

namespace Epsilon.BackgroundTask
{
    public class BackgroundTaskService : IService
    {
        private readonly Model _model;

        public BackgroundTaskService(Model model)
        {
            _model = model;
        }

        public void RunTask(Type type, string jsonConfig)
        {
            _model.AddTask(Guid.NewGuid().ToString("D"), type.FullName, jsonConfig, null);
        }

        public void ScheduleTask(Type type, string jsonConfig, DateTime date)
        {
            _model.AddTask(Guid.NewGuid().ToString("D"), type.FullName, jsonConfig, date);
        }

        public void ScheduleUniqTask(Type type, string jsonConfig, DateTime date, string key)
        {
            if(_model.GetByKey(key) != null)
                    _model.Update(new Model.BgTask{ TaskKey = key, TaskType = type.FullName, JsonConfig = jsonConfig, NextRun = date});
            _model.AddTask(key, type.FullName, jsonConfig, date);
        }
    }

    public interface IBackgroundTask
    {
        void Run(string key, string jsonConfig);
    }
}