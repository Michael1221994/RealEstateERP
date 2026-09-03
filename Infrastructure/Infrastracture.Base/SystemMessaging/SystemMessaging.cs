using Perago.Enterprise.SystemMessaging.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Perago.Enterprise.SystemMessaging
{

    public class SystemMessaging
    {
        public static SystemMessage ErrorMessage<T>(T message, string? moduleCode = null) where T : Enum
        {
            if (message != null)
            {
                if (!Enum.IsDefined(typeof(T), message))
                {
                    throw new Exception("Invalid Message Type!");
                }
                var msgObject = Enum.ToObject(typeof(T), message);


                if (400 > (int)msgObject || (int)msgObject >= 600)
                {
                    throw new Exception("Invalid Error Message code! Error message Enum MessageCode should be between 400-600");
                }


                SystemMessage systemMessage = new SystemMessage(moduleCode);
                systemMessage.MessageCode = "E" + (int)msgObject;
                systemMessage.Message = message.ToString()?.Replace('_', ' ');
                systemMessage.MessageType = SystemMessageType.Error;
                return systemMessage;
            }

            else { throw new Exception("Invalid Message Type!"); }
        }

        public static SystemMessage WarningMessage<T>(T message, string? moduleCode = null)
        {
            if (!Enum.IsDefined(typeof(T), message))
            {
                throw new Exception("Invalid Message Type");
            }

            var msgObject = Enum.ToObject(typeof(T), message);

            if (600 > (int)msgObject || (int)msgObject >= 800)
            {
                throw new Exception("Invalid Error Message code! Warning message Enum MessageCode should be between 600-800");
            }

            SystemMessage systemMessage = new SystemMessage(moduleCode);
            systemMessage.MessageCode = "W" + (int)msgObject;
            systemMessage.Message = message?.ToString()?.Replace('_', ' ');
            systemMessage.MessageType = SystemMessageType.Warning;
            return systemMessage;
        }

        public static SystemMessage InformationMessage<T>(object message, string? moduleCode = null)
        {
            if (!Enum.IsDefined(typeof(T), message))
            {
                throw new Exception("Invalid Message Type");
            }

            if (1 > (int)message || (int)message >= 200)
            {
                throw new Exception("Invalid Error Message code! Information message Enum MessageCode should be between 1-200");
            }

            SystemMessage systemMessage = new SystemMessage();
            systemMessage.MessageCode = "I" + (int)message;
            systemMessage.Message = message.ToString()?.Replace('_', ' ');
            systemMessage.MessageType = SystemMessageType.Information;
            return systemMessage;
        }

        public static SystemMessage SuccessMessage<T>(T message, string? moduleCode = null)
        {
            if (message != null)
            {
                if (!Enum.IsDefined(typeof(T), message))
                {
                    throw new Exception("Invalid Message Type!");
                }
                var msgObject = Enum.ToObject(typeof(T), message);

                if (200 > (int)msgObject || (int)msgObject >= 400)
                {
               
                    throw new Exception("Invalid Error Message code! Success message Enum MessageCode should be between 200-400");
                }

                SystemMessage systemMessage = new SystemMessage();
                systemMessage.MessageCode = "S" + (int)msgObject;
                systemMessage.Message = message.ToString().Replace('_', ' ');
                systemMessage.MessageType = SystemMessageType.Success;
                return systemMessage;
            }

            else { throw new Exception("Invalid Message Type!"); }
        }

        public static SystemMessage UnknownMessage<T>(object message, string? moduleCode = null)
        {
            if (!Enum.IsDefined(typeof(T), message))
            {
                throw new Exception("Invalid Message Type");
            }

            if ((int)message < 800)
            {
                throw new Exception("Invalid Error Message code! Unknown message Enum MessageCode should be greater than 800");
            }

            SystemMessage systemMessage = new SystemMessage();
            systemMessage.MessageCode = "U" + (int)message;
            systemMessage.Message = message.ToString()?.Replace('_', ' ');
            systemMessage.MessageType = SystemMessageType.Unknown;
            return systemMessage;
        }
    }
}

namespace Perago.Enterprise.SystemMessaging.Entity
{
    public class SystemMessage : EndUserSystemMessage
    {
        [JsonIgnore]
        public Exception? Exception { get; set; } 

        public string ExceptionMessage { get; set; } = string.Empty;

        public SystemMessage()
        {
        }

        public SystemMessage(string? moduleCode = null)
            : base(moduleCode)
        {
        }
    }

    public class EndUserSystemMessage
    {
        private string? moduleCode;

        public string Culture { get; set; } = string.Empty;

        public SystemMessageType MessageType { get; set; }

        public string Type => MessageType.ToString();

        public string Message { get; set; } = string.Empty;

        public string MessageCode { get; set; } = string.Empty;

        public string SystemMessageCode => MessageCode + "-" + moduleCode;

        public bool HasDetail { get; set; }

        public string? ModuleCode => moduleCode;

        public EndUserSystemMessage()
        {
        }

        public EndUserSystemMessage(string? moduleCode = null)
        {
            this.moduleCode = moduleCode;
        }
    }

    public enum SystemMessageType
    {
        Error,
        Warning,
        Information,
        Success,
        Unknown

    }

}