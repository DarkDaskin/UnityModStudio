using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options
{
    [ComVisible(true), Guid("554424E6-C720-4AD5-9F67-4ADE786296CF")]
    public sealed class GameRegistryPage : UIElementDialogPage
    {
        public const string CategoryName = "Unity Mod Studio";
        public const string PageName = "Games";
        
        private readonly Lazy<GameRegistryView> _child;

        protected override UIElement Child => _child.Value;

        private IComponentModel ComponentModel => (IComponentModel) GetService(typeof(SComponentModel))!;
        private IGameRegistry GameRegistry => ComponentModel.GetService<IGameRegistry>();
        
        public GameRegistryPage()
        {
            _child = new Lazy<GameRegistryView>(CreateGameRegistryView);
        }

        private GameRegistryView CreateGameRegistryView()
        {
            var viewModel = new GameRegistryViewModel();
            ComponentModel.DefaultCompositionService.SatisfyImportsOnce(viewModel);
            return new GameRegistryView(viewModel);
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);

            ThreadHelper.JoinableTaskFactory.Run(GameRegistry.LoadSafeAsync);

            _child.Value.ViewModel.LoadGames();
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            ThreadHelper.JoinableTaskFactory.Run(GameRegistry.SaveSafeAsync);
        }
    }
}
