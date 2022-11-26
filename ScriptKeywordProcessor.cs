using System.IO;
using System.Linq;
using Keyword.Processors;
using UnityEditor;
using UnityEngine;

namespace Keyword {
  internal sealed class ScriptKeywordProcessor : AssetModificationProcessor {
    // string that identifies a file as a meta file.
    private const string MetaIdentifier = ".meta";

    // Extensions that this processor should act upon.
    private static readonly string[] ValidExtensions = { ".cs", ".txt", ".shader", ".compute", ".asmdef", ".raytrace" };

    // active keyword processors.
    private static readonly KeywordProcessor[] KeywordProcessors = {
      new CopyrightProcessor(),
      new DateProcessor(),
      new DayProcessor(),
      new DirPathProcessor(),
      new FileNameProcessor(),
      new HourProcessor(),
      new MinuteProcessor(),
      new MonthNameFullProcessor(),
      new MonthNameShortProcessor(),
      new MonthProcessor(),
      new NamespaceProcessor(),
      new ProductNameProcessor(),
      new ProductVersionProcessor(),
      new ProjectNameProcessor(),
      new SecondProcessor(),
      new TimeProcessor(),
      new UserProcessor(),
      new YearProcessor()
    };

    private static void OnWillCreateAsset(string assetName) {
      // Remove meta extension if present
      assetName = assetName.Replace(MetaIdentifier, "");

      var assetInfo = new AssetInfo(assetName);
      
      // if invalid extension, ignore
      if (!ValidExtensions.Contains(assetInfo.FileExtension)) {
        return;
      }
      
      // if target file does not exist, write warning
      if (!File.Exists(assetInfo.FilePath)) {
        Debug.LogWarning($"Templating failure on asset {assetName}: non-meta file does not exist.");
      }
      
      // if temp file already exists, write warning
      if (File.Exists(assetInfo.TempFilePath)) {
        Debug.LogWarning($"Templating failure on asset {assetName}: temporary file already exists.");
      }

      // create read and write streams
      using (var input = File.OpenText(assetInfo.FilePath))
      using (var output = new StreamWriter(assetInfo.TempFilePath)) {
        var line = input.ReadLine();

        while (line is not null) {
          // run all processors over the line
          line = KeywordProcessors.Aggregate(line, (current, processor) => processor.Process(assetInfo, current));

          // write to the temp file and get the next line
          output.WriteLine(line);
          line = input.ReadLine();
        }
      }
      
      // move temp file to final file location
      File.Replace(assetInfo.TempFilePath, assetInfo.FilePath, null);
      
      AssetDatabase.Refresh();
    }
  }
}