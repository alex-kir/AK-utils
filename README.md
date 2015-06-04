# AK-utils
AK-utils is a set of lightweight utils, helpers and wrapers.
Each file has minimum dependices from other files of library.

$ DatabaseContext

db.Create("IMAGE", new[] { "image_pk" }, new[] { "bytes", "ratio" });

db.Write("IMAGE", images
  .Select(it => new Dictionary<string, object>
      {
          { "image_pk", it.Key },
          { "bytes", it.Value.Item1 },
          { "ratio", it.Value.Item2 }
      }));


using (var result = db.Execute("SELECT * FROM `IMAGE` WHERE `image_pk` = '" + image + "';"))
{
  var row = result.FirstOrDefault();
  if (row != null)
    return row["bytes"] as byte[];
}
