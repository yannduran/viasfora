﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Winterdom.Viasfora.Text {

  [Export(typeof(ITaggerProvider))]
  [ContentType(CSharp.ContentType)]
  [ContentType(Cpp.ContentType)]
  [ContentType(JScript.ContentType)]
  [ContentType(JScript.ContentTypeVS2012)]
  [ContentType(VB.ContentType)]
  [ContentType(FSharp.ContentType)]
  [TextViewRole(PredefinedTextViewRoles.Document)]
  [TagType(typeof(RainbowClassificationTag))]
  public class RainbowTaggerProvider : ITaggerProvider {
    [Import]
    internal IClassificationTypeRegistryService ClassificationRegistry = null;

    public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag {
      return new RainbowTagger(buffer, ClassificationRegistry) as ITagger<T>;
    }
  }

}