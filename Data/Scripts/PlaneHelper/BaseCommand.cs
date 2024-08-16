using VRage.Game.ModAPI.Ingame.Utilities;

namespace PlaneHelper
{
    public abstract class BaseCommand
    {
        private CommandHandler _handler;
        private string[] _aliases;

        protected CommandHandler Handler => _handler;
        public string[] Aliases => _aliases;

        public BaseCommand(CommandHandler handler, params string[] aliases)
        {
            _handler = handler;
            _aliases = aliases;

            handler.AddCommand(this);
        }

        public abstract void Execute(MyCommandLine commandLine, string rawText);
    }
}
