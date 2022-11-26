﻿using System;

namespace Keyword.Processors {
  public class MinuteProcessor : KeywordProcessor {
    public MinuteProcessor() : base("MINUTE") { }
    
    protected override string ProcessExecutor(AssetInfo assetInfo) {
      return DateTime.Now.ToString("mm");
    }
  }
}