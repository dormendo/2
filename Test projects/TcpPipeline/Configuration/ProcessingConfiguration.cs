using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using JetBrains.Annotations;
using Maxima.Server.Configuration;

namespace Maxima.Tcp
{
    /// <summary>
    /// Конфигурация обработки команд сервером.
    /// </summary>
    public sealed class ProcessingConfiguration
    {
        public TimeSpan HeaderReadingTimeout { get; private set; }

        public TimeSpan MessageRecieveTimeout { get; private set; }

        public TimeSpan ResponseSendingTimeout { get; private set; }

        public int MaxMessageBodySize { get; private set; }

        public ReadOnlyDictionary<string, CommandProcessingConfiguration> CommandOverrides { get; private set; }

        private ProcessingConfiguration()
        {
        }

        public static ProcessingConfiguration LoadConfigurationFromAppConfig()
        {
            ServerConfigurationSection serverConfigurationSection = (ServerConfigurationSection) ConfigurationManager.GetSection("server");
            ProcessingConfigurationElement processingConfig = serverConfigurationSection.Processing;

            Dictionary<string, CommandProcessingConfiguration> commandOverrrides = processingConfig.Commands
                .Cast<CommandConfigurationElement>()
                .ToDictionary(element => element.Name, CommandProcessingConfiguration.Create);

            ProcessingConfiguration processingConfiguration = new ProcessingConfiguration
            {
                MaxMessageBodySize = processingConfig.MaxMessageBodySize,
                HeaderReadingTimeout = processingConfig.HeaderReadingTimeout,
                MessageRecieveTimeout = processingConfig.MessageRecieveTimeout,
                ResponseSendingTimeout = processingConfig.ResponseSendingTimeout,
                CommandOverrides = new ReadOnlyDictionary<string, CommandProcessingConfiguration>(commandOverrrides)
            };

            return processingConfiguration;
        }

        public TimeSpan GetMessageRecieveTimeout([NotNull] string commandName)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }

            CommandProcessingConfiguration config;
            if (CommandOverrides.TryGetValue(commandName, out config))
            {
                return config.MessageRecieveTimeout ?? MessageRecieveTimeout;
            }
            return MessageRecieveTimeout;
        }

        public TimeSpan GetResponseSendingTimeout([NotNull] string commandName)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }

            CommandProcessingConfiguration config;
            if (CommandOverrides.TryGetValue(commandName, out config))
            {
                return config.ResponseSendingTimeout ?? ResponseSendingTimeout;
            }
            return ResponseSendingTimeout;
        }

        public int GetMaxMessageBodySize([NotNull] string commandName)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }

            CommandProcessingConfiguration config;
            if (CommandOverrides.TryGetValue(commandName, out config))
            {
                return config.MaxMessageBodySize ?? MaxMessageBodySize;
            }
            return MaxMessageBodySize;
        }
    }

    public sealed class CommandProcessingConfiguration
    {
        public TimeSpan? MessageRecieveTimeout { get; private set; }
        public TimeSpan? ResponseSendingTimeout { get; private set; }
        public int? MaxMessageBodySize { get; private set; }

        public static CommandProcessingConfiguration Create(CommandConfigurationElement commandConfiguration)
        {
            return new CommandProcessingConfiguration
            {
                MaxMessageBodySize = commandConfiguration.MaxMessageBodySize,
                ResponseSendingTimeout = commandConfiguration.ResponseSendingTimeout,
                MessageRecieveTimeout = commandConfiguration.MessageRecieveTimeout
            };
        }
    }
}