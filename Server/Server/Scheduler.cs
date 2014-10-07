using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using ServerTools;
using Clients;

namespace Scheduler
{
    public class ScheduleHandler
    {
        static IScheduler Scheduler = StdSchedulerFactory.GetDefaultScheduler();
        public static void InitializeScheduler()
        {
            Scheduler.Start();
        }

        public static void ScheduleTurnOn(ushort DeviceID, CronScheduleBuilder Cron)
        {
            IJobDetail turnOnJob = JobBuilder.Create<TurnOn>()
                .UsingJobData("DeviceID", DeviceID)
                .Build();

            ITrigger turnOnTrigger = TriggerBuilder.Create()
                .WithSchedule(Cron)
                .ForJob(turnOnJob)
                .Build();

            Scheduler.ScheduleJob(turnOnJob, turnOnTrigger);
        }

        public static void ScheduleTurnOff(ushort DeviceID, CronScheduleBuilder Cron)
        {
            IJobDetail turnOffJob = JobBuilder.Create<TurnOff>()
                .UsingJobData("DeviceID", DeviceID)
                .Build();

            ITrigger turnOffTrigger = TriggerBuilder.Create()
                .WithSchedule(Cron)
                .ForJob(turnOffJob)
                .Build();

            Scheduler.ScheduleJob(turnOffJob, turnOffTrigger);
        }

        public static void ScheduleSendMagnitude(ushort DeviceID, CronScheduleBuilder Cron, byte Magnitude)
        {
            IJobDetail sendMagnitudeJob = JobBuilder.Create<SendMagnitude>()
                .UsingJobData("DeviceID", DeviceID)
                .UsingJobData("Magnitude", Magnitude)
                .Build();

            ITrigger sendMagnitudeTrigger = TriggerBuilder.Create()
                .WithSchedule(Cron)
                .ForJob(sendMagnitudeJob)
                .Build();

            Scheduler.ScheduleJob(sendMagnitudeJob, sendMagnitudeTrigger);
        }
    }

    class TurnOn : IJob
    {
        public ushort DeviceID { get; set; }
        public void Execute(IJobExecutionContext context)
        {
            Device D;
            if (Tools.CurrentDeviceList.TryGetValue(DeviceID, out D))
            {
                D.TurnOn();
            }
            else
            {
                Console.WriteLine("Cannot execute scheduled task! Device {0} is not connected.", DeviceID.ToString());
            }
        }
    }

    class TurnOff : IJob
    {
        public ushort DeviceID { get; set; }
        public void Execute(IJobExecutionContext context)
        {
            Device D;
            if (Tools.CurrentDeviceList.TryGetValue(DeviceID, out D))
            {
                D.TurnOff();
            }
            else
            {
                Console.WriteLine("Cannot execute scheduled task! Device {0} is not connected.", DeviceID.ToString());
            }
        }
    }

    class SendMagnitude : IJob
    {
        public ushort DeviceID { get; set; }
        public byte Magnitude { get; set; }
        public void Execute(IJobExecutionContext context)
        {
            Device D;
            if (Tools.CurrentDeviceList.TryGetValue(DeviceID, out D))
            {
                D.SendMagnitude(Magnitude);
            }
            else
            {
                Console.WriteLine("Cannot execute scheduled task! Device {0} is not connected.", DeviceID.ToString());
            }
        }
    }
}
