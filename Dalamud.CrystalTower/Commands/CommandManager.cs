using Dalamud.CrystalTower.Commands.Attributes;
using Dalamud.CrystalTower.DependencyInjection;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dalamud.CrystalTower.Commands
{
    public class CommandManager : IDisposable
    {
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly IDictionary<Type, IList<RegisteredCommandInfo>> _pluginCommands;
        private readonly IList<object> _moduleInstances;

        private readonly PluginServiceCollection _serviceCollection;

        public CommandManager(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
            _pluginCommands = new Dictionary<Type, IList<RegisteredCommandInfo>>();
            _moduleInstances = new List<object>();
        }

        public CommandManager(DalamudPluginInterface pluginInterface, PluginServiceCollection serviceCollection) : this(pluginInterface)
        {
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Installs commands from the provided command module type into the plugin interface and hydrates the resulting
        /// module instance with any applicable service implementations.
        /// </summary>
        /// <param name="commandModule">The command module type to install commands from.</param>
        public void AddCommandModule(Type commandModule)
        {
            var instance = Activator.CreateInstance(commandModule);
            _serviceCollection?.InjectInto(instance);

            var newPluginCommands = commandModule.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
                .SelectMany(methodInfo => BuildRegisteredCommandInfo(instance, methodInfo))
                .ToList();

            foreach (var registeredCommandInfo in newPluginCommands)
            {
                _pluginInterface.CommandManager.AddHandler(registeredCommandInfo.Name, registeredCommandInfo.Command);
            }

            _pluginCommands[commandModule] = newPluginCommands;
            _moduleInstances.Add(instance);
        }

        /// <summary>
        /// Installs commands from the provided command module type into the plugin interface.
        /// </summary>
        /// <typeparam name="TCommandModule">The command module type to install commands from.</typeparam>
        public void AddCommandModule<TCommandModule>()
            => AddCommandModule(typeof(TCommandModule));

        /// <summary>
        /// Uninstalls commands from the provided command module type into the plugin interface.
        /// </summary>
        /// <param name="commandModule">The command module type to uninstall commands from.</param>
        public void RemoveCommandModule(Type commandModule)
        {
            foreach (var registeredCommandInfo in _pluginCommands[commandModule])
            {
                _pluginInterface.CommandManager.RemoveHandler(registeredCommandInfo.Name);
            }

            _pluginCommands.Remove(commandModule);
        }

        /// <summary>
        /// Uninstalls commands from the provided command module type into the plugin interface.
        /// </summary>
        /// <typeparam name="TCommandModule">The command module type to uninstall commands from.</typeparam>
        public void RemoveCommandModule<TCommandModule>()
            => RemoveCommandModule(typeof(TCommandModule));

        /// <summary>
        /// Builds <see cref="CommandInfo"/> instances from the attributes attached to a method.
        /// </summary>
        /// <param name="moduleInstance"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static IEnumerable<RegisteredCommandInfo> BuildRegisteredCommandInfo(object moduleInstance, MethodInfo method)
        {
            var handlerDelegate = (CommandInfo.HandlerDelegate)Delegate.CreateDelegate(typeof(CommandInfo.HandlerDelegate), moduleInstance, method);

            var command = handlerDelegate.Method.GetCustomAttribute<CommandAttribute>();
            var aliases = handlerDelegate.Method.GetCustomAttribute<AliasesAttribute>();
            var helpMessage = handlerDelegate.Method.GetCustomAttribute<HelpMessageAttribute>();
            var doNotShowInHelp = handlerDelegate.Method.GetCustomAttribute<DoNotShowInHelpAttribute>();

            var commandInfo = new CommandInfo(handlerDelegate)
            {
                HelpMessage = helpMessage?.HelpMessage ?? string.Empty,
                ShowInHelp = doNotShowInHelp == null,
            };

            // Create list of objects that will be filled with one object per alias, in addition to the base command object.
            var registeredCommandInfos = new List<RegisteredCommandInfo> { new RegisteredCommandInfo { Name = command.Command, Command = commandInfo } };

            // ReSharper disable once InvertIf
            if (aliases != null)
            {
                registeredCommandInfos.AddRange(aliases.Aliases.Select(alias => new RegisteredCommandInfo
                {
                    Name = alias,
                    Command = commandInfo,
                }));
            }

            return registeredCommandInfos;
        }

        /// <summary>
        /// Removes all commands from the plugin interface and frees any command modules implementing <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            foreach (var moduleType in _pluginCommands.Keys)
            {
                RemoveCommandModule(moduleType);
            }

            foreach (var instance in _moduleInstances)
            {
                if (instance is IDisposable disposableInstance)
                {
                    disposableInstance.Dispose();
                }
            }
        }

        private class RegisteredCommandInfo
        {
            public string Name { get; set; }

            public CommandInfo Command { get; set; }
        }
    }
}