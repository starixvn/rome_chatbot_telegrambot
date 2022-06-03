using Quartz;
using TelegramBot.SenderService.Constants;
using TelegramBot.SenderService.Jobs;
using TelegramBot.SenderService.Models;

namespace TelegramBot.SenderService.Services
{
    public interface IMessageSender
    {
        Task SendNotification(Notification notification);

        Task RetrySendNotification(Notification notification, string environment);
    }

    public class MessageSender : IMessageSender
    {
        private readonly IBotService botService;
        private readonly ISchedulerFactory schedulerFactory;
        private readonly IJobListener retryJobListener;

        public MessageSender(IBotService botService, ISchedulerFactory schedulerFactory, IJobListener retryJobListener)
        {
            this.botService = botService;
            this.schedulerFactory = schedulerFactory;
            this.retryJobListener = retryJobListener;
        }

        public async Task SendNotification(Notification notification)
        {
            await botService.SendMessage(notification.BotId, notification.ChatId, notification.Message);
        }

        public async Task RetrySendNotification(Notification notification, string environment)
        {
            var scheduler = await schedulerFactory.GetScheduler();
            scheduler.ListenerManager.AddJobListener(retryJobListener);

            var job = JobBuilder.Create<RetryJob>()
                        .WithIdentity($"{notification.NotificationId}", "SendMessage")
                        .UsingJobData(JobDataKey.ChatId, notification.ChatId)
                        .UsingJobData(JobDataKey.BotId, notification.BotId)
                        .UsingJobData(JobDataKey.NotificationId, notification.NotificationId)
                        .UsingJobData(JobDataKey.Message, notification.Message)
                        .UsingJobData(JobDataKey.Environment, environment)
                        .Build();
            var trigger = TriggerBuilder
                        .Create()
                        .StartNow()
                        .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}