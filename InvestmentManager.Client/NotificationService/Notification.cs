using InvestmentManager.Client.ChoiceModel;
using InvestmentManager.ViewModels;
using System;
using System.Threading.Tasks;

namespace InvestmentManager.Client.NotificationService
{
    public class Notification
    {
        public string SideColor { get; private set; }
        public string Message { get; private set; }
        public string Title { get; private set; }
        public bool VisibleAlert { get; set; }
        public bool VisibleInfo { get; set; }

        public event Action OnChange;


        public void ShowInfo(string title, string message)
        {
            SideColor = string.Intern(Choices.Color.info.ToString());
            Title = title;
            Message = message;
            VisibleInfo = true;
            NotifyStateChanged();
        }

        public async Task NoticeAccessAsync(string message = null) => await GetNoticeAsync(Choices.Color.warning, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeAccess : message).ConfigureAwait(false);
        public async Task NoticeFailedAsync(string message = null) => await GetNoticeAsync(Choices.Color.danger, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeFailed : message).ConfigureAwait(false);
        public async Task NoticeSuccesAsync(string message = null) => await GetNoticeAsync(Choices.Color.success, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeSuccess : message).ConfigureAwait(false);
        public async Task NoticeInfoAsync(string message = null) => await GetNoticeAsync(Choices.Color.secondary, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeInfo : message).ConfigureAwait(false);

        private async Task GetNoticeAsync(Choices.Color color, string message)
        {
            SideColor = string.Intern(color.ToString());
            Message = message;
            VisibleAlert = true;
            NotifyStateChanged();
            await Task.Delay(1000).ConfigureAwait(false);
            VisibleAlert = false;
            NotifyStateChanged();
        }
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
