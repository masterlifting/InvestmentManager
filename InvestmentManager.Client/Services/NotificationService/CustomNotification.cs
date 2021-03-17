using InvestmentManager.Client.Configurations;
using static InvestmentManager.Client.Configurations.EnumConfig;
using System;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.NotificationService
{
    public class CustomNotification
    {
        public Alert Alert { get; private set; } = new Alert();
        public Toast Toast { get; private set; } = new Toast();
        public Confirm Confirm { get; private set; } = new Confirm();
        public bool IsLoading { get; private set; }

        public event Action OnChange;

        #region Toasts
        public void ToastInfo(string title, string message) => ToastBase(ColorCustom.iinfo, title, message);
        public void ToastDanger(string title, string message) => ToastBase(ColorCustom.idanger, title, message);
        public void ToastSuccess(string title, string message) => ToastBase(ColorCustom.isuccess, title, message);
        public void ToastDark(string title, string message) => ToastBase(ColorCustom.idark, title, message);
        public void ToastWarning(string title, string message) => ToastBase(ColorCustom.iwarning, title, message);
        public void ToastSecondary(string title, string message) => ToastBase(ColorCustom.isecondary, title, message);
        private void ToastBase(ColorCustom color, string title, string message)
        {
            Toast.ColorBg = color.ToString();
            Toast.Title = title;
            Toast.Message = message;
            Toast.Visible = true;
            NotifyStateChanged();
        }
        #endregion
        #region Alerts
        public async Task AlertAccessAsync(string message = null, int delay = 1500) =>
            await AlertBaseAsync(ColorCustom.iwarning, string.IsNullOrWhiteSpace(message) ? DefaultString.noticeAccess : message, delay);
        public async Task AlertFailedAsync(string message = null, int delay = 1500) =>
            await AlertBaseAsync(ColorCustom.idanger, string.IsNullOrWhiteSpace(message) ? DefaultString.noticeFailed : message, delay);
        public async Task AlertSuccesAsync(string message = null, int delay = 1500) =>
            await AlertBaseAsync(ColorCustom.isuccess, string.IsNullOrWhiteSpace(message) ? DefaultString.noticeSuccess : message, delay);
        public async Task AlertInfoAsync(string message = null, int delay = 1500) =>
            await AlertBaseAsync(ColorCustom.iinfo, string.IsNullOrWhiteSpace(message) ? DefaultString.noticeInfo : message, delay);
        private async Task AlertBaseAsync(ColorCustom color, string message, int delay)
        {
            Alert.ColorBg = color.ToString();
            Alert.Message = message;
            Alert.Visible = true;
            NotifyStateChanged();
            await Task.Delay(delay);
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
        #region Confirm
        public void ConfirmAction(ColorCustom color, string title, Action action)
        {
            Confirm.ColorBg = color.ToString();
            Confirm.Title = title;
            Confirm.Visible = true;
            Confirm.Action = action;
            NotifyStateChanged();
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
    public class Confirm
    {
        public string ColorBg { get; set; }
        public string Title { get; set; }
        public bool Visible { get; set; }
        public  Action Action { get; set; }
    }
}
