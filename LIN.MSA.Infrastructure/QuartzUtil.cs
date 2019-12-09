﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace LIN.MSA.Infrastructure
{
    public class QuartzUtil<T> where T : IJob
    {
        public static IScheduler scheduler = null;

        /// <summary>
        /// 获取任务计划
        /// </summary>
        /// <returns></returns>
        public static async Task<IScheduler> GetScheduler()
        {
            if (scheduler != null)
            {
                return scheduler;
            }
            else
            {
                ISchedulerFactory factory = new StdSchedulerFactory();
                scheduler = await factory.GetScheduler();
                return scheduler;
            }
        }

        /// <summary>
        /// 添加任务计划
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AddScheduleJob(ScheduleEntity model)
        {
            try
            {
                if (model != null)
                {
                    if (model.StarRunTime == null)
                    {
                        model.StarRunTime = DateTime.Now;
                    }
                    DateTimeOffset starRunTime = DateBuilder.NextGivenSecondDate(model.StarRunTime, 1);
                    if (model.EndRunTime == null)
                    {
                        model.EndRunTime = DateTime.MaxValue.AddDays(-1);
                    }
                    DateTimeOffset endRunTime = DateBuilder.NextGivenSecondDate(model.EndRunTime, 1);
                    scheduler =await GetScheduler();

                    IJobDetail job = JobBuilder.Create<T>()
                      .WithIdentity(model.JobName, model.JobGroup)
                      .Build();

                    ICronTrigger trigger = null;

                    switch (model.Type)
                    {
                        case ScheduleType.Hour:
                            trigger = (ICronTrigger)TriggerBuilder.Create()
                                                  .StartAt(starRunTime)
                                                  .EndAt(endRunTime)
                                                  .WithIdentity(model.JobName, model.JobGroup)
                                                  .WithSimpleSchedule(x => x
                                                  .WithIntervalInHours(model.Span)
                                                  .RepeatForever())
                                                  .Build();
                            break;
                        case ScheduleType.Minutes:
                            trigger = (ICronTrigger)TriggerBuilder.Create()
                                                  .StartAt(starRunTime)
                                                  .EndAt(endRunTime)
                                                  .WithIdentity(model.JobName, model.JobGroup)
                                                  .WithSimpleSchedule(x => x
                                                  .WithIntervalInMinutes(model.Span)
                                                  .RepeatForever())
                                                  .Build();
                            break;
                        case ScheduleType.Seconds:
                            trigger = (ICronTrigger)TriggerBuilder.Create()
                                                  .StartAt(starRunTime)
                                                  .EndAt(endRunTime)
                                                  .WithIdentity(model.JobName, model.JobGroup)
                                                  .WithSimpleSchedule(x => x
                                                  .WithIntervalInSeconds(model.Span)
                                                  .RepeatForever())
                                                  .Build();
                            break;
                        default:
                            trigger = (ICronTrigger)TriggerBuilder.Create()
                                                  .StartAt(starRunTime)
                                                  .EndAt(endRunTime)
                                                  .WithIdentity(model.JobName, model.JobGroup)
                                                  .WithCronSchedule(model.CronStr)
                                                  .Build();
                            break;
                    }

                    await scheduler.ScheduleJob(job, trigger);
                    await scheduler.Start();
                    
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 暂停计划
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task StopScheduleJob(ScheduleEntity model)
        {
            try
            {
                scheduler = await GetScheduler();
                await scheduler.PauseJob(new JobKey(model.JobName, model.JobGroup));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 暂停计划
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task StopScheduleJob(string jobGroup, string jobName)
        {
            try
            {
                scheduler = await GetScheduler();
                await scheduler.PauseJob(new JobKey(jobName, jobGroup));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 开始计划
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task RunScheduleJob(ScheduleEntity model)
        {
            try
            {
                scheduler = await GetScheduler();
                await scheduler.ResumeJob(new JobKey(model.JobName, model.JobGroup));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 开始计划
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task RunScheduleJob(string jobGroup, string jobName)
        {
            try
            {
                scheduler = await GetScheduler();
                await scheduler.ResumeJob(new JobKey(jobName, jobGroup));
            }
            catch (Exception ex)
            {

            }
        }

    }

    public class ScheduleEntity
    {
        /// <summary>
        /// 开始执行时间
        /// </summary>
        public DateTime StarRunTime { get; set; }
        /// <summary>
        /// 结束执行时间
        /// </summary>
        public DateTime EndRunTime { get; set; }
        /// <summary>
        /// 作业名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 触发器名称
        /// </summary>
        public string JobGroup { get; set; }
        /// <summary>
        /// 间隔表达式
        /// </summary>
        public string CronStr { get; set; }

        /// <summary>
        /// 方式:0 表达式；1：时分秒
        /// </summary>
        public ScheduleType Type { get; set; }

        public int Span { get; set; }
    }

    public enum ScheduleType
    {
        CronStr =0,

        Hour =1,

        Minutes=2,

        Seconds= 3
    }
}