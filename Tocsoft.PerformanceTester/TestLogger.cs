using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;

namespace Tocsoft.PerformanceTester
{
    public class TestLogger : IMessageLogger
    {
        private AdapterSettings adapterSettings;
        private const string EXCEPTION_FORMAT = "Exception {0}, {1}";

        private IMessageLogger MessageLogger { get; set; }

        public int Verbosity { get; set; }

        public TestLogger(IMessageLogger messageLogger)
        {

            MessageLogger = messageLogger;
        }

        public TestLogger InitSettings(AdapterSettings settings)
        {
            adapterSettings = settings;
            Verbosity = adapterSettings.Verbosity;
            return this;
        }

        #region Error Messages

        public void Error(string message)
        {
            SendMessage(TestMessageLevel.Error, message);
        }

        public void Error(string message, Exception ex)
        {
            SendMessage(TestMessageLevel.Error, message, ex);
        }

        #endregion

        #region Warning Messages

        public void Warning(string message)
        {
            SendMessage(TestMessageLevel.Warning, message);
        }

        public void Warning(string message, Exception ex)
        {
            SendMessage(TestMessageLevel.Warning, message, ex);
        }

        #endregion

        #region Information Messages

        public void Info(string message)
        {
            if (adapterSettings?.Verbosity >= 0)
                SendMessage(TestMessageLevel.Informational, message);
        }

        #endregion

        #region Debug Messages

        public void Debug(string message)
        {
            if (adapterSettings?.Verbosity >= 5)
                SendMessage(TestMessageLevel.Informational, message);

        }

        #endregion

        #region SendMessage

        public void SendMessage(TestMessageLevel testMessageLevel, string message)
        {
            MessageLogger?.SendMessage(testMessageLevel, message);
        }

        public void SendMessage(TestMessageLevel testMessageLevel, string message, Exception ex)
        {
            switch (Verbosity)
            {
                case 0:
                    var type = ex.GetType();
                    SendMessage(testMessageLevel, string.Format(EXCEPTION_FORMAT, type, message));
                    SendMessage(testMessageLevel, ex.Message);
                    SendMessage(testMessageLevel, ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        SendMessage(testMessageLevel, $"Innerexception: {ex.InnerException.ToString()}");
                    }
                    break;

                default:
                    SendMessage(testMessageLevel, message);
                    SendMessage(testMessageLevel, ex.ToString());
                    SendMessage(testMessageLevel, ex.StackTrace);
                    break;
            }
        }
        #endregion
    }
}
