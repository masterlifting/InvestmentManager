using InvestmentManager.Client.ChoiceModel;
using InvestmentManager.ViewModels;
using System;
using System.Threading.Tasks;

namespace InvestmentManager.Client.NotificationService
{
    public class Notification
    {
        public Notice Notice { get; set; } = new Notice();
        public Info Info { get; set; } = new Info();

        public event Action OnChange;


        public void ShowInfo(string title, string message)
        {
            Info.ColorBg = string.Intern(Choice.Color.info.ToString());
            Info.Title = title;
            Info.Message = message;
            Info.Visible = true;
            NotifyStateChanged();
        }

        public async Task NoticeAccessAsync(string message = null) => await GetNoticeAsync(Choice.Color.warning, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeAccess : message).ConfigureAwait(false);
        public async Task NoticeFailedAsync(string message = null) => await GetNoticeAsync(Choice.Color.danger, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeFailed : message).ConfigureAwait(false);
        public async Task NoticeSuccesAsync(string message = null) => await GetNoticeAsync(Choice.Color.success, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeSuccess : message).ConfigureAwait(false);
        public async Task NoticeInfoAsync(string message = null) => await GetNoticeAsync(Choice.Color.secondary, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeInfo : message).ConfigureAwait(false);

        private async Task GetNoticeAsync(Choice.Color color, string message)
        {
            Notice.ColorBg = string.Intern(color.ToString());
            Notice.Message = message;
            Notice.Visible = true;
            NotifyStateChanged();
            await Task.Delay(1500).ConfigureAwait(false);
            Notice.Visible = false;
            NotifyStateChanged();
        }
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
    public class Notice
    {
        public string ColorBg { get; set; }
        public string Message { get; set; }
        public bool Visible { get; set; }
    }
    public class Info
    {
        public string ColorBg { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
        public bool Visible { get; set; }

    }
    public class Loading
    {

    }
}
