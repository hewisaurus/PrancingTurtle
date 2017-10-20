using System;

namespace Common
{
    public class ReturnValue
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public TimeSpan TimeTaken { get; set; }

        public ReturnValue()
        {
            Initialise(false, "", new TimeSpan());
        }
        
        public ReturnValue(bool success, string message)
        {
            Initialise(success, message, new TimeSpan());
        }

        public ReturnValue(bool success, string message, TimeSpan timeTaken)
        {
            Initialise(success, message, timeTaken);
        }

        private void Initialise(bool success, string message, TimeSpan timeTaken)
        {
            Success = success;
            Message = message;
            TimeTaken = timeTaken;
        }
    }
}
