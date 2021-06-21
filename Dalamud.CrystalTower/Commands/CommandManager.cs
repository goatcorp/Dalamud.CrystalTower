using Dalamud.CrystalTower.Commands.Attributes;
using Dalamud.CrystalTower.DependencyInjection.Extensions;
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
        protected readonly DalamudPluginInterface PluginInterface;
        protected readonly IDictionary<Type, IList<RegisteredCommandInfo>> PluginCommands;
        protected readonly IList<object> ModuleInstances;

        protected readonly IServiceProvider ServiceProvider;

        public CommandManager(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            PluginCommands = new Dictionary<Type, IList<RegisteredCommandInfo>>();
            ModuleInstances = new List<object>();
        }

        public CommandManager(DalamudPluginInterface pluginInterface, IServiceProvider serviceProvider) : this(pluginInterface)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Installs commands from the provided command module type into the plugin interface and hydrates the resulting
        /// module instance with any applicable service implementations.
        /// </summary>
        /// <param name="commandModule">The command module type to install commands from.</param>
        public void AddCommandModule(Type commandModule)
        {
            var instance = Activator.CreateInstance(commandModule);
            ServiceProvider?.InjectInto(instance);

            var newPluginCommands = commandModule.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(method => method.GetCustomAttribute<CommandAttribute>() != null)
                .SelectMany(methodInfo => BuildRegisteredCommandInfo(instance, methodInfo))
                .ToList();

            foreach (var registeredCommandInfo in newPluginCommands)
            {
                PluginInterface.CommandManager.AddHandler(registeredCommandInfo.Name, registeredCommandInfo.Command);
            }

            PluginCommands[commandModule] = newPluginCommands;
            ModuleInstances.Add(instance);
        }

        /// <summary>
        /// Installs commands from the provided command module type into the plugin interface.
        /// </summary>
        /// <typeparam name="TCommandModule">The command module type to install commands from.</typeparam>
        public void AddCommandModule<TCommandModule>()
            => AddCommandModule(typeof(TCommandModule));

        /// <summary>
        /// Retrieves the command module registered under the provided type, or throws an exception
        /// if the module was not registered.
        /// </summary>
        /// <typeparam name="TCommandModule">The type of the module instance to retrieve.</typeparam>
        /// <returns>The module instance.</returns>
        public TCommandModule GetCommandModule<TCommandModule>()
        {
            return (TCommandModule)ModuleInstances.First(inst => inst is TCommandModule);
        }

        /// <summary>
        /// Uninstalls commands from the provided command module type into the plugin interface.
        /// </summary>
        /// <param name="commandModule">The command module type to uninstall commands from.</param>
        public void RemoveCommandModule(Type commandModule)
        {
            foreach (var registeredCommandInfo in PluginCommands[commandModule])
            {
                PluginInterface.CommandManager.RemoveHandler(registeredCommandInfo.Name);
            }

            PluginCommands.Remove(commandModule);
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
        public virtual void Dispose()
        {
            var moduleTypes = PluginCommands.Keys.ToList(); // Duplicate the list to avoiding modifying the collection during this loop
            foreach (var moduleType in moduleTypes)
            {
                RemoveCommandModule(moduleType);
            }

            foreach (var instance in ModuleInstances)
            {
                if (instance is IDisposable disposableInstance)
                {
                    disposableInstance.Dispose();
                }
            }
        }

        protected class RegisteredCommandInfo
        {
            public string Name { get; set; }

            public CommandInfo Command { get; set; }
        }
    }
}