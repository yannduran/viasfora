﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Utilities;
using Winterdom.Viasfora.Contracts;
using Winterdom.Viasfora.Languages.CommentParsers;
using Winterdom.Viasfora.Rainbow;
using Winterdom.Viasfora.Util;

namespace Winterdom.Viasfora.Languages {
  public abstract class LanguageInfo : ILanguage {
    private static StringComparer comparer = StringComparer.CurrentCultureIgnoreCase;
    protected static readonly String[] EMPTY = { };
    public IVsfSettings Settings { get; private set; }

    public String[] ControlFlow {
      get { return Get("ControlFlow", ControlFlowDefaults); }
      set { Set("ControlFlow", value); }
    }
    public String[] Linq {
      get { return Get("Linq", LinqDefaults); }
      set { Set("Linq", value); }
    }
    public String[] Visibility {
      get { return Get("Visibility", VisibilityDefaults); }
      set { Set("Visibility", value); }
    }
    public bool Enabled {
      get { return Settings.GetBoolean(KeyName + "_Enabled", true); }
      set { Settings.SetValue(KeyName + "_Enabled", value); }
    }

    public LanguageInfo(IVsfSettings settings) {
      this.Settings = settings;
    }

    public abstract IBraceExtractor NewBraceExtractor();
    public virtual IFirstLineCommentParser NewFirstLineCommentParser() {
      return new GenericCommentParser();
    }
    public virtual IStringScanner NewStringParser(String text) {
      return null;
    }

    public virtual bool MatchesContentType(IContentType contentType) {
      foreach ( String str in this.SupportedContentTypes ) {
        if ( contentType.IsOfType(str) ) 
          return true;
      }
      return false;
    }

    public bool IsControlFlowKeyword(String text) {
      return ControlFlow.Contains(TextToCompare(text), comparer);
    }
    public bool IsVisibilityKeyword(String text) {
      return Visibility.Contains(TextToCompare(text), comparer);
    }
    public bool IsLinqKeyword(String text) {
      return Linq.Contains(TextToCompare(text), comparer);
    }

    protected virtual String TextToCompare(String text) {
      return text;
    }

    protected abstract String[] SupportedContentTypes { get; }
    protected abstract String[] ControlFlowDefaults { get; }
    protected abstract String[] LinqDefaults { get; }
    protected abstract String[] VisibilityDefaults { get; }
    public abstract String KeyName { get; }

    protected String[] Get(String name, String[] defaults) {
      String[] values = Settings.GetValue(this.KeyName + "_" + name, null).AsList();
      if ( values == null || values.Length == 0 )
        values = defaults;
      return values;
    }
    protected void Set(String name, IEnumerable<String> values) {
      Settings.SetValue(this.KeyName + "_" + name, values.FromList());
    }
  }
}
