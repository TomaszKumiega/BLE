using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ERGBLE.ViewModels.Commands.Builders
{
    public class CommandsBuilder<TCommand, TViewModel>
        where TCommand : ICommand<TViewModel>, new()
    {
        private TCommand Command { get; }

        public CommandsBuilder(TCommand command)
        {
            Command = command;
        }

        public CommandsBuilder<TCommand, TViewModel> WithViewModel(TViewModel viewModel)
        {
            Command.ViewModel = viewModel;

            return this;
        }

        public TCommand Build()
        {
            return Command;
        }
    }
}
