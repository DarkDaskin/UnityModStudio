namespace UnityModStudio.Options.Tests;

[TestClass]
public sealed class ObservableObjectWithValidationTests
{
    [TestMethod]
    public void WhenCreated_NoErrors()
    {
        var vm = new TestViewModel();

        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(TestViewModel.PositiveInt)).SequenceEqual([]));
        Assert.IsTrue(vm.GetErrors(nameof(TestViewModel.ShortString)).SequenceEqual([]));
        Assert.IsTrue(vm.GetErrors(null).SequenceEqual([]));
        Assert.IsTrue(vm.GetErrors("").SequenceEqual([]));
    }

    [TestMethod]
    public void WhenSetToCorrectValues_NoErrorsAndNotifyChanges()
    {
        var (vm, notifiedProperties, notifiedPropertyErrors) = SetupViewModelWithEvents();

        vm.PositiveInt = 42;
        vm.ShortString = ":)";

        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(TestViewModel.PositiveInt)).SequenceEqual([]));
        Assert.IsTrue(vm.GetErrors(nameof(TestViewModel.ShortString)).SequenceEqual([]));
        Assert.IsTrue(vm.GetErrors(null).SequenceEqual([]));
        Assert.IsTrue(vm.GetErrors("").SequenceEqual([]));
        Assert.IsTrue(notifiedProperties.SequenceEqual([nameof(TestViewModel.PositiveInt), nameof(TestViewModel.ShortString)]));
        Assert.IsTrue(notifiedPropertyErrors.SequenceEqual([]));
    }

    [TestMethod]
    public void WhenSetToIncorrectValues_ProduceErrorsAndNotify()
    {
        var (vm, notifiedProperties, notifiedPropertyErrors) = SetupViewModelWithEvents();

        vm.PositiveInt = -1;
        vm.ShortString = "LongString";

        Assert.IsTrue(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(TestViewModel.PositiveInt)).SequenceEqual(["PositiveInt must be positive."]));
        Assert.IsTrue(vm.GetErrors(nameof(TestViewModel.ShortString)).SequenceEqual(["ShortString must not be longer than 5 characters."]));
        Assert.IsTrue(vm.GetErrors(null).SequenceEqual(["PositiveInt must be positive.", "ShortString must not be longer than 5 characters."]));
        Assert.IsTrue(vm.GetErrors("").SequenceEqual(["PositiveInt must be positive.", "ShortString must not be longer than 5 characters."]));
        Assert.IsTrue(notifiedProperties.SequenceEqual([nameof(TestViewModel.PositiveInt), nameof(TestViewModel.HasErrors), nameof(TestViewModel.ShortString), nameof(TestViewModel.HasErrors)]));
        Assert.IsTrue(notifiedPropertyErrors.SequenceEqual([nameof(TestViewModel.PositiveInt), nameof(TestViewModel.ShortString)]));
    }

    [TestMethod]
    public void WhenSetToIncorrectThenCorrectValue_NoErrorsAndNotify()
    {
        var (vm, notifiedProperties, notifiedPropertyErrors) = SetupViewModelWithEvents();

        vm.PositiveInt = -1;
        vm.PositiveInt = 42;

        Assert.IsFalse(vm.HasErrors);
        Assert.IsTrue(vm.GetErrors(nameof(TestViewModel.PositiveInt)).SequenceEqual([]));
        Assert.IsTrue(vm.GetErrors(nameof(TestViewModel.ShortString)).SequenceEqual([]));
        Assert.IsTrue(vm.GetErrors(null).SequenceEqual([]));
        Assert.IsTrue(vm.GetErrors("").SequenceEqual([]));
        Assert.IsTrue(notifiedProperties.SequenceEqual([nameof(TestViewModel.PositiveInt), nameof(TestViewModel.HasErrors), nameof(TestViewModel.PositiveInt), nameof(TestViewModel.HasErrors)]));
        Assert.IsTrue(notifiedPropertyErrors.SequenceEqual([nameof(TestViewModel.PositiveInt), nameof(TestViewModel.PositiveInt)]));
    }

    private static (TestViewModel, List<string?>, List<string?>) SetupViewModelWithEvents()
    {
        var notifiedProperties = new List<string?>();
        var notifiedPropertyErrors = new List<string?>();
        var vm = new TestViewModel();
        vm.PropertyChanged += (sender, args) => notifiedProperties.Add(args.PropertyName);
        vm.ErrorsChanged += (sender, args) => notifiedPropertyErrors.Add(args.PropertyName);
        return (vm, notifiedProperties, notifiedPropertyErrors);
    }


    private class TestViewModel : ObservableObjectWithValidation
    {
        private int _positiveInt = 1;
        private string _shortString = "";

        public int PositiveInt
        {
            get => _positiveInt;
            set => SetPropertyWithValidation(ref _positiveInt, value, 
                v => v > 0 ? [] : [$"{nameof(PositiveInt)} must be positive."]);
        }

        public string ShortString
        {
            get => _shortString;
            set => SetPropertyWithValidation(ref _shortString, value,
                v => v.Length <= 5 ? [] : [$"{nameof(ShortString)} must not be longer than 5 characters."]);
        }
    }
}
