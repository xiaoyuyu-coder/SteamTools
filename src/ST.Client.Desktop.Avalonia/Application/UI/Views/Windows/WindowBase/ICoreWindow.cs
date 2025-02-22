using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Application.UI.Views.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APoint = Avalonia.Point;
using FluentAvalonia.UI.Controls.Primitives;

// ReSharper disable once CheckNamespace
namespace Avalonia.Controls
{
    internal partial interface ICoreWindow
    {
        Window Window { get; }

        public bool IsHideWindow { get; set; }

        MinMaxCloseControl? SystemCaptionButtons { get; }

        public void Resized(Size clientSize, PlatformResizeReason reason);

        bool HitTestTitleBarRegion(APoint windowPoint)
        {
            return SystemCaptionButtons?.HitTestCustom(windowPoint) ?? false;
        }

        bool HitTestCaptionButtons(APoint pos)
        {
            if (pos.Y < 1)
                return false;

            var result = SystemCaptionButtons?.HitTestCustom(pos) ?? false;
            return result;
        }

        //bool HitTestMaximizeButton(APoint pos)
        //{
        //    return SystemCaptionButtons?.HitTestMaxButton(pos) ?? default;
        //}

        //void FakeMaximizeHover(bool hover)
        //{
        //    SystemCaptionButtons?.FakeMaximizeHover(hover);
        //}

        //void FakeMaximizePressed(bool pressed)
        //{
        //    SystemCaptionButtons?.FakeMaximizePressed(pressed);
        //}

        //void FakeMaximizeClick()
        //{
        //    SystemCaptionButtons?.FakeMaximizeClick();
        //}
    }

    partial interface ICoreWindow : IStyleable, IAvaloniaObject, INamed, IFocusScope, ILayoutRoot, ILayoutable, IVisual
    {

    }
}
