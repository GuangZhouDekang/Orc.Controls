﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogFilterEditorViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.Controls
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    using Catel;
    using Catel.Data;
    using Catel.Fody;
    using Catel.MVVM;

    public class LogFilterEditorViewModel : ViewModelBase
    {
        public LogFilterEditorViewModel(LogFilter logFilter)
        {
            Argument.IsNotNull(() => logFilter);

            ExpressionTypes = new ObservableCollection<LogFilterExpressionType>(Enum<LogFilterExpressionType>.GetValues());
            Actions = new ObservableCollection<LogFilterAction>(Enum<LogFilterAction>.GetValues());
            Targets = new ObservableCollection<LogFilterTarget>(Enum<LogFilterTarget>.GetValues());

            LogFilter = logFilter;
        }

        public ObservableCollection<LogFilterAction> Actions { get; private set; }

        public ObservableCollection<LogFilterExpressionType> ExpressionTypes { get; private set; }

        public ObservableCollection<LogFilterTarget> Targets { get; private set; }

        [Model]
        public LogFilter LogFilter { get; set; }

        [ViewModelToModel]
        public string Name { get; set; }

        [ViewModelToModel]
        public LogFilterExpressionType ExpressionType { get; set; }

        [ViewModelToModel]
        public string ExpressionValue { get; set; }

        [ViewModelToModel]
        public LogFilterAction Action { get; set; }

        [ViewModelToModel]
        public LogFilterTarget Target { get; set; }

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            base.ValidateFields(validationResults);

            if (string.IsNullOrWhiteSpace(Name))
            {
                validationResults.Add(FieldValidationResult.CreateError(nameof(Name), "Name is required"));
            }

            if (string.IsNullOrWhiteSpace(ExpressionValue))
            {
                validationResults.Add(FieldValidationResult.CreateError(nameof(ExpressionValue), "Expression value is required"));
            }
        }
    }
}