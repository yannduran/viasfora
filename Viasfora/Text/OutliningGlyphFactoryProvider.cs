﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Winterdom.Viasfora.Tags;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;

namespace Winterdom.Viasfora.Text {
  [Export(typeof(IGlyphFactoryProvider))]
  [Export(typeof(IGlyphMouseProcessorProvider))]
  [ContentType("Text")]
  [TagType(typeof(OutliningGlyphTag))]
  [Name("VsfOutliningGlyph")]
  public class OutliningGlyphFactoryProvider : IGlyphFactoryProvider, IGlyphMouseProcessorProvider {

    [Import]
    internal IBufferTagAggregatorFactoryService aggregatorFactory = null;
    public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin) {
      return new OutliningGlyphFactory();
    }
    public IMouseProcessor GetAssociatedMouseProcessor(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin margin) {
      return new GlyphMouseProcessor(
        wpfTextViewHost, margin, 
        aggregatorFactory.CreateTagAggregator<IGlyphTag>(wpfTextViewHost.TextView.TextBuffer)
        );
    }


    class OutliningGlyphFactory : IGlyphFactory {
      public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag) {
        OutliningGlyphTag ourTag = tag as OutliningGlyphTag;
        if ( ourTag == null ) {
          return null;
        }
        const double minSize = 16.0;
        double size = line != null
          ? Math.Min(minSize, line.TextHeight)
          : minSize;
        TextBlock tb = CreateGlyphElement(minSize);
        return tb;
      }

      private TextBlock CreateGlyphElement(double minSize) {
        TextBlock tb = new TextBlock();
        tb.Text = "U";
        tb.FontWeight = FontWeights.Bold;
        tb.Height = tb.Width = minSize;
        tb.TextAlignment = TextAlignment.Center;
        tb.VerticalAlignment = VerticalAlignment.Center;
        tb.Foreground = Brushes.White;

        Rectangle rect = new Rectangle();
        rect.Height = rect.Width = minSize * 0.9;
        rect.Stroke = Brushes.Transparent;
        rect.Fill = new SolidColorBrush(Color.FromRgb(90,90,90));
        rect.RadiusX = rect.RadiusY = rect.Height * 0.1;

        tb.Background = new VisualBrush(rect);
        return tb;
      }
    }

    class GlyphMouseProcessor : MouseProcessorBase {
      private IWpfTextViewHost theHost;
      private IWpfTextViewMargin theMargin;
      private ITagAggregator<IGlyphTag> tagAggregator;
      Point clickPos;
      public GlyphMouseProcessor(IWpfTextViewHost host, IWpfTextViewMargin margin, ITagAggregator<IGlyphTag> aggregator) {
        this.theHost = host;
        this.theMargin = margin;
        this.tagAggregator = aggregator;
        this.theHost.Closed += OnTextViewHostClosed;
      }
      public GlyphMouseProcessor() {
      }
      public override void PreprocessMouseLeftButtonDown(MouseButtonEventArgs e) {
        clickPos = GetLocation(e);
      }
      public override void PostprocessMouseLeftButtonUp(MouseButtonEventArgs e) {
        Point pos = GetLocation(e);
        var oLine = GetLineAt(clickPos);
        var cLine = GetLineAt(pos);
        if ( oLine == cLine && cLine != null ) {
          SnapshotSpan span = new SnapshotSpan(cLine.Start, cLine.End);
          var spans = new NormalizedSnapshotSpanCollection(span);
          foreach ( var tag in tagAggregator.GetTags(spans) ) {
            if ( !(tag.Tag is OutliningGlyphTag) ) continue;
            var realSpan = tag.GetSpan(cLine.Snapshot);
            if ( realSpan.IsEmpty ) continue;
            RemoveOutlineAt(realSpan.Start);
            break;
          }
        }
      }

      private void RemoveOutlineAt(SnapshotPoint snapshotPoint) {
        var textBuffer = this.theHost.TextView.TextBuffer;
        IUserOutlining outlining = OutliningManager.Get(textBuffer);
        if ( outlining != null ) {
          outlining.RemoveAt(snapshotPoint);
        }
      }
      private ITextViewLine GetLineAt(Point pos) {
        return this.theHost.TextView.TextViewLines.GetTextViewLineContainingYCoordinate(pos.Y);
      }
      private Point GetLocation(MouseButtonEventArgs e) {
        Point location = e.GetPosition(this.theHost.TextView.VisualElement);
        location.X += theHost.TextView.ViewportLeft;
        location.Y += theHost.TextView.ViewportTop;
        return location;
      }
      private void OnTextViewHostClosed(object sender, EventArgs e) {
        if ( this.tagAggregator != null ) {
          this.tagAggregator.Dispose();
          this.tagAggregator = null;
        }
        this.theHost.Closed -= OnTextViewHostClosed;
      }
    }

  }
}
