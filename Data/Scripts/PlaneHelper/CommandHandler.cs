using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace PlaneHelper
{
    public class CommandHandler : IDisposable
    {
        private PlaneHelperMod _mod;
        private bool _commandsRegistered = false;
        private Dictionary<string, BaseCommand> _commands = new Dictionary<string, BaseCommand>(StringComparer.InvariantCultureIgnoreCase);

        public PlaneHelperMod Mod => _mod;

        public CommandHandler(PlaneHelperMod mod)
        {
            _mod = mod;
        }

        public void Init()
        {
            if (_commandsRegistered)
                return;
            _commandsRegistered = true;

            new HelpCommand(this);
            new OverlayCommand(this);

            MyAPIGateway.Utilities.MessageEnteredSender += OnMessageEnteredSender;
        }

        public void AddCommand(BaseCommand command)
        {
            foreach (var name in command.Aliases)
            {
                _commands.Add(name, command);
            }
        }

        public void ShowMessage(string text)
        {
            MyAPIGateway.Utilities.ShowMessage(PlaneHelperMod.MOD_NAME, text);
        }

        private void OnMessageEnteredSender(ulong sender, string messageText, ref bool sendToOthers)
        {
            if (string.IsNullOrEmpty(messageText))
                return;

            string[] commandPrefixes = new string[] { "/planeHelper", "/ph" };
            foreach (var prefix in commandPrefixes)
            {
                if (messageText.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    OnCommandEntered(messageText.Substring(prefix.Length).Trim());
                    sendToOthers = false;
                    break;
                }
            }
        }

        private void OnCommandEntered(string text)
        {
            MyCommandLine commandLine = new MyCommandLine();
            if (string.IsNullOrEmpty(text) || !commandLine.TryParse(text) || commandLine.ArgumentCount == 0)
            {
                ShowHelp();
                return;
            }

            if (_commands.ContainsKey(commandLine.Argument(0)))
            {
                _commands[commandLine.Argument(0)].Execute(commandLine, text);
            }
        }

        private void ShowHelp()
        {
            MyCommandLine commandLine = new MyCommandLine();

            if (_commands.ContainsKey("help"))
            {
                _commands["help"].Execute(commandLine, "");
            }
        }

        public void Dispose()
        {
            if (!_commandsRegistered)
                return;
            _commandsRegistered = false;

            MyAPIGateway.Utilities.MessageEnteredSender -= OnMessageEnteredSender;
        }
    }
}
