using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace TelegramBot.Common.Configurations
{
    public class WebApiConfiguration
    {
        private readonly IConfiguration configuration;

        public WebApiConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private T GetValue<T>(string key)
        {
            try
            {
                var value = configuration[key];

                var typeConverter = TypeDescriptor.GetConverter(typeof(T));
                return (T)typeConverter.ConvertFromString(value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Key: {key}", ex);
            }
        }
    }
}
