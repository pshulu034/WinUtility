using System;
using System.Threading;
using System.Windows.Forms;

namespace TWinService
{
    //这个类需要放在GUI项目中..net-windows才能引用System.Windows.Forms.dll
    public static class ClipboardHelper
    {
        private const int RetryCount = 5;
        private const int RetryDelayMs = 50;

        /* =========================
         * Public API
         * ========================= */

        public static void SetText(string text)
        {
            RunInSta(() =>
            {
                Retry(() => Clipboard.SetText(text ?? string.Empty));
            });
        }

        public static string GetText()
        {
            string result = string.Empty;

            RunInSta(() =>
            {
                Retry(() =>
                {
                    if (Clipboard.ContainsText())
                        result = Clipboard.GetText();
                });
            });

            return result;
        }

        public static bool ContainsText()
        {
            bool result = false;

            RunInSta(() =>
            {
                Retry(() => result = Clipboard.ContainsText());
            });

            return result;
        }

        public static void Clear()
        {
            RunInSta(() =>
            {
                Retry(Clipboard.Clear);
            });
        }

        /* =========================
         * Internal helpers
         * ========================= */

        private static void RunInSta(Action action)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                action();
                return;
            }

            Exception exception = null;

            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
                throw exception;
        }

        private static void Retry(Action action)
        {
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                    Thread.Sleep(RetryDelayMs);
                }
            }

            throw new InvalidOperationException("Clipboard operation failed after retries.");
        }
    }

    public class TClipboardHelper
    {
        public static void Test()
        {
            Clipboard.SetText("Hello");
            string text = ClipboardHelper.GetText();
            ClipboardHelper.Clear();
        }
    }
}
