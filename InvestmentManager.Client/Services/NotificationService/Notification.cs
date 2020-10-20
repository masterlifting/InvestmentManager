using InvestmentManager.Client.ChoiceModel;
using InvestmentManager.ViewModels;
using System;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.NotificationService
{
    public class Notification
    {
        public Notice Notice { get; set; } = new Notice();
        public Info Info { get; set; } = new Info();
        public bool IsLoading { get; private set; }

        public event Action OnChange;


        public void ShowInfo(string title, string message)
        {
            Info.ColorBg = Choice.InvestColor.iinfo.ToString();
            Info.Title = title;
            Info.Message = message;
            Info.Visible = true;
            NotifyStateChanged();
        }

        public async Task NoticeAccessAsync(string message = null, int delay = 1500) =>
            await GetNoticeAsync(Choice.InvestColor.iwarning, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeAccess : message, delay).ConfigureAwait(false);
        public async Task NoticeFailedAsync(string message = null, int delay = 1500) =>
            await GetNoticeAsync(Choice.InvestColor.idanger, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeFailed : message, delay).ConfigureAwait(false);
        public async Task NoticeSuccesAsync(string message = null, int delay = 1500) =>
            await GetNoticeAsync(Choice.InvestColor.isuccess, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeSuccess : message, delay).ConfigureAwait(false);
        public async Task NoticeInfoAsync(string message = null, int delay = 1500) =>
            await GetNoticeAsync(Choice.InvestColor.isecondary, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeInfo : message, delay).ConfigureAwait(false);

        private async Task GetNoticeAsync(Choice.InvestColor color, string message, int delay)
        {
            Notice.ColorBg = color.ToString();
            Notice.Message = message;
            Notice.Visible = true;
            NotifyStateChanged();
            await Task.Delay(delay).ConfigureAwait(false);
            Notice.Visible = false;
            NotifyStateChanged();
        }

        public void LoadStart()
        {
            if (!IsLoading)
            {
                IsLoading = true;
                NotifyStateChanged();
            }
        }
        public void LoadStop()
        {
            if (IsLoading)
            {
                IsLoading = false;
                NotifyStateChanged();
            }
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
}
