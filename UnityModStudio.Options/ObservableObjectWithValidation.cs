using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.PlatformUI;

namespace UnityModStudio.Options;

public class ObservableObjectWithValidation : ObservableObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<object>> _errors = new();

    public IEnumerable<object> GetErrors(string? propertyName) =>
        string.IsNullOrEmpty(propertyName) ?
            _errors.Values.SelectMany(list => list) :
            _errors.TryGetValue(propertyName!, out var errors) ? errors : Enumerable.Empty<object>();

    IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName) => GetErrors(propertyName);

    public bool HasErrors => _errors.Values.Any(errors => errors.Count > 0);

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    protected void NotifyErrorsChanged([CallerMemberName] string propertyName = "")
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        NotifyPropertyChanged(nameof(HasErrors));
    }

    protected bool SetPropertyWithValidation<T>(ref T field, T newValue, Func<T, IEnumerable<object>> validator, [CallerMemberName] string propertyName = "")
    {
        if (!SetProperty(ref field, newValue, propertyName))
            return false;

        var oldErrors = GetErrors(propertyName);
        _errors[propertyName] = validator(newValue).ToList();
        if (!oldErrors.SequenceEqual(GetErrors(propertyName)))
            NotifyErrorsChanged(propertyName);

        return true;
    }

    protected void ClearAllErrors() => _errors.Clear();

    protected void ClearErrors([CallerMemberName] string propertyName = "") => _errors.Remove(propertyName);

    protected void AddError(object error, [CallerMemberName] string propertyName = "")
    {
        if (!_errors.TryGetValue(propertyName, out var errors))
            _errors[propertyName] = errors = [];

        errors.Add(error);
    }

    protected bool HasPropertyErrors([CallerMemberName] string propertyName = "") =>
        _errors.TryGetValue(propertyName, out var errors) && errors.Count > 0;
}