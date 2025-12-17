using System.Windows;
using System.Windows.Controls;

namespace UnityModStudio.Options;

public class ContentControlWithValidation : ContentControl
{
    public static readonly DependencyPropertyKey ErrorVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ErrorVisibility), typeof(Visibility), typeof(ContentControlWithValidation), new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty ErrorVisibilityProperty = ErrorVisibilityPropertyKey.DependencyProperty;

    public Visibility ErrorVisibility
    {
        get => (Visibility)GetValue(ErrorVisibilityProperty);
        set => SetValue(ErrorVisibilityPropertyKey, value);
    }

    public static readonly DependencyPropertyKey ErrorTextPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ErrorText), typeof(string), typeof(ContentControlWithValidation), new PropertyMetadata());

    public static readonly DependencyProperty ErrorTextProperty = ErrorTextPropertyKey.DependencyProperty;

    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextPropertyKey, value);
    }
    static ContentControlWithValidation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentControlWithValidation), new FrameworkPropertyMetadata(typeof(ContentControlWithValidation)));
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        if (oldContent is FrameworkElement oldElement) 
            Validation.RemoveErrorHandler(oldElement, OnError);
        if (newContent is FrameworkElement newElement) 
            Validation.AddErrorHandler(newElement, OnError);
    }

    private void OnError(object sender, ValidationErrorEventArgs e)
    {
        ErrorVisibility = e.Action == ValidationErrorEventAction.Added ? Visibility.Visible : Visibility.Collapsed;
        ErrorText = e.Error.ErrorContent.ToString();
    }
}