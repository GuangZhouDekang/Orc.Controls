﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropDownButtonViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.Controls.Example.ViewModels
{
    using Catel;
    using Catel.MVVM;
    using Catel.Services;

    public class DropDownButtonViewModel : ViewModelBase
    {
        #region Fields
        private readonly IMessageService _messageService;
        #endregion

        #region Constructors
        public DropDownButtonViewModel(IMessageService messageService)
        {
            Argument.IsNotNull(() => messageService);

            _messageService = messageService;
            DefaultAction = new Command(OnDefaultActionExecute);
        }
        #endregion

        #region Commands
        public Command DefaultAction { get; private set; }

        private void OnDefaultActionExecute()
        {
            _messageService.ShowInformationAsync("Default action has been executed");
        }
        #endregion
    }
}
