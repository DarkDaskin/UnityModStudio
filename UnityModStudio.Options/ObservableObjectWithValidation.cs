using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.PlatformUI;
using UnityModStudio.Common;

namespace UnityModStudio.Options;

public class ObservableObjectWithValidation : ObservableObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, ValidationInfo> _validationInfos = new();

    public IEnumerable<string> GetErrors(string? propertyName) =>
        string.IsNullOrEmpty(propertyName) ?
            _validationInfos.Values.SelectMany(vi => vi.Errors) :
            _validationInfos.TryGetValue(propertyName!, out var validationInfo) ? validationInfo.Errors : [];

    IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName) => GetErrors(propertyName);

    public bool HasErrors => _validationInfos.Values.Any(vi => vi.Errors.Count > 0);

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    protected void NotifyErrorsChanged([CallerMemberName] string propertyName = "")
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        NotifyPropertyChanged(nameof(HasErrors));
    }

    protected new bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
    {
        if (!base.SetProperty(ref field, newValue, propertyName))
            return false;

        if (!_validationInfos.ContainsKey(propertyName))
            return true;

        Validate(propertyName);
        return true;
    }

    protected bool HasPropertyErrors([CallerMemberName] string propertyName = "") => GetErrors(propertyName).Any();

    protected void AddRule<T>(Expression<Func<T>> property, ValidationRule<T> rule)
    {
        var propertyName = property.GetMemberName();
        if (!_validationInfos.TryGetValue(propertyName, out var untypedValidationInfo))
            _validationInfos.Add(propertyName, untypedValidationInfo = new ValidationInfo<T>(property.Compile()));
        var validationInfo = (ValidationInfo<T>)untypedValidationInfo;
        validationInfo.Rules.Add(rule);
    }

    protected void AddRule<T>(Expression<Func<T>> property, Func<T, bool> rule, Func<T, string> errorMessage) =>
        AddRule(property, v => !rule(v) ? errorMessage(v) : null);

    protected void AddRule<T>(Expression<Func<T>> property, Func<T, bool> rule, string errorMessage) =>
        AddRule(property, rule, _ => errorMessage);

    protected bool Validate([CallerMemberName] string propertyName = "")
    {
        if (!_validationInfos.TryGetValue(propertyName, out var validationInfo))
            throw new ArgumentException($"Property '{propertyName}' can't be validated.", nameof(propertyName));

        var oldErrors = GetErrors(propertyName).ToHashSet();
        var result = validationInfo.Validate();
        var newErrors = GetErrors(propertyName);
        if (!oldErrors.SetEquals(newErrors))
            NotifyErrorsChanged(propertyName);
        return result;
    }

    protected bool ValidateAll() => _validationInfos.Keys.Aggregate(false, (current, propertyName) => current | Validate(propertyName));


    public delegate string? ValidationRule<in T>(T value);

    private abstract class ValidationInfo
    {
        public readonly List<string> Errors = [];

        public abstract bool Validate();
    }

    private class ValidationInfo<T>(Func<T> getter) : ValidationInfo
    {
        public readonly List<ValidationRule<T>> Rules = [];

        public override bool Validate()
        {
            Errors.Clear();
            foreach (var rule in Rules)
            {
                var error = rule(getter());
                if (error != null)
                    Errors.Add(error);
            }
            return Errors.Count == 0;
        }
    }
}