using InvestmentManager.Client.ChoiceModel;
using InvestmentManager.ViewModels;
using System;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.NotificationService
{
    public class Notification
    {
        public Alert Alert { get; private set; } = new Alert();
        public Toast Toast { get; private set; } = new Toast();
        public bool IsLoading { get; private set; }

        public event Action OnChange;

        #region Toasts
        public void ToastInfo(string title, string message) => ToastBase(Choice.InvestColor.iinfo, title, message);
        public void ToastDanger(string title, string message) => ToastBase(Choice.InvestColor.idanger, title, message);
        public void ToastSuccess(string title, string message) => ToastBase(Choice.InvestColor.isuccess, title, message);
        public void ToastDark(string title, string message) => ToastBase(Choice.InvestColor.idark, title, message);
        public void ToastWarning(string title, string message) => ToastBase(Choice.InvestColor.iwarning, title, message);
        public void ToastSecondary(string title, string message) => ToastBase(Choice.InvestColor.isecondary, title, message);
        private void ToastBase(Choice.InvestColor color, string title, string message)
        {
            Toast.ColorBg = color.ToString();
            Toast.Title = title;
            Toast.Message = message;
            Toast.Visible = true;
            NotifyStateChanged();
        }
        #endregion
        #region Alerts
        public async Task AlerAccessAsync(string message = null, int delay = 1500) =>
            await AlertBaseAsync(Choice.InvestColor.iwarning, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeAccess : message, delay).ConfigureAwait(false);
        public async Task AlertFailedAsync(string message = null, int delay = 1500) =>
            await AlertBaseAsync(Choice.InvestColor.idanger, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeFailed : message, delay).ConfigureAwait(false);
        public async Task AlertSuccesAsync(string message = null, int delay = 1500) =>
            await AlertBaseAsync(Choice.InvestColor.isuccess, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeSuccess : message, delay).ConfigureAwait(false);
        public async Task AlertInfoAsync(string message = null, int delay = 1500) =>
            await AlertBaseAsync(Choice.InvestColor.isecondary, string.IsNullOrWhiteSpace(message) ? DefaultData.noticeInfo : message, delay).ConfigureAwait(false);
        private async Task AlertBaseAsync(Choice.InvestColor color, string message, int delay)
        {
            Alert.ColorBg = color.ToString();
            Alert.Message = message;
            Alert.Visible = true;
            NotifyStateChanged();
            await Task.Delay(delay).ConfigureAwait(false);
            Alert.Visible = false;
            NotifyStateChanged();
        }
        #endregion
        #region Loading
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
        #endregion

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
    public class Alert
    {
        public string ColorBg { get; set; }
        public string Message { get; set; }
        public bool Visible { get; set; }
    }
    public class Toast
    {
        public string ColorBg { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
        public bool Visible { get; set; }
    }
}
